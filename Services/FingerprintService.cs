using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using libzkfpcsharp;
using Models;

namespace Services
{
    public class HuellaCapturaEventArgs : EventArgs
    {
        public byte[] Template { get; set; } = Array.Empty<byte>();
        public int TemplateSize { get; set; }
        public byte[]? ImagenHuella { get; set; }
        public int ImagenAncho { get; set; }
        public int ImagenAlto { get; set; }
    }

    public class FingerprintService : IDisposable
    {
        private const int MAX_TEMPLATE_SIZE = 2048;
        private const int CAPTURE_TIMEOUT_SECONDS = 15;

        private IntPtr _deviceHandle = IntPtr.Zero;
        private IntPtr _dbHandle = IntPtr.Zero;
        private int _imageWidth = 0;
        private int _imageHeight = 0;

        private bool _isInitialized = false;
        private bool _isCapturing = false;
        private bool _disposed = false;

        public event EventHandler<HuellaCapturaEventArgs>? OnHuellaCapturada;
        public event EventHandler<string>? OnError;
        public event EventHandler<string>? OnEstadoCambiado;

        // ===================================================================
        // INICIALIZAR SENSOR
        // ===================================================================
        public bool Inicializar()
        {
            try
            {
                int ret = zkfp2.Init();
                if (ret != zkfp.ZKFP_ERR_OK && ret != zkfp.ZKFP_ERR_ALREADY_INIT)
                {
                    OnError?.Invoke(this, $"Error al inicializar SDK. Codigo: {ret}");
                    return false;
                }

                int deviceCount = zkfp2.GetDeviceCount();
                if (deviceCount <= 0)
                {
                    OnError?.Invoke(this, "No se detecto ningun sensor ZK4500");
                    zkfp2.Terminate();
                    return false;
                }

                _deviceHandle = zkfp2.OpenDevice(0);
                if (_deviceHandle == IntPtr.Zero)
                {
                    OnError?.Invoke(this, "No se pudo abrir el sensor ZK4500");
                    zkfp2.Terminate();
                    return false;
                }

                _dbHandle = zkfp2.DBInit();
                if (_dbHandle == IntPtr.Zero)
                {
                    OnError?.Invoke(this, "Error al crear cache de algoritmo");
                    zkfp2.CloseDevice(_deviceHandle);
                    zkfp2.Terminate();
                    return false;
                }

                // Obtener ancho de imagen (parametro 1)
                byte[] paramValue = new byte[4];
                int size = 4;
                zkfp2.GetParameters(_deviceHandle, 1, paramValue, ref size);
                zkfp2.ByteArray2Int(paramValue, ref _imageWidth);

                // Obtener alto de imagen (parametro 2)
                size = 4;
                zkfp2.GetParameters(_deviceHandle, 2, paramValue, ref size);
                zkfp2.ByteArray2Int(paramValue, ref _imageHeight);

                _isInitialized = true;
                OnEstadoCambiado?.Invoke(this, $"ZK4500 conectado ({_imageWidth}x{_imageHeight})");
                return true;
            }
            catch (DllNotFoundException)
            {
                OnError?.Invoke(this, "No se encontro libzkfpcsharp.dll. Instala el SDK de ZKTeco.");
                return false;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"Error: {ex.Message}");
                return false;
            }
        }

        // ===================================================================
        // CAPTURAR UNA HUELLA (una sola captura, para verificacion)
        // ===================================================================
        public async Task<HuellaCapturaEventArgs?> CapturarHuella()
        {
            if (!_isInitialized)
            {
                OnError?.Invoke(this, "Sensor no inicializado");
                return null;
            }

            if (_isCapturing)
            {
                OnError?.Invoke(this, "Ya hay una captura en proceso");
                return null;
            }

            _isCapturing = true;
            OnEstadoCambiado?.Invoke(this, "Coloque el dedo en el sensor...");

            try
            {
                return await Task.Run(() =>
                {
                    byte[] imgBuffer = new byte[_imageWidth * _imageHeight];
                    byte[] template = new byte[MAX_TEMPLATE_SIZE];
                    int templateSize = MAX_TEMPLATE_SIZE;

                    var timeout = DateTime.Now.AddSeconds(CAPTURE_TIMEOUT_SECONDS);

                    while (DateTime.Now < timeout && _isCapturing)
                    {
                        // FIX: Reiniciar templateSize en cada iteracion
                        templateSize = MAX_TEMPLATE_SIZE;

                        int ret = zkfp2.AcquireFingerprint(
                            _deviceHandle, imgBuffer, template, ref templateSize);

                        if (ret == zkfp.ZKFP_ERR_OK)
                        {
                            var args = new HuellaCapturaEventArgs
                            {
                                Template = template.Take(templateSize).ToArray(),
                                TemplateSize = templateSize,
                                ImagenHuella = imgBuffer.ToArray(),
                                ImagenAncho = _imageWidth,
                                ImagenAlto = _imageHeight
                            };

                            _isCapturing = false;
                            OnHuellaCapturada?.Invoke(this, args);
                            OnEstadoCambiado?.Invoke(this, "Huella capturada correctamente");
                            return args;
                        }

                        Thread.Sleep(200);
                    }

                    _isCapturing = false;

                    if (DateTime.Now >= timeout)
                        OnEstadoCambiado?.Invoke(this, "Tiempo agotado. Intente nuevamente.");

                    return null;
                });
            }
            catch (Exception ex)
            {
                _isCapturing = false;
                OnError?.Invoke(this, $"Error en captura: {ex.Message}");
                return null;
            }
        }

        // ===================================================================
        // REGISTRAR HUELLA (3 capturas + DBMerge)
        // El SDK requiere 3 capturas del mismo dedo para crear
        // un template de registro de alta calidad
        // ===================================================================
        public async Task<HuellaCapturaEventArgs?> RegistrarHuella()
        {
            if (!_isInitialized)
            {
                OnError?.Invoke(this, "Sensor no inicializado");
                return null;
            }

            if (_isCapturing)
            {
                OnError?.Invoke(this, "Ya hay una captura en proceso");
                return null;
            }

            byte[][] temps = new byte[3][];

            for (int i = 0; i < 3; i++)
            {
                OnEstadoCambiado?.Invoke(this, $"Captura {i + 1} de 3 — Coloque el dedo...");

                _isCapturing = true;
                byte[] imgBuf = new byte[_imageWidth * _imageHeight];
                byte[] tpl = new byte[MAX_TEMPLATE_SIZE];
                int tplSize = MAX_TEMPLATE_SIZE;
                bool captured = false;

                var timeoutDate = DateTime.Now.AddSeconds(CAPTURE_TIMEOUT_SECONDS);

                try
                {
                    await Task.Run(() =>
                    {
                        while (DateTime.Now < timeoutDate && _isCapturing)
                        {
                            // FIX: Reiniciar tplSize en cada iteracion
                            tplSize = MAX_TEMPLATE_SIZE;

                            int ret = zkfp2.AcquireFingerprint(
                                _deviceHandle, imgBuf, tpl, ref tplSize);

                            if (ret == zkfp.ZKFP_ERR_OK)
                            {
                                captured = true;
                                _isCapturing = false;
                                break;
                            }
                            Thread.Sleep(200);
                        }
                    });
                }
                catch (Exception ex)
                {
                    _isCapturing = false;
                    OnError?.Invoke(this, $"Error en captura {i + 1}: {ex.Message}");
                    return null;
                }

                if (!captured)
                {
                    _isCapturing = false;
                    OnError?.Invoke(this, $"Fallo captura {i + 1}. Intente nuevamente.");
                    EncenderLuzRoja();
                    await Task.Delay(1000);
                    ApagarLuces();
                    return null;
                }

                temps[i] = tpl.Take(tplSize).ToArray();

                EncenderLuzVerde();
                OnEstadoCambiado?.Invoke(this, $"Captura {i + 1} de 3 — OK. Retire el dedo.");

                // Esperar a que retire el dedo antes de la siguiente captura
                await Task.Delay(2000);
                ApagarLuces();
            }

            // Fusionar 3 templates en 1 template de registro
            byte[] regTemplate = new byte[MAX_TEMPLATE_SIZE];
            int regSize = MAX_TEMPLATE_SIZE;

            int mergeRet = zkfp2.DBMerge(
                _dbHandle, temps[0], temps[1], temps[2],
                regTemplate, ref regSize);

            if (mergeRet != zkfp.ZKFP_ERR_OK)
            {
                OnError?.Invoke(this, $"Error al fusionar huellas. Codigo: {mergeRet}. Las 3 capturas deben ser del mismo dedo.");
                EncenderLuzRoja();
                await Task.Delay(1000);
                ApagarLuces();
                return null;
            }

            var args = new HuellaCapturaEventArgs
            {
                Template = regTemplate.Take(regSize).ToArray(),
                TemplateSize = regSize
            };

            EncenderLuzVerde();
            OnEstadoCambiado?.Invoke(this, "Huella registrada exitosamente (3/3 capturas fusionadas)");
            OnHuellaCapturada?.Invoke(this, args);

            await Task.Delay(1000);
            ApagarLuces();

            return args;
        }

        // ===================================================================
        // COMPARACION 1:1 (un template contra otro)
        // Retorna score >= 0 si coinciden, < 0 si error
        // ===================================================================
        public int Comparar(byte[] template1, byte[] template2)
        {
            if (_dbHandle == IntPtr.Zero) return 0;
            int score = zkfp2.DBMatch(_dbHandle, template1, template2);
            return Math.Max(score, 0);
        }

        // ===================================================================
        // IDENTIFICACION 1:N (buscar entre todos los registrados)
        // Usa el cache interno del SDK para busqueda rapida
        // ===================================================================
        public (int personaId, int score) Identificar(
            byte[] templateCapturado, int templateSize,
            List<Persona> personasRegistradas)
        {
            if (_dbHandle == IntPtr.Zero)
                return (-1, 0);

            if (personasRegistradas.Count == 0)
                return (-1, 0);

            // Limpiar cache anterior
            zkfp2.DBClear(_dbHandle);

            int templatesCargados = 0;

            // Agregar todos los templates al cache (fid = persona.Id)
            foreach (var p in personasRegistradas)
            {
                if (p.HuellaTemplate == null || p.HuellaTemplate.Length == 0)
                    continue;

                int addRet = zkfp2.DBAdd(_dbHandle, p.Id, p.HuellaTemplate);
                if (addRet == zkfp.ZKFP_ERR_OK)
                {
                    templatesCargados++;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Error al agregar template persona {p.Id} (DNI: {p.DNI}): {addRet}");
                }
            }

            if (templatesCargados == 0)
            {
                System.Diagnostics.Debug.WriteLine("No se cargo ningun template valido para identificacion");
                return (-1, 0);
            }

            System.Diagnostics.Debug.WriteLine(
                $"Identificando contra {templatesCargados} templates cargados");

            // Ejecutar identificacion 1:N
            int fid = 0;
            int score = 0;
            int ret = zkfp2.DBIdentify(_dbHandle, templateCapturado, ref fid, ref score);

            if (ret == zkfp.ZKFP_ERR_OK)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Coincidencia encontrada: PersonaId={fid}, Score={score}");
                return (fid, score);
            }

            System.Diagnostics.Debug.WriteLine(
                $"Sin coincidencia. Codigo retorno: {ret}");
            return (-1, 0);
        }

        // ===================================================================
        // LUCES Y BUZZER DEL SENSOR
        // Codigos: 101=blanca, 102=verde, 103=roja, 104=buzzer
        // ===================================================================
        public void EncenderLuzVerde()
        {
            if (_deviceHandle == IntPtr.Zero) return;
            byte[] val = new byte[4];
            zkfp2.Int2ByteArray(1, val);
            zkfp2.SetParameters(_deviceHandle, 102, val, 4);
        }

        public void EncenderLuzRoja()
        {
            if (_deviceHandle == IntPtr.Zero) return;
            byte[] val = new byte[4];
            zkfp2.Int2ByteArray(1, val);
            zkfp2.SetParameters(_deviceHandle, 103, val, 4);
        }

        public void EncenderLuzBlanca()
        {
            if (_deviceHandle == IntPtr.Zero) return;
            byte[] val = new byte[4];
            zkfp2.Int2ByteArray(1, val);
            zkfp2.SetParameters(_deviceHandle, 101, val, 4);
        }

        public void EncenderBuzzer()
        {
            if (_deviceHandle == IntPtr.Zero) return;
            byte[] val = new byte[4];
            zkfp2.Int2ByteArray(1, val);
            zkfp2.SetParameters(_deviceHandle, 104, val, 4);
        }

        public void ApagarLuces()
        {
            if (_deviceHandle == IntPtr.Zero) return;
            byte[] val = new byte[4];
            zkfp2.Int2ByteArray(0, val);
            zkfp2.SetParameters(_deviceHandle, 101, val, 4);
            zkfp2.SetParameters(_deviceHandle, 102, val, 4);
            zkfp2.SetParameters(_deviceHandle, 103, val, 4);
        }

        // ===================================================================
        // UTILIDADES
        // ===================================================================
        public void CancelarCaptura()
        {
            _isCapturing = false;
            OnEstadoCambiado?.Invoke(this, "Captura cancelada");
        }

        public bool SensorConectado()
        {
            if (!_isInitialized) return false;
            try
            {
                return zkfp2.GetDeviceCount() > 0;
            }
            catch
            {
                return false;
            }
        }

        public int ObtenerAnchoImagen() => _imageWidth;
        public int ObtenerAltoImagen() => _imageHeight;

        public string TemplateToBase64(byte[] template, int size)
        {
            return Convert.ToBase64String(template, 0, size);
        }

        public byte[]? Base64ToTemplate(string base64)
        {
            try
            {
                return Convert.FromBase64String(base64);
            }
            catch
            {
                return null;
            }
        }

        public string ObtenerInfoDispositivo()
        {
            if (_deviceHandle == IntPtr.Zero) return "No conectado";

            byte[] paramValue = new byte[64];
            int size = 64;
            int ret = zkfp2.GetParameters(_deviceHandle, 1102, paramValue, ref size);

            if (ret == zkfp.ZKFP_ERR_OK)
                return System.Text.Encoding.ASCII.GetString(paramValue, 0, size).TrimEnd('\0');

            return "ZK4500";
        }

        public string ObtenerNumeroSerie()
        {
            if (_deviceHandle == IntPtr.Zero) return "";

            byte[] paramValue = new byte[64];
            int size = 64;
            int ret = zkfp2.GetParameters(_deviceHandle, 1103, paramValue, ref size);

            if (ret == zkfp.ZKFP_ERR_OK)
                return System.Text.Encoding.ASCII.GetString(paramValue, 0, size).TrimEnd('\0');

            return "";
        }

        // ===================================================================
        // LIBERAR RECURSOS
        // ===================================================================
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _isCapturing = false;

            if (_dbHandle != IntPtr.Zero)
            {
                zkfp2.DBFree(_dbHandle);
                _dbHandle = IntPtr.Zero;
            }

            if (_deviceHandle != IntPtr.Zero)
            {
                zkfp2.CloseDevice(_deviceHandle);
                _deviceHandle = IntPtr.Zero;
            }

            zkfp2.Terminate();
        }
    }
}
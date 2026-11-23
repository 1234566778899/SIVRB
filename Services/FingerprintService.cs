using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Models;
using libzkfpcsharp;

namespace Services;

public class FingerprintService : IDisposable
{
	private const int MAX_TEMPLATE_SIZE = 2048;

	private const int CAPTURE_TIMEOUT_SECONDS = 15;

	private nint _deviceHandle = IntPtr.Zero;

	private nint _dbHandle = IntPtr.Zero;

	private int _imageWidth;

	private int _imageHeight;

	private bool _isInitialized;

	private bool _isCapturing;

	private bool _disposed;

	public event EventHandler<HuellaCapturaEventArgs>? OnHuellaCapturada;

	public event EventHandler<string>? OnError;

	public event EventHandler<string>? OnEstadoCambiado;

	public bool Inicializar()
	{
		try
		{
			int num = zkfp2.Init();
			if (num != zkfp.ZKFP_ERR_OK && num != zkfp.ZKFP_ERR_ALREADY_INIT)
			{
				OnError?.Invoke(this, $"Error al inicializar SDK. Codigo: {num}");
				return false;
			}
			if (zkfp2.GetDeviceCount() <= 0)
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
			byte[] array = new byte[4];
			int size = 4;
			zkfp2.GetParameters(_deviceHandle, 1, array, ref size);
			zkfp2.ByteArray2Int(array, ref _imageWidth);
			size = 4;
			zkfp2.GetParameters(_deviceHandle, 2, array, ref size);
			zkfp2.ByteArray2Int(array, ref _imageHeight);
			_isInitialized = true;
			OnEstadoCambiado?.Invoke(this, $"ZK4500 conectado ({_imageWidth}x{_imageHeight})");
			return true;
		}
		catch (DllNotFoundException)
		{
			OnError?.Invoke(this, "No se encontro libzkfpcsharp.dll. Instala el SDK de ZKTeco.");
			return false;
		}
		catch (Exception ex2)
		{
			OnError?.Invoke(this, "Error: " + ex2.Message);
			return false;
		}
	}

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
				byte[] array = new byte[_imageWidth * _imageHeight];
				byte[] array2 = new byte[2048];
				int num = 2048;
				DateTime dateTime = DateTime.Now.AddSeconds(15.0);
				while (DateTime.Now < dateTime && _isCapturing)
				{
					num = 2048;
					if (zkfp2.AcquireFingerprint(_deviceHandle, array, array2, ref num) == zkfp.ZKFP_ERR_OK)
					{
						HuellaCapturaEventArgs e = new HuellaCapturaEventArgs
						{
							Template = array2.Take(num).ToArray(),
							TemplateSize = num,
							ImagenHuella = array.ToArray(),
							ImagenAncho = _imageWidth,
							ImagenAlto = _imageHeight
						};
						_isCapturing = false;
						OnHuellaCapturada?.Invoke(this, e);
						OnEstadoCambiado?.Invoke(this, "Huella capturada correctamente");
						return e;
					}
					Thread.Sleep(200);
				}
				_isCapturing = false;
				if (DateTime.Now >= dateTime)
				{
					OnEstadoCambiado?.Invoke(this, "Tiempo agotado. Intente nuevamente.");
				}
				return (HuellaCapturaEventArgs)null;
			});
		}
		catch (Exception ex)
		{
			_isCapturing = false;
			OnError?.Invoke(this, "Error en captura: " + ex.Message);
			return null;
		}
	}

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
			byte[] tpl = new byte[2048];
			int tplSize = 2048;
			bool captured = false;
			DateTime timeoutDate = DateTime.Now.AddSeconds(15.0);
			try
			{
				await Task.Run(() =>
				{
					while (DateTime.Now < timeoutDate && _isCapturing)
					{
						tplSize = 2048;
						if (zkfp2.AcquireFingerprint(_deviceHandle, imgBuf, tpl, ref tplSize) == zkfp.ZKFP_ERR_OK)
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
			await Task.Delay(2000);
			ApagarLuces();
		}
		byte[] array = new byte[2048];
		int regTempLen = 2048;
		int num = zkfp2.DBMerge(_dbHandle, temps[0], temps[1], temps[2], array, ref regTempLen);
		if (num != zkfp.ZKFP_ERR_OK)
		{
			OnError?.Invoke(this, $"Error al fusionar huellas. Codigo: {num}. Las 3 capturas deben ser del mismo dedo.");
			EncenderLuzRoja();
			await Task.Delay(1000);
			ApagarLuces();
			return null;
		}
		HuellaCapturaEventArgs args = new HuellaCapturaEventArgs
		{
			Template = array.Take(regTempLen).ToArray(),
			TemplateSize = regTempLen
		};
		EncenderLuzVerde();
		OnEstadoCambiado?.Invoke(this, "Huella registrada exitosamente (3/3 capturas fusionadas)");
		OnHuellaCapturada?.Invoke(this, args);
		await Task.Delay(1000);
		ApagarLuces();
		return args;
	}

	public int Comparar(byte[] template1, byte[] template2)
	{
		if (_dbHandle == IntPtr.Zero)
		{
			return 0;
		}
		return Math.Max(zkfp2.DBMatch(_dbHandle, template1, template2), 0);
	}

	public (int personaId, int score) Identificar(byte[] templateCapturado, int templateSize, List<Persona> personasRegistradas)
	{
		if (_dbHandle == IntPtr.Zero)
		{
			return (personaId: -1, score: 0);
		}
		if (personasRegistradas.Count == 0)
		{
			return (personaId: -1, score: 0);
		}
		zkfp2.DBClear(_dbHandle);
		int num = 0;
		foreach (Persona personasRegistrada in personasRegistradas)
		{
			if (personasRegistrada.HuellaTemplate != null && personasRegistrada.HuellaTemplate.Length != 0 && zkfp2.DBAdd(_dbHandle, personasRegistrada.Id, personasRegistrada.HuellaTemplate) == zkfp.ZKFP_ERR_OK)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return (personaId: -1, score: 0);
		}
		int fid = 0;
		int score = 0;
		if (zkfp2.DBIdentify(_dbHandle, templateCapturado, ref fid, ref score) == zkfp.ZKFP_ERR_OK)
		{
			return (personaId: fid, score: score);
		}
		return (personaId: -1, score: 0);
	}

	public void EncenderLuzVerde()
	{
		if (_deviceHandle != IntPtr.Zero)
		{
			byte[] array = new byte[4];
			zkfp2.Int2ByteArray(1, array);
			zkfp2.SetParameters(_deviceHandle, 102, array, 4);
		}
	}

	public void EncenderLuzRoja()
	{
		if (_deviceHandle != IntPtr.Zero)
		{
			byte[] array = new byte[4];
			zkfp2.Int2ByteArray(1, array);
			zkfp2.SetParameters(_deviceHandle, 103, array, 4);
		}
	}

	public void EncenderLuzBlanca()
	{
		if (_deviceHandle != IntPtr.Zero)
		{
			byte[] array = new byte[4];
			zkfp2.Int2ByteArray(1, array);
			zkfp2.SetParameters(_deviceHandle, 101, array, 4);
		}
	}

	public void EncenderBuzzer()
	{
		if (_deviceHandle != IntPtr.Zero)
		{
			byte[] array = new byte[4];
			zkfp2.Int2ByteArray(1, array);
			zkfp2.SetParameters(_deviceHandle, 104, array, 4);
		}
	}

	public void ApagarLuces()
	{
		if (_deviceHandle != IntPtr.Zero)
		{
			byte[] array = new byte[4];
			zkfp2.Int2ByteArray(0, array);
			zkfp2.SetParameters(_deviceHandle, 101, array, 4);
			zkfp2.SetParameters(_deviceHandle, 102, array, 4);
			zkfp2.SetParameters(_deviceHandle, 103, array, 4);
		}
	}

	public void CancelarCaptura()
	{
		_isCapturing = false;
		OnEstadoCambiado?.Invoke(this, "Captura cancelada");
	}

	public bool SensorConectado()
	{
		if (!_isInitialized)
		{
			return false;
		}
		try
		{
			return zkfp2.GetDeviceCount() > 0;
		}
		catch
		{
			return false;
		}
	}

	public int ObtenerAnchoImagen()
	{
		return _imageWidth;
	}

	public int ObtenerAltoImagen()
	{
		return _imageHeight;
	}

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
		if (_deviceHandle == IntPtr.Zero)
		{
			return "No conectado";
		}
		byte[] array = new byte[64];
		int size = 64;
		if (zkfp2.GetParameters(_deviceHandle, 1102, array, ref size) == zkfp.ZKFP_ERR_OK)
		{
			return Encoding.ASCII.GetString(array, 0, size).TrimEnd('\0');
		}
		return "ZK4500";
	}

	public string ObtenerNumeroSerie()
	{
		if (_deviceHandle == IntPtr.Zero)
		{
			return "";
		}
		byte[] array = new byte[64];
		int size = 64;
		if (zkfp2.GetParameters(_deviceHandle, 1103, array, ref size) == zkfp.ZKFP_ERR_OK)
		{
			return Encoding.ASCII.GetString(array, 0, size).TrimEnd('\0');
		}
		return "";
	}

	public void Dispose()
	{
		if (!_disposed)
		{
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

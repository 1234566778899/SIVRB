using Helpers;
using Models;
using Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _db;
        private readonly FingerprintService _fp;

        // ===== COLECCION DE PERSONAS =====

        private ObservableCollection<Persona> _personas = new();
        public ObservableCollection<Persona> Personas
        {
            get => _personas;
            set => SetProperty(ref _personas, value);
        }

        private Persona? _personaSeleccionada;
        public Persona? PersonaSeleccionada
        {
            get => _personaSeleccionada;
            set
            {
                SetProperty(ref _personaSeleccionada, value);
                OnPropertyChanged(nameof(PersonaSeleccionadaVisible));
                OnPropertyChanged(nameof(SeleccionadaTieneRequisitoria));
                OnPropertyChanged(nameof(SeleccionadaSinRequisitoria));
            }
        }

        public bool PersonaSeleccionadaVisible => PersonaSeleccionada != null;
        public bool SeleccionadaTieneRequisitoria => PersonaSeleccionada?.Requisitoria != null && PersonaSeleccionada.Requisitoria.Activa;
        public bool SeleccionadaSinRequisitoria => PersonaSeleccionada != null && !SeleccionadaTieneRequisitoria;

        // ===== ESTADISTICAS =====

        private int _totalRegistrados;
        public int TotalRegistrados
        {
            get => _totalRegistrados;
            set => SetProperty(ref _totalRegistrados, value);
        }

        private int _totalRequisitorias;
        public int TotalRequisitorias
        {
            get => _totalRequisitorias;
            set => SetProperty(ref _totalRequisitorias, value);
        }

        private int _verificacionesHoy;
        public int VerificacionesHoy
        {
            get => _verificacionesHoy;
            set => SetProperty(ref _verificacionesHoy, value);
        }

        private int _capturasHoy;
        public int CapturasHoy
        {
            get => _capturasHoy;
            set => SetProperty(ref _capturasHoy, value);
        }

        // ===== SENSOR =====

        private string _estadoSensor = "Desconectado";
        public string EstadoSensor
        {
            get => _estadoSensor;
            set => SetProperty(ref _estadoSensor, value);
        }

        private bool _sensorConectado;
        public bool SensorConectado
        {
            get => _sensorConectado;
            set => SetProperty(ref _sensorConectado, value);
        }

        // ===== VERIFICACION =====

        private string _estadoVerificacion = "Esperando huella...";
        public string EstadoVerificacion
        {
            get => _estadoVerificacion;
            set => SetProperty(ref _estadoVerificacion, value);
        }

        private string _estadoVerificacionDetalle = "Coloque el dedo en el sensor ZK4500";
        public string EstadoVerificacionDetalle
        {
            get => _estadoVerificacionDetalle;
            set => SetProperty(ref _estadoVerificacionDetalle, value);
        }

        private bool _alertaRequisitoria;
        public bool AlertaRequisitoria
        {
            get => _alertaRequisitoria;
            set => SetProperty(ref _alertaRequisitoria, value);
        }

        private string _alertaMensaje = string.Empty;
        public string AlertaMensaje
        {
            get => _alertaMensaje;
            set => SetProperty(ref _alertaMensaje, value);
        }

        // ===== RESULTADO =====

        private Persona? _personaVerificada;
        public Persona? PersonaVerificada
        {
            get => _personaVerificada;
            set => SetProperty(ref _personaVerificada, value);
        }

        private double _porcentajeCoincidencia;
        public double PorcentajeCoincidencia
        {
            get => _porcentajeCoincidencia;
            set => SetProperty(ref _porcentajeCoincidencia, value);
        }

        private bool _resultadoVisible;
        public bool ResultadoVisible
        {
            get => _resultadoVisible;
            set => SetProperty(ref _resultadoVisible, value);
        }

        // ===== FORMULARIO REGISTRO / EDICION =====

        private string _regDNI = string.Empty;
        public string RegDNI
        {
            get => _regDNI;
            set => SetProperty(ref _regDNI, value);
        }

        private string _regNombre = string.Empty;
        public string RegNombre
        {
            get => _regNombre;
            set => SetProperty(ref _regNombre, value);
        }

        private string _regMotivo = "Denunciante";
        public string RegMotivo
        {
            get => _regMotivo;
            set => SetProperty(ref _regMotivo, value);
        }

        private string _regObservaciones = string.Empty;
        public string RegObservaciones
        {
            get => _regObservaciones;
            set => SetProperty(ref _regObservaciones, value);
        }

        private string _busqueda = string.Empty;
        public string Busqueda
        {
            get => _busqueda;
            set
            {
                SetProperty(ref _busqueda, value);
                _ = FiltrarPersonas();
            }
        }

        // ===== MODO EDICION =====

        private bool _modoEdicion;
        public bool ModoEdicion
        {
            get => _modoEdicion;
            set
            {
                SetProperty(ref _modoEdicion, value);
                OnPropertyChanged(nameof(ModoRegistro));
                OnPropertyChanged(nameof(TituloFormulario));
                OnPropertyChanged(nameof(TextoBotonGuardar));
            }
        }

        public bool ModoRegistro => !ModoEdicion;
        public string TituloFormulario => ModoEdicion ? "EDITAR PERSONA" : "REGISTRAR NUEVA PERSONA";
        public string TextoBotonGuardar => ModoEdicion ? "Guardar Cambios" : "Guardar Registro";

        private int _editandoPersonaId;

        // ===== FORMULARIO REQUISITORIA =====

        private bool _panelRequisitoriaVisible;
        public bool PanelRequisitoriaVisible
        {
            get => _panelRequisitoriaVisible;
            set => SetProperty(ref _panelRequisitoriaVisible, value);
        }

        private bool _modoEdicionRequisitoria;
        public bool ModoEdicionRequisitoria
        {
            get => _modoEdicionRequisitoria;
            set
            {
                SetProperty(ref _modoEdicionRequisitoria, value);
                OnPropertyChanged(nameof(TituloRequisitoria));
                OnPropertyChanged(nameof(TextoBotonRequisitoria));
            }
        }

        public string TituloRequisitoria => ModoEdicionRequisitoria ? "EDITAR REQUISITORIA" : "NUEVA REQUISITORIA";
        public string TextoBotonRequisitoria => ModoEdicionRequisitoria ? "Actualizar Requisitoria" : "Registrar Requisitoria";

        private string _reqPersonaInfo = string.Empty;
        public string ReqPersonaInfo
        {
            get => _reqPersonaInfo;
            set => SetProperty(ref _reqPersonaInfo, value);
        }

        private string _reqDelito = string.Empty;
        public string ReqDelito
        {
            get => _reqDelito;
            set => SetProperty(ref _reqDelito, value);
        }

        private string _reqJuzgado = string.Empty;
        public string ReqJuzgado
        {
            get => _reqJuzgado;
            set => SetProperty(ref _reqJuzgado, value);
        }

        private string _reqExpediente = string.Empty;
        public string ReqExpediente
        {
            get => _reqExpediente;
            set => SetProperty(ref _reqExpediente, value);
        }

        private DateTime _reqFechaEmision = DateTime.Today;
        public DateTime ReqFechaEmision
        {
            get => _reqFechaEmision;
            set => SetProperty(ref _reqFechaEmision, value);
        }

        private string _reqDescripcion = string.Empty;
        public string ReqDescripcion
        {
            get => _reqDescripcion;
            set => SetProperty(ref _reqDescripcion, value);
        }

        private int _reqPersonaId;
        private int _reqEditandoId;

        // ===== HUELLA TEMPORAL PARA REGISTRO =====

        private byte[]? _huellaTemporalTemplate;
        private int _huellaTemporalSize;

        // ===== COMANDOS =====

        public ICommand VerificarHuellaCommand { get; }
        public ICommand RegistrarPersonaCommand { get; }
        public ICommand CapturarHuellaRegistroCommand { get; }
        public ICommand ActualizarDatosCommand { get; }
        public ICommand EditarPersonaCommand { get; }
        public ICommand EliminarPersonaCommand { get; }
        public ICommand CancelarEdicionCommand { get; }

        // Comandos de requisitoria
        public ICommand AbrirAgregarRequisitoriaCommand { get; }
        public ICommand AbrirEditarRequisitoriaCommand { get; }
        public ICommand GuardarRequisitoriaCommand { get; }
        public ICommand DesactivarRequisitoriaCommand { get; }
        public ICommand EliminarRequisitoriaCommand { get; }
        public ICommand CancelarRequisitoriaCommand { get; }

        // ===== CONSTRUCTOR =====

        public MainViewModel()
        {
            _db = new DatabaseService();
            _fp = new FingerprintService();

            _fp.OnEstadoCambiado += (s, msg) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    EstadoSensor = msg;
                    EstadoVerificacion = msg;
                });
            };

            _fp.OnError += (s, msg) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    EstadoVerificacion = $"Error: {msg}";
                    EstadoVerificacionDetalle = msg;
                });
            };

            // Comandos persona
            VerificarHuellaCommand = new RelayCommand(async _ => await VerificarHuella());
            RegistrarPersonaCommand = new RelayCommand(async _ => await GuardarPersona());
            CapturarHuellaRegistroCommand = new RelayCommand(async _ => await CapturarHuellaParaRegistro());
            ActualizarDatosCommand = new RelayCommand(async _ => await CargarDatos());
            EditarPersonaCommand = new RelayCommand(_ => IniciarEdicion());
            EliminarPersonaCommand = new RelayCommand(async _ => await EliminarPersona());
            CancelarEdicionCommand = new RelayCommand(_ => CancelarEdicion());

            // Comandos requisitoria
            AbrirAgregarRequisitoriaCommand = new RelayCommand(_ => AbrirAgregarRequisitoria());
            AbrirEditarRequisitoriaCommand = new RelayCommand(_ => AbrirEditarRequisitoria());
            GuardarRequisitoriaCommand = new RelayCommand(async _ => await GuardarRequisitoria());
            DesactivarRequisitoriaCommand = new RelayCommand(async _ => await DesactivarRequisitoria());
            EliminarRequisitoriaCommand = new RelayCommand(async _ => await EliminarRequisitoria());
            CancelarRequisitoriaCommand = new RelayCommand(_ => CerrarPanelRequisitoria());

            Task.Run(async () =>
            {
                DatabaseService.Initialize();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _fp.Inicializar();
                    SensorConectado = _fp.SensorConectado();
                });

                await CargarDatos();
            });
        }

        // ===== CARGAR DATOS =====

        private async Task CargarDatos()
        {
            try
            {
                var personas = await _db.ObtenerTodasPersonas();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Personas = new ObservableCollection<Persona>(personas);
                });

                TotalRegistrados = await _db.ContarRegistrados();
                TotalRequisitorias = await _db.ContarConRequisitoria();
                VerificacionesHoy = await _db.ContarVerificacionesHoy();
                CapturasHoy = await _db.ContarCapturas();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar datos: {ex.Message}");
            }
        }

        // ===== VERIFICAR HUELLA =====

        private async Task VerificarHuella()
        {
            EstadoVerificacion = "Coloque el dedo en el sensor...";
            EstadoVerificacionDetalle = "Esperando captura de huella";
            ResultadoVisible = false;
            AlertaRequisitoria = false;

            var captura = await _fp.CapturarHuella();
            if (captura == null)
            {
                EstadoVerificacion = "No se pudo capturar la huella";
                EstadoVerificacionDetalle = "Intente nuevamente";
                return;
            }

            EstadoVerificacion = "Buscando coincidencia...";
            EstadoVerificacionDetalle = "Comparando contra registros";

            var personasConHuella = await _db.ObtenerPersonasConHuella();

            if (personasConHuella.Count == 0)
            {
                EstadoVerificacion = "Sin registros";
                EstadoVerificacionDetalle = "No hay personas con huella registrada";
                return;
            }

            var (personaId, score) = _fp.Identificar(
                captura.Template, captura.TemplateSize, personasConHuella);

            var log = new VerificacionLog
            {
                FechaVerificacion = DateTime.Now,
                OperadorPNP = "Operador"
            };

            if (personaId > 0 && score > 50)
            {
                var persona = personasConHuella.FirstOrDefault(p => p.Id == personaId);
                if (persona == null)
                {
                    EstadoVerificacion = "Error interno";
                    EstadoVerificacionDetalle = "No se encontro la persona en la base de datos";
                    return;
                }

                PersonaVerificada = persona;
                PorcentajeCoincidencia = score;
                ResultadoVisible = true;

                log.PersonaId = persona.Id;
                log.Resultado = "Coincidencia";
                log.PorcentajeCoincidencia = score;

                if (persona.Requisitoria != null && persona.Requisitoria.Activa)
                {
                    EstadoVerificacion = "REQUISITORIA DETECTADA";
                    EstadoVerificacionDetalle = "Persona con orden judicial identificada";
                    AlertaRequisitoria = true;
                    AlertaMensaje = $"ALERTA: Requisitoria judicial — DNI {persona.DNI} — {persona.Requisitoria.Juzgado}";

                    _fp.EncenderLuzRoja();
                    _fp.EncenderBuzzer();
                }
                else
                {
                    EstadoVerificacion = "Sin requisitoria";
                    EstadoVerificacionDetalle = "Persona verificada — sin antecedentes";
                    AlertaRequisitoria = false;

                    _fp.EncenderLuzVerde();
                }
            }
            else
            {
                EstadoVerificacion = "Sin coincidencia";
                EstadoVerificacionDetalle = "Huella no registrada en el sistema";
                PersonaVerificada = null;
                ResultadoVisible = false;
                log.Resultado = "Sin coincidencia";
                log.PorcentajeCoincidencia = score;

                _fp.EncenderLuzRoja();
            }

            await _db.RegistrarVerificacion(log);
            await CargarDatos();

            await Task.Delay(2000);
            _fp.ApagarLuces();
        }

        // ===== CAPTURAR HUELLA PARA REGISTRO =====

        private async Task CapturarHuellaParaRegistro()
        {
            EstadoVerificacion = "Iniciando registro de huella...";
            EstadoVerificacionDetalle = "Se requieren 3 capturas del mismo dedo";

            var captura = await _fp.RegistrarHuella();

            if (captura != null)
            {
                _huellaTemporalTemplate = captura.Template;
                _huellaTemporalSize = captura.TemplateSize;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    EstadoVerificacion = "Huella capturada para registro";
                    EstadoVerificacionDetalle = "Complete los datos y presione Guardar";
                });
            }
            else
            {
                _huellaTemporalTemplate = null;
                _huellaTemporalSize = 0;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    EstadoVerificacion = "Error al capturar huella";
                    EstadoVerificacionDetalle = "Intente nuevamente";
                });
            }
        }

        // ===== GUARDAR PERSONA (REGISTRO O EDICION) =====

        private async Task GuardarPersona()
        {
            if (ModoEdicion)
                await ActualizarPersona();
            else
                await RegistrarPersona();
        }

        // ===== REGISTRAR PERSONA =====

        private async Task RegistrarPersona()
        {
            if (string.IsNullOrWhiteSpace(RegDNI) || RegDNI.Length != 8)
            {
                MessageBox.Show("El DNI debe tener 8 digitos.",
                    "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(RegNombre))
            {
                MessageBox.Show("Ingrese el nombre completo.",
                    "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_huellaTemporalTemplate == null || _huellaTemporalTemplate.Length == 0)
            {
                MessageBox.Show("Primero capture la huella presionando 'Registrar'.\nSe requieren 3 capturas del mismo dedo.",
                    "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existente = await _db.BuscarPorDNI(RegDNI);
            if (existente != null)
            {
                MessageBox.Show($"Ya existe una persona registrada con DNI {RegDNI}.",
                    "Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var personasConHuella = await _db.ObtenerPersonasConHuella();
            if (personasConHuella.Count > 0)
            {
                var (personaId, score) = _fp.Identificar(
                    _huellaTemporalTemplate, _huellaTemporalSize, personasConHuella);

                if (personaId > 0 && score > 50)
                {
                    var duplicada = personasConHuella.FirstOrDefault(p => p.Id == personaId);
                    string info = duplicada != null
                        ? $"DNI: {duplicada.DNI} — {duplicada.NombreCompleto}"
                        : $"ID: {personaId}";

                    MessageBox.Show(
                        $"Esta huella ya esta registrada en el sistema.\n\n" +
                        $"Persona encontrada: {info}\n" +
                        $"Coincidencia: {score} puntos\n\n" +
                        $"No se puede registrar la misma huella dos veces.",
                        "Huella Duplicada", MessageBoxButton.OK, MessageBoxImage.Warning);

                    _fp.EncenderLuzRoja();
                    await Task.Delay(1500);
                    _fp.ApagarLuces();
                    return;
                }
            }

            var persona = new Persona
            {
                DNI = RegDNI,
                NombreCompleto = RegNombre,
                MotivoRegistro = RegMotivo,
                Observaciones = RegObservaciones,
                HuellaTemplate = _huellaTemporalTemplate,
                HuellaTemplateSize = _huellaTemporalSize,
                FechaRegistro = DateTime.Now
            };

            await _db.RegistrarPersona(persona);
            LimpiarFormulario();

            EstadoVerificacion = "Persona registrada exitosamente";
            EstadoVerificacionDetalle = $"DNI {persona.DNI} guardado en el sistema";

            MessageBox.Show("Persona registrada exitosamente.",
                "Exito", MessageBoxButton.OK, MessageBoxImage.Information);

            await CargarDatos();
        }

        // ===== INICIAR EDICION =====

        private void IniciarEdicion()
        {
            if (PersonaSeleccionada == null)
            {
                MessageBox.Show("Seleccione una persona de la tabla para editar.",
                    "Editar", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ModoEdicion = true;
            _editandoPersonaId = PersonaSeleccionada.Id;
            RegDNI = PersonaSeleccionada.DNI;
            RegNombre = PersonaSeleccionada.NombreCompleto;
            RegMotivo = PersonaSeleccionada.MotivoRegistro;
            RegObservaciones = PersonaSeleccionada.Observaciones;

            _huellaTemporalTemplate = PersonaSeleccionada.HuellaTemplate;
            _huellaTemporalSize = PersonaSeleccionada.HuellaTemplateSize;

            EstadoVerificacion = "Modo edicion";
            EstadoVerificacionDetalle = $"Editando: {PersonaSeleccionada.NombreCompleto}";
        }

        // ===== ACTUALIZAR PERSONA =====

        private async Task ActualizarPersona()
        {
            if (string.IsNullOrWhiteSpace(RegDNI) || RegDNI.Length != 8)
            {
                MessageBox.Show("El DNI debe tener 8 digitos.",
                    "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(RegNombre))
            {
                MessageBox.Show("Ingrese el nombre completo.",
                    "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existente = await _db.BuscarPorDNI(RegDNI);
            if (existente != null && existente.Id != _editandoPersonaId)
            {
                MessageBox.Show($"Ya existe otra persona registrada con DNI {RegDNI}.",
                    "Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var persona = new Persona
            {
                Id = _editandoPersonaId,
                DNI = RegDNI,
                NombreCompleto = RegNombre,
                MotivoRegistro = RegMotivo,
                Observaciones = RegObservaciones
            };

            await _db.ActualizarPersona(persona);

            if (_huellaTemporalTemplate != null && _huellaTemporalSize > 0)
            {
                await _db.ActualizarHuellaPersona(_editandoPersonaId, _huellaTemporalTemplate, _huellaTemporalSize);
            }

            CancelarEdicion();

            EstadoVerificacion = "Persona actualizada";
            EstadoVerificacionDetalle = $"DNI {persona.DNI} actualizado correctamente";

            MessageBox.Show("Persona actualizada exitosamente.",
                "Exito", MessageBoxButton.OK, MessageBoxImage.Information);

            await CargarDatos();
        }

        // ===== ELIMINAR PERSONA =====

        private async Task EliminarPersona()
        {
            if (PersonaSeleccionada == null)
            {
                MessageBox.Show("Seleccione una persona de la tabla para eliminar.",
                    "Eliminar", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Esta seguro de eliminar a {PersonaSeleccionada.NombreCompleto} (DNI: {PersonaSeleccionada.DNI})?\n\n" +
                "Se eliminaran tambien sus verificaciones y requisitorias asociadas.\n" +
                "Esta accion no se puede deshacer.",
                "Confirmar Eliminacion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
                await _db.EliminarPersona(PersonaSeleccionada.Id);

                if (ModoEdicion && _editandoPersonaId == PersonaSeleccionada.Id)
                    CancelarEdicion();

                PersonaSeleccionada = null;
                ResultadoVisible = false;

                EstadoVerificacion = "Persona eliminada";
                EstadoVerificacionDetalle = "Registro eliminado del sistema";

                MessageBox.Show("Persona eliminada exitosamente.",
                    "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);

                await CargarDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelarEdicion()
        {
            ModoEdicion = false;
            _editandoPersonaId = 0;
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            RegDNI = string.Empty;
            RegNombre = string.Empty;
            RegMotivo = "Denunciante";
            RegObservaciones = string.Empty;
            _huellaTemporalTemplate = null;
            _huellaTemporalSize = 0;
        }

        // ================================================================
        // ===== REQUISITORIAS =====
        // ================================================================

        private void AbrirAgregarRequisitoria()
        {
            if (PersonaSeleccionada == null)
            {
                MessageBox.Show("Seleccione una persona de la tabla para asignarle una requisitoria.",
                    "Requisitoria", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (PersonaSeleccionada.Requisitoria != null && PersonaSeleccionada.Requisitoria.Activa)
            {
                MessageBox.Show(
                    $"Esta persona ya tiene una requisitoria activa.\n\n" +
                    $"Delito: {PersonaSeleccionada.Requisitoria.Delito}\n" +
                    $"Juzgado: {PersonaSeleccionada.Requisitoria.Juzgado}\n\n" +
                    $"Use 'Editar Req.' para modificarla o 'Desactivar' para darla de baja.",
                    "Requisitoria Existente", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ModoEdicionRequisitoria = false;
            _reqPersonaId = PersonaSeleccionada.Id;
            _reqEditandoId = 0;
            ReqPersonaInfo = $"{PersonaSeleccionada.NombreCompleto} — DNI: {PersonaSeleccionada.DNI}";

            LimpiarFormularioRequisitoria();
            PanelRequisitoriaVisible = true;
        }

        private void AbrirEditarRequisitoria()
        {
            if (PersonaSeleccionada == null)
            {
                MessageBox.Show("Seleccione una persona de la tabla.",
                    "Requisitoria", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (PersonaSeleccionada.Requisitoria == null)
            {
                MessageBox.Show("Esta persona no tiene requisitoria registrada.\nUse 'Agregar Req.' para crear una.",
                    "Requisitoria", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ModoEdicionRequisitoria = true;
            _reqPersonaId = PersonaSeleccionada.Id;
            _reqEditandoId = PersonaSeleccionada.Requisitoria.Id;
            ReqPersonaInfo = $"{PersonaSeleccionada.NombreCompleto} — DNI: {PersonaSeleccionada.DNI}";

            ReqDelito = PersonaSeleccionada.Requisitoria.Delito;
            ReqJuzgado = PersonaSeleccionada.Requisitoria.Juzgado;
            ReqExpediente = PersonaSeleccionada.Requisitoria.NumeroExpediente;
            ReqFechaEmision = PersonaSeleccionada.Requisitoria.FechaEmision;
            ReqDescripcion = PersonaSeleccionada.Requisitoria.Descripcion;

            PanelRequisitoriaVisible = true;
        }

        private async Task GuardarRequisitoria()
        {
            if (string.IsNullOrWhiteSpace(ReqDelito))
            {
                MessageBox.Show("Ingrese el delito.",
                    "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ReqJuzgado))
            {
                MessageBox.Show("Ingrese el juzgado.",
                    "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ReqExpediente))
            {
                MessageBox.Show("Ingrese el numero de expediente.",
                    "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (ModoEdicionRequisitoria && _reqEditandoId > 0)
                {
                    var requisitoria = new Requisitoria
                    {
                        Id = _reqEditandoId,
                        PersonaId = _reqPersonaId,
                        Delito = ReqDelito,
                        Juzgado = ReqJuzgado,
                        NumeroExpediente = ReqExpediente,
                        FechaEmision = ReqFechaEmision,
                        Descripcion = ReqDescripcion,
                        Activa = true
                    };

                    await _db.ActualizarRequisitoria(requisitoria);

                    MessageBox.Show("Requisitoria actualizada exitosamente.",
                        "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var requisitoria = new Requisitoria
                    {
                        PersonaId = _reqPersonaId,
                        Delito = ReqDelito,
                        Juzgado = ReqJuzgado,
                        NumeroExpediente = ReqExpediente,
                        FechaEmision = ReqFechaEmision,
                        Descripcion = ReqDescripcion,
                        Activa = true
                    };

                    await _db.RegistrarRequisitoria(requisitoria);

                    MessageBox.Show("Requisitoria registrada exitosamente.\nEsta persona ahora aparecera con alerta roja.",
                        "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                CerrarPanelRequisitoria();
                await CargarDatos();

                OnPropertyChanged(nameof(SeleccionadaTieneRequisitoria));
                OnPropertyChanged(nameof(SeleccionadaSinRequisitoria));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar requisitoria: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public List<string> MotivosRegistro { get; } = new List<string>
{
    "Denunciante", "Testigo", "Intervenido", "Personal PNP"
};
        private async Task DesactivarRequisitoria()
        {
            if (PersonaSeleccionada == null || PersonaSeleccionada.Requisitoria == null)
            {
                MessageBox.Show("Seleccione una persona con requisitoria activa.",
                    "Desactivar", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!PersonaSeleccionada.Requisitoria.Activa)
            {
                MessageBox.Show("Esta requisitoria ya esta desactivada.",
                    "Desactivar", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Desactivar la requisitoria de {PersonaSeleccionada.NombreCompleto}?\n\n" +
                $"Delito: {PersonaSeleccionada.Requisitoria.Delito}\n" +
                $"Juzgado: {PersonaSeleccionada.Requisitoria.Juzgado}\n\n" +
                "La persona dejara de aparecer con alerta roja.\n" +
                "El registro de la requisitoria se conservara como inactivo.",
                "Confirmar Desactivacion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
                await _db.DesactivarRequisitoria(PersonaSeleccionada.Requisitoria.Id);

                MessageBox.Show("Requisitoria desactivada.",
                    "Desactivada", MessageBoxButton.OK, MessageBoxImage.Information);

                AlertaRequisitoria = false;
                CerrarPanelRequisitoria();
                await CargarDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task EliminarRequisitoria()
        {
            if (PersonaSeleccionada == null || PersonaSeleccionada.Requisitoria == null)
            {
                MessageBox.Show("Seleccione una persona con requisitoria.",
                    "Eliminar Requisitoria", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Eliminar permanentemente la requisitoria de {PersonaSeleccionada.NombreCompleto}?\n\n" +
                $"Delito: {PersonaSeleccionada.Requisitoria.Delito}\n" +
                $"Expediente: {PersonaSeleccionada.Requisitoria.NumeroExpediente}\n\n" +
                "Esta accion no se puede deshacer.",
                "Confirmar Eliminacion de Requisitoria",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
                await _db.EliminarRequisitoria(PersonaSeleccionada.Requisitoria.Id);

                MessageBox.Show("Requisitoria eliminada permanentemente.",
                    "Eliminada", MessageBoxButton.OK, MessageBoxImage.Information);

                AlertaRequisitoria = false;
                CerrarPanelRequisitoria();
                await CargarDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CerrarPanelRequisitoria()
        {
            PanelRequisitoriaVisible = false;
            ModoEdicionRequisitoria = false;
            _reqPersonaId = 0;
            _reqEditandoId = 0;
            LimpiarFormularioRequisitoria();
        }

        private void LimpiarFormularioRequisitoria()
        {
            ReqDelito = string.Empty;
            ReqJuzgado = string.Empty;
            ReqExpediente = string.Empty;
            ReqFechaEmision = DateTime.Today;
            ReqDescripcion = string.Empty;
            ReqPersonaInfo = string.Empty;
        }

        // ===== FILTRAR PERSONAS =====

        private async Task FiltrarPersonas()
        {
            if (string.IsNullOrWhiteSpace(Busqueda))
            {
                await CargarDatos();
                return;
            }

            try
            {
                var todas = await _db.ObtenerTodasPersonas();
                var filtro = Busqueda.ToLower();
                var filtradas = todas.Where(p =>
                    p.DNI.Contains(filtro) ||
                    p.NombreCompleto.ToLower().Contains(filtro) ||
                    p.MotivoRegistro.ToLower().Contains(filtro) ||
                    p.Observaciones.ToLower().Contains(filtro))
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Personas = new ObservableCollection<Persona>(filtradas);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al filtrar: {ex.Message}");
            }
        }
    }
}
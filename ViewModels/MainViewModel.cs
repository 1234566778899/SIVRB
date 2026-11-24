using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Helpers;
using Models;
using Services;

namespace ViewModels;

public class MainViewModel : BaseViewModel
{
	private readonly DatabaseService _db;

	private readonly FingerprintService _fp;

	private string _vistaActual;

	private ObservableCollection<BarItem> _registrosPorMes = new ObservableCollection<BarItem>();

	private ObservableCollection<BarItem> _verificacionesPorDia = new ObservableCollection<BarItem>();

	private ObservableCollection<BarItem> _registrosPorMotivo = new ObservableCollection<BarItem>();

	private ObservableCollection<Persona> _personas = new ObservableCollection<Persona>();

	private Persona? _personaSeleccionada;

	private int _totalRegistrados;

	private int _totalRequisitorias;

	private int _verificacionesHoy;

	private int _capturasHoy;

	private string _estadoSensor = "Desconectado";

	private bool _sensorConectado;

	private string _estadoVerificacion = "Esperando huella...";

	private string _estadoVerificacionDetalle = "Coloque el dedo en el sensor ZK4500";

	private bool _alertaRequisitoria;

	private string _alertaMensaje = string.Empty;

	private bool _modoVerificacionActivo = true;

	private Persona? _personaVerificada;

	private double _porcentajeCoincidencia;

	private bool _resultadoVisible;

	private string _regDNI = string.Empty;

	private string _regNombre = string.Empty;

	private string _regMotivo = "Denunciante";

	private string _regObservaciones = string.Empty;

	private string _busqueda = string.Empty;

	private bool _modoEdicion;

	private int _editandoPersonaId;

	private bool _panelRequisitoriaVisible;

	private bool _modoEdicionRequisitoria;

	private string _reqPersonaInfo = string.Empty;

	private string _reqDelito = string.Empty;

	private string _reqJuzgado = string.Empty;

	private string _reqExpediente = string.Empty;

	private DateTime _reqFechaEmision = DateTime.Today;

	private string _reqDescripcion = string.Empty;

	private int _reqPersonaId;

	private int _reqEditandoId;

	private byte[]? _huellaTemporalTemplate;

	private int _huellaTemporalSize;

	public bool EsAdministrador => SesionActual.EsAdministrador;

	public bool EsOperador => SesionActual.EsOperador;

	public string NombreUsuarioActual => SesionActual.NombreUsuario;

	public string VistaActual
	{
		get
		{
			return _vistaActual;
		}
		set
		{
			SetProperty(ref _vistaActual, value, "VistaActual");
			OnPropertyChanged("EsDashboard");
			OnPropertyChanged("EsOperaciones");
		}
	}

	public bool EsDashboard => VistaActual == "Dashboard";

	public bool EsOperaciones => VistaActual == "Operaciones";

	public ICommand IrDashboardCommand { get; }

	public ICommand IrOperacionesCommand { get; }

	public ObservableCollection<BarItem> RegistrosPorMes
	{
		get
		{
			return _registrosPorMes;
		}
		set
		{
			SetProperty(ref _registrosPorMes, value, "RegistrosPorMes");
		}
	}

	public ObservableCollection<BarItem> VerificacionesPorDia
	{
		get
		{
			return _verificacionesPorDia;
		}
		set
		{
			SetProperty(ref _verificacionesPorDia, value, "VerificacionesPorDia");
		}
	}

	public ObservableCollection<BarItem> RegistrosPorMotivo
	{
		get
		{
			return _registrosPorMotivo;
		}
		set
		{
			SetProperty(ref _registrosPorMotivo, value, "RegistrosPorMotivo");
		}
	}

	public ObservableCollection<Persona> Personas
	{
		get
		{
			return _personas;
		}
		set
		{
			SetProperty(ref _personas, value, "Personas");
			OnPropertyChanged("SinResultadosBusqueda");
		}
	}

	public Persona? PersonaSeleccionada
	{
		get
		{
			return _personaSeleccionada;
		}
		set
		{
			SetProperty(ref _personaSeleccionada, value, "PersonaSeleccionada");
			CommandManager.InvalidateRequerySuggested();
			OnPropertyChanged("PersonaSeleccionadaVisible");
			OnPropertyChanged("SeleccionadaTieneRequisitoria");
			OnPropertyChanged("SeleccionadaSinRequisitoria");
		}
	}

	public bool PersonaSeleccionadaVisible => PersonaSeleccionada != null;

	public bool SeleccionadaTieneRequisitoria
	{
		get
		{
			if (PersonaSeleccionada?.Requisitoria != null)
			{
				return PersonaSeleccionada.Requisitoria.Activa;
			}
			return false;
		}
	}

	public bool SeleccionadaSinRequisitoria
	{
		get
		{
			if (PersonaSeleccionada != null)
			{
				return !SeleccionadaTieneRequisitoria;
			}
			return false;
		}
	}

	public int TotalRegistrados
	{
		get
		{
			return _totalRegistrados;
		}
		set
		{
			SetProperty(ref _totalRegistrados, value, "TotalRegistrados");
			OnPropertyChanged("SinRegistros");
			OnPropertyChanged("SinResultadosBusqueda");
		}
	}

	/// <summary>No hay ninguna persona registrada en el sistema (estado vacio).</summary>
	public bool SinRegistros => TotalRegistrados == 0;

	/// <summary>Hay registros, pero la busqueda actual no devolvio resultados.</summary>
	public bool SinResultadosBusqueda => TotalRegistrados > 0 && (Personas == null || Personas.Count == 0);

	public int TotalRequisitorias
	{
		get
		{
			return _totalRequisitorias;
		}
		set
		{
			SetProperty(ref _totalRequisitorias, value, "TotalRequisitorias");
		}
	}

	public int VerificacionesHoy
	{
		get
		{
			return _verificacionesHoy;
		}
		set
		{
			SetProperty(ref _verificacionesHoy, value, "VerificacionesHoy");
		}
	}

	public int CapturasHoy
	{
		get
		{
			return _capturasHoy;
		}
		set
		{
			SetProperty(ref _capturasHoy, value, "CapturasHoy");
		}
	}

	public string EstadoSensor
	{
		get
		{
			return _estadoSensor;
		}
		set
		{
			SetProperty(ref _estadoSensor, value, "EstadoSensor");
		}
	}

	public bool SensorConectado
	{
		get
		{
			return _sensorConectado;
		}
		set
		{
			SetProperty(ref _sensorConectado, value, "SensorConectado");
			// Habilita/deshabilita los botones que dependen del sensor
			CommandManager.InvalidateRequerySuggested();
		}
	}

	public string EstadoVerificacion
	{
		get
		{
			return _estadoVerificacion;
		}
		set
		{
			SetProperty(ref _estadoVerificacion, value, "EstadoVerificacion");
		}
	}

	public string EstadoVerificacionDetalle
	{
		get
		{
			return _estadoVerificacionDetalle;
		}
		set
		{
			SetProperty(ref _estadoVerificacionDetalle, value, "EstadoVerificacionDetalle");
		}
	}

	public bool AlertaRequisitoria
	{
		get
		{
			return _alertaRequisitoria;
		}
		set
		{
			SetProperty(ref _alertaRequisitoria, value, "AlertaRequisitoria");
		}
	}

	public string AlertaMensaje
	{
		get
		{
			return _alertaMensaje;
		}
		set
		{
			SetProperty(ref _alertaMensaje, value, "AlertaMensaje");
		}
	}

	public bool ModoVerificacionActivo
	{
		get
		{
			return _modoVerificacionActivo;
		}
		set
		{
			if (SetProperty(ref _modoVerificacionActivo, value, "ModoVerificacionActivo"))
			{
				OnPropertyChanged("ModoRegistroActivo");
			}
		}
	}

	public bool ModoRegistroActivo => !ModoVerificacionActivo;

	private bool _capturaRegistroFallida;

	/// <summary>True cuando la ultima captura de huella para registro fallo (muestra "Reintentar").</summary>
	public bool CapturaRegistroFallida
	{
		get => _capturaRegistroFallida;
		set => SetProperty(ref _capturaRegistroFallida, value, "CapturaRegistroFallida");
	}

	// ===== Estado del formulario de registro =====
	// Separado de EstadoVerificacion para que el panel de verificacion (Operaciones)
	// no muestre textos del formulario de registro y viceversa.

	private string _estadoRegistro = "Registro";
	private string _estadoRegistroDetalle = "Capture la huella y complete los datos";
	private string _mensajeRegistro = string.Empty;
	private string _mensajeRequisitoria = string.Empty;
	private bool _capturandoRegistro;

	public string EstadoRegistro
	{
		get => _estadoRegistro;
		set => SetProperty(ref _estadoRegistro, value, "EstadoRegistro");
	}

	public string EstadoRegistroDetalle
	{
		get => _estadoRegistroDetalle;
		set => SetProperty(ref _estadoRegistroDetalle, value, "EstadoRegistroDetalle");
	}

	/// <summary>Error de validacion del formulario de registro (se muestra dentro del formulario).</summary>
	public string MensajeRegistro
	{
		get => _mensajeRegistro;
		set => SetProperty(ref _mensajeRegistro, value, "MensajeRegistro");
	}

	/// <summary>Error de validacion del formulario de requisitoria (se muestra dentro del formulario).</summary>
	public string MensajeRequisitoria
	{
		get => _mensajeRequisitoria;
		set => SetProperty(ref _mensajeRequisitoria, value, "MensajeRequisitoria");
	}

	// ===== Datos para el panel (dashboard) y estados vacios =====

	private ObservableCollection<Persona> _personasRecientes = new ObservableCollection<Persona>();
	private bool _hayRegistrosPorMes;
	private bool _hayVerificacionesPorDia;
	private bool _hayRegistrosPorMotivo;

	/// <summary>Ultimos 5 registros, para el panel. La lista completa esta en Operaciones.</summary>
	public ObservableCollection<Persona> PersonasRecientes
	{
		get => _personasRecientes;
		set => SetProperty(ref _personasRecientes, value, "PersonasRecientes");
	}

	public bool HayRegistrosPorMes
	{
		get => _hayRegistrosPorMes;
		set => SetProperty(ref _hayRegistrosPorMes, value, "HayRegistrosPorMes");
	}

	public bool HayVerificacionesPorDia
	{
		get => _hayVerificacionesPorDia;
		set => SetProperty(ref _hayVerificacionesPorDia, value, "HayVerificacionesPorDia");
	}

	public bool HayRegistrosPorMotivo
	{
		get => _hayRegistrosPorMotivo;
		set => SetProperty(ref _hayRegistrosPorMotivo, value, "HayRegistrosPorMotivo");
	}

	/// <summary>Libera el lector de huellas. Se llama al cerrar la ventana principal.</summary>
	public void LiberarRecursos()
	{
		_fp.Dispose();
	}

	private void ActualizarEstadoInicialVerificacion()
	{
		if (SensorConectado)
		{
			EstadoVerificacion = "Listo para verificar";
			EstadoVerificacionDetalle = "Presione «Verificar huella» y coloque el dedo en el sensor";
		}
		else
		{
			EstadoVerificacion = "Sensor desconectado";
			EstadoVerificacionDetalle = "Conecte el lector ZK4500 y reinicie la aplicación para verificar huellas";
		}
	}

	public Persona? PersonaVerificada
	{
		get
		{
			return _personaVerificada;
		}
		set
		{
			SetProperty(ref _personaVerificada, value, "PersonaVerificada");
		}
	}

	public double PorcentajeCoincidencia
	{
		get
		{
			return _porcentajeCoincidencia;
		}
		set
		{
			SetProperty(ref _porcentajeCoincidencia, value, "PorcentajeCoincidencia");
		}
	}

	public bool ResultadoVisible
	{
		get
		{
			return _resultadoVisible;
		}
		set
		{
			SetProperty(ref _resultadoVisible, value, "ResultadoVisible");
		}
	}

	public string RegDNI
	{
		get
		{
			return _regDNI;
		}
		set
		{
			SetProperty(ref _regDNI, value, "RegDNI");
		}
	}

	public string RegNombre
	{
		get
		{
			return _regNombre;
		}
		set
		{
			SetProperty(ref _regNombre, value, "RegNombre");
		}
	}

	public string RegMotivo
	{
		get
		{
			return _regMotivo;
		}
		set
		{
			SetProperty(ref _regMotivo, value, "RegMotivo");
		}
	}

	public string RegObservaciones
	{
		get
		{
			return _regObservaciones;
		}
		set
		{
			SetProperty(ref _regObservaciones, value, "RegObservaciones");
		}
	}

	public string Busqueda
	{
		get
		{
			return _busqueda;
		}
		set
		{
			SetProperty(ref _busqueda, value, "Busqueda");
			FiltrarPersonas();
		}
	}

	public bool ModoEdicion
	{
		get
		{
			return _modoEdicion;
		}
		set
		{
			SetProperty(ref _modoEdicion, value, "ModoEdicion");
			OnPropertyChanged("ModoRegistro");
			OnPropertyChanged("TituloFormulario");
			OnPropertyChanged("TextoBotonGuardar");
		}
	}

	public bool ModoRegistro => !ModoEdicion;

	public string TituloFormulario
	{
		get
		{
			if (!ModoEdicion)
			{
				return "Registrar nueva persona";
			}
			return "Editar persona";
		}
	}

	public string TextoBotonGuardar
	{
		get
		{
			if (!ModoEdicion)
			{
				return "Guardar registro";
			}
			return "Guardar cambios";
		}
	}

	public bool PanelRequisitoriaVisible
	{
		get
		{
			return _panelRequisitoriaVisible;
		}
		set
		{
			SetProperty(ref _panelRequisitoriaVisible, value, "PanelRequisitoriaVisible");
		}
	}

	public bool ModoEdicionRequisitoria
	{
		get
		{
			return _modoEdicionRequisitoria;
		}
		set
		{
			SetProperty(ref _modoEdicionRequisitoria, value, "ModoEdicionRequisitoria");
			OnPropertyChanged("TituloRequisitoria");
			OnPropertyChanged("TextoBotonRequisitoria");
		}
	}

	public string TituloRequisitoria
	{
		get
		{
			if (!ModoEdicionRequisitoria)
			{
				return "Nueva requisitoria";
			}
			return "Editar requisitoria";
		}
	}

	public string TextoBotonRequisitoria
	{
		get
		{
			if (!ModoEdicionRequisitoria)
			{
				return "Registrar requisitoria";
			}
			return "Actualizar requisitoria";
		}
	}

	public string ReqPersonaInfo
	{
		get
		{
			return _reqPersonaInfo;
		}
		set
		{
			SetProperty(ref _reqPersonaInfo, value, "ReqPersonaInfo");
		}
	}

	public string ReqDelito
	{
		get
		{
			return _reqDelito;
		}
		set
		{
			SetProperty(ref _reqDelito, value, "ReqDelito");
		}
	}

	public string ReqJuzgado
	{
		get
		{
			return _reqJuzgado;
		}
		set
		{
			SetProperty(ref _reqJuzgado, value, "ReqJuzgado");
		}
	}

	public string ReqExpediente
	{
		get
		{
			return _reqExpediente;
		}
		set
		{
			SetProperty(ref _reqExpediente, value, "ReqExpediente");
		}
	}

	public DateTime ReqFechaEmision
	{
		get
		{
			return _reqFechaEmision;
		}
		set
		{
			SetProperty(ref _reqFechaEmision, value, "ReqFechaEmision");
		}
	}

	public string ReqDescripcion
	{
		get
		{
			return _reqDescripcion;
		}
		set
		{
			SetProperty(ref _reqDescripcion, value, "ReqDescripcion");
		}
	}

	public ICommand VerificarHuellaCommand { get; }

	public ICommand AbrirRegistroModalCommand { get; }

	public ICommand RegistrarPersonaCommand { get; }

	public ICommand CapturarHuellaRegistroCommand { get; }

	public ICommand ActualizarDatosCommand { get; }

	public ICommand EditarPersonaCommand { get; }

	public ICommand EliminarPersonaCommand { get; }

	public ICommand CancelarEdicionCommand { get; }

	public ICommand AbrirAgregarRequisitoriaCommand { get; }

	public ICommand AbrirEditarRequisitoriaCommand { get; }

	public ICommand GuardarRequisitoriaCommand { get; }

	public ICommand EliminarRequisitoriaCommand { get; }

	public ICommand CancelarRequisitoriaCommand { get; }

	public List<string> MotivosRegistro { get; } = new List<string> { "Denunciante", "Testigo", "Intervenido", "Personal PNP" };

	public event Action? RegistroModalSolicitado;

	public event Action? RegistroModalCierreSolicitado;

	public MainViewModel()
	{
		_db = new DatabaseService();
		_fp = new FingerprintService();
		_vistaActual = (SesionActual.EsAdministrador ? "Dashboard" : "Operaciones");
		// Los mensajes del sensor van al panel que inicio la captura:
		// el formulario de registro o el panel de verificacion.
		_fp.OnEstadoCambiado += (object? s, string msg) =>
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				if (_capturandoRegistro)
				{
					EstadoRegistro = msg;
				}
				else
				{
					EstadoVerificacion = msg;
				}
			});
		};
		_fp.OnError += (object? s, string msg) =>
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				if (_capturandoRegistro)
				{
					EstadoRegistro = "Error del sensor";
					EstadoRegistroDetalle = msg;
				}
				else
				{
					EstadoVerificacion = "Error del sensor";
					EstadoVerificacionDetalle = msg;
				}
			});
		};
		// Solo se puede verificar con el sensor conectado
		VerificarHuellaCommand = new RelayCommand(async (object? _) =>
		{
			await VerificarHuella();
		}, (object? _) => SensorConectado);
		AbrirRegistroModalCommand = new RelayCommand((object? _) =>
		{
			PrepararNuevoRegistro();
		});
		RegistrarPersonaCommand = new RelayCommand(async (object? _) =>
		{
			await GuardarPersona();
		});
		CapturarHuellaRegistroCommand = new RelayCommand(async (object? _) =>
		{
			await CapturarHuellaParaRegistro();
		}, (object? _) => SensorConectado && !_capturandoRegistro);
		ActualizarDatosCommand = new RelayCommand(async (object? _) =>
		{
			await CargarDatos();
		});
		EditarPersonaCommand = new RelayCommand((object? _) =>
		{
			IniciarEdicion();
		});
		EliminarPersonaCommand = new RelayCommand(async (object? _) =>
		{
			await EliminarPersona();
		});
		CancelarEdicionCommand = new RelayCommand((object? _) =>
		{
			CancelarEdicion();
		});
		IrDashboardCommand = new RelayCommand((object? _) =>
		{
			VistaActual = "Dashboard";
		});
		IrOperacionesCommand = new RelayCommand((object? _) =>
		{
			VistaActual = "Operaciones";
		});
		// "Agregar" solo si la persona no tiene requisitoria activa; "Editar" solo si tiene una
		AbrirAgregarRequisitoriaCommand = new RelayCommand((object? _) =>
		{
			AbrirAgregarRequisitoria();
		}, (object? _) => PersonaSeleccionada != null && (PersonaSeleccionada.Requisitoria == null || !PersonaSeleccionada.Requisitoria.Activa));
		AbrirEditarRequisitoriaCommand = new RelayCommand((object? _) =>
		{
			AbrirEditarRequisitoria();
		}, (object? _) => PersonaSeleccionada?.Requisitoria != null);
		GuardarRequisitoriaCommand = new RelayCommand(async (object? _) =>
		{
			await GuardarRequisitoria();
		});
		EliminarRequisitoriaCommand = new RelayCommand(async (object? _) =>
		{
			await EliminarRequisitoria();
		});
		CancelarRequisitoriaCommand = new RelayCommand((object? _) =>
		{
			CerrarPanelRequisitoria();
		});
		Task.Run(async () =>
		{
			DatabaseService.Initialize();
			Application.Current.Dispatcher.Invoke(() =>
			{
				_fp.Inicializar();
				SensorConectado = _fp.SensorConectado();
				ActualizarEstadoInicialVerificacion();
			});
			await CargarDatos();
		});
	}

	private async Task CargarDatos()
	{
		try
		{
			List<Persona> personas = await _db.ObtenerTodasPersonas();
			Application.Current.Dispatcher.Invoke(() =>
			{
				Personas = new ObservableCollection<Persona>(personas);
				PersonasRecientes = new ObservableCollection<Persona>(personas.OrderByDescending((Persona p) => p.FechaRegistro).Take(5));
			});
			TotalRegistrados = await _db.ContarRegistrados();
			TotalRequisitorias = await _db.ContarConRequisitoria();
			VerificacionesHoy = await _db.ContarVerificacionesHoy();
			CapturasHoy = await _db.ContarCapturas();
			List<(string Mes, int Cantidad)> regPorMes = await _db.ObtenerRegistrosPorMes();
			int maxReg = ((regPorMes.Count > 0) ? regPorMes.Max(((string Mes, int Cantidad) x) => x.Cantidad) : 0);
			Application.Current.Dispatcher.Invoke(() =>
			{
				RegistrosPorMes = new ObservableCollection<BarItem>(regPorMes.Select(((string Mes, int Cantidad) x) => new BarItem
				{
					Label = x.Mes,
					Value = x.Cantidad,
					Percentage = ((maxReg > 0) ? ((double)x.Cantidad / (double)maxReg * 100.0) : 0.0),
					Color = "#4C78A8"
				}));
			});
			List<(string Dia, int Cantidad)> verPorDia = await _db.ObtenerVerificacionesPorDia();
			int maxVer = ((verPorDia.Count > 0) ? verPorDia.Max(((string Dia, int Cantidad) x) => x.Cantidad) : 0);
			Application.Current.Dispatcher.Invoke(() =>
			{
				VerificacionesPorDia = new ObservableCollection<BarItem>(verPorDia.Select(((string Dia, int Cantidad) x) => new BarItem
				{
					Label = x.Dia,
					Value = x.Cantidad,
					Percentage = ((maxVer > 0) ? ((double)x.Cantidad / (double)maxVer * 100.0) : 0.0),
					Color = "#4F8F70"
				}));
			});
			List<(string Motivo, int Cantidad)> regPorMotivo = await _db.ObtenerRegistrosPorMotivo();
			int maxMot = ((regPorMotivo.Count > 0) ? regPorMotivo.Max(((string Motivo, int Cantidad) x) => x.Cantidad) : 0);
			Application.Current.Dispatcher.Invoke(() =>
			{
				RegistrosPorMotivo = new ObservableCollection<BarItem>(from x in regPorMotivo
					where x.Cantidad > 0
					select new BarItem
					{
						Label = x.Motivo,
						Value = x.Cantidad,
						Percentage = ((maxMot > 0) ? ((double)x.Cantidad / (double)maxMot * 100.0) : 0.0),
						Color = "#B88A4A"
					});
				// Indicadores para mostrar "sin datos" en lugar de graficos en cero
				HayRegistrosPorMes = regPorMes.Any(((string Mes, int Cantidad) x) => x.Cantidad > 0);
				HayVerificacionesPorDia = verPorDia.Any(((string Dia, int Cantidad) x) => x.Cantidad > 0);
				HayRegistrosPorMotivo = RegistrosPorMotivo.Count > 0;
				CommandManager.InvalidateRequerySuggested();
			});
		}
		catch (Exception)
		{
		}
	}

	private async Task VerificarHuella()
	{
		ModoVerificacionActivo = true;
		EstadoVerificacion = "Coloque el dedo en el sensor...";
		EstadoVerificacionDetalle = "Esperando captura de huella";
		ResultadoVisible = false;
		AlertaRequisitoria = false;
		HuellaCapturaEventArgs captura = await _fp.CapturarHuella();
		if (captura == null)
		{
			EstadoVerificacion = "No se pudo capturar la huella";
			EstadoVerificacionDetalle = "Intente nuevamente";
			return;
		}
		EstadoVerificacion = "Buscando coincidencia...";
		EstadoVerificacionDetalle = "Comparando contra registros";
		List<Persona> list = await _db.ObtenerPersonasConHuella();
		if (list.Count == 0)
		{
			EstadoVerificacion = "Sin registros";
			EstadoVerificacionDetalle = "No hay personas con huella registrada";
			return;
		}
		(int, int) tuple = _fp.Identificar(captura.Template, captura.TemplateSize, list);
		int personaId = tuple.Item1;
		int item = tuple.Item2;
		VerificacionLog verificacionLog = new VerificacionLog
		{
			FechaVerificacion = DateTime.Now,
			OperadorPNP = "Operador"
		};
		if (personaId > 0 && item > 50)
		{
			Persona persona = list.FirstOrDefault((Persona p) => p.Id == personaId);
			if (persona == null)
			{
				EstadoVerificacion = "Error interno";
				EstadoVerificacionDetalle = "No se encontro la persona en la base de datos";
				return;
			}
			PersonaVerificada = persona;
			PorcentajeCoincidencia = item;
			ResultadoVisible = true;
			verificacionLog.PersonaId = persona.Id;
			verificacionLog.Resultado = "Coincidencia";
			verificacionLog.PorcentajeCoincidencia = item;
			if (persona.Requisitoria != null && persona.Requisitoria.Activa)
			{
				EstadoVerificacion = "REQUISITORIA DETECTADA";
				EstadoVerificacionDetalle = "Persona con orden judicial identificada";
				AlertaRequisitoria = true;
				AlertaMensaje = "ALERTA: Requisitoria judicial — DNI " + persona.DNI + " — " + persona.Requisitoria.Juzgado;
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
			verificacionLog.Resultado = "Sin coincidencia";
			verificacionLog.PorcentajeCoincidencia = item;
			_fp.EncenderLuzRoja();
		}
		await _db.RegistrarVerificacion(verificacionLog);
		await CargarDatos();
		await Task.Delay(2000);
		_fp.ApagarLuces();
	}

	private async Task CapturarHuellaParaRegistro()
	{
		ModoVerificacionActivo = false;
		CapturaRegistroFallida = false;
		MensajeRegistro = string.Empty;
		_capturandoRegistro = true;
		CommandManager.InvalidateRequerySuggested();
		EstadoRegistro = "Iniciando registro de huella...";
		EstadoRegistroDetalle = "Se requieren 3 capturas del mismo dedo";
		HuellaCapturaEventArgs? e;
		try
		{
			e = await _fp.RegistrarHuella();
		}
		finally
		{
			_capturandoRegistro = false;
			CommandManager.InvalidateRequerySuggested();
		}
		if (e != null)
		{
			_huellaTemporalTemplate = e.Template;
			_huellaTemporalSize = e.TemplateSize;
			Application.Current.Dispatcher.Invoke(() =>
			{
				EstadoRegistro = "Huella capturada";
				EstadoRegistroDetalle = "Complete los datos y presione «Guardar registro»";
			});
		}
		else
		{
			_huellaTemporalTemplate = null;
			_huellaTemporalSize = 0;
			Application.Current.Dispatcher.Invoke(() =>
			{
				EstadoRegistro = "Error al capturar huella";
				EstadoRegistroDetalle = "Intente nuevamente";
				CapturaRegistroFallida = true;
			});
		}
	}

	private async Task GuardarPersona()
	{
		if (ModoEdicion)
		{
			await ActualizarPersona();
		}
		else
		{
			await RegistrarPersona();
		}
	}

	/// <summary>Valida DNI y nombre; si hay error lo deja en MensajeRegistro.</summary>
	private bool ValidarDatosRegistro()
	{
		if (string.IsNullOrWhiteSpace(RegDNI) || RegDNI.Length != 8 || !RegDNI.All(char.IsDigit))
		{
			MensajeRegistro = "El DNI debe tener exactamente 8 dígitos numéricos.";
			return false;
		}
		if (string.IsNullOrWhiteSpace(RegNombre))
		{
			MensajeRegistro = "Ingrese el nombre completo.";
			return false;
		}
		MensajeRegistro = string.Empty;
		return true;
	}

	private async Task RegistrarPersona()
	{
		if (!ValidarDatosRegistro())
		{
			return;
		}
		if (_huellaTemporalTemplate == null || _huellaTemporalTemplate.Length == 0)
		{
			MensajeRegistro = "Capture la huella con «Capturar huella» antes de guardar (3 capturas del mismo dedo).";
			return;
		}
		if (await _db.BuscarPorDNI(RegDNI) != null)
		{
			MensajeRegistro = "Ya existe una persona registrada con DNI " + RegDNI + ".";
			return;
		}
		List<Persona> list = await _db.ObtenerPersonasConHuella();
		if (list.Count > 0)
		{
			var (personaId, num) = _fp.Identificar(_huellaTemporalTemplate, _huellaTemporalSize, list);
			if (personaId > 0 && num > 50)
			{
				Persona persona = list.FirstOrDefault((Persona p) => p.Id == personaId);
				string value = ((persona != null) ? ("DNI: " + persona.DNI + " — " + persona.NombreCompleto) : $"ID: {personaId}");
				MensajeRegistro = $"Esta huella ya está registrada ({value}, coincidencia {num} puntos). No se puede registrar la misma huella dos veces.";
				_fp.EncenderLuzRoja();
				await Task.Delay(1500);
				_fp.ApagarLuces();
				return;
			}
		}
		Persona persona2 = new Persona
		{
			DNI = RegDNI,
			NombreCompleto = RegNombre,
			MotivoRegistro = RegMotivo,
			Observaciones = RegObservaciones,
			HuellaTemplate = _huellaTemporalTemplate,
			HuellaTemplateSize = _huellaTemporalSize,
			FechaRegistro = DateTime.Now
		};
		await _db.RegistrarPersona(persona2);
		LimpiarFormulario();
		MessageBox.Show("Persona registrada exitosamente.", "Exito", MessageBoxButton.OK, MessageBoxImage.Asterisk);
		await CargarDatos();
		RegistroModalCierreSolicitado?.Invoke();
	}

	private void PrepararNuevoRegistro()
	{
		CancelarEdicion();
		ModoVerificacionActivo = false;
		CapturaRegistroFallida = false;
		MensajeRegistro = string.Empty;
		if (SensorConectado)
		{
			EstadoRegistro = "Nuevo registro";
			EstadoRegistroDetalle = "Capture la huella y complete los datos";
		}
		else
		{
			EstadoRegistro = "Sensor desconectado";
			EstadoRegistroDetalle = "Conecte el lector ZK4500 para capturar la huella";
		}
		RegistroModalSolicitado?.Invoke();
	}

	private void IniciarEdicion()
	{
		if (PersonaSeleccionada == null)
		{
			MessageBox.Show("Seleccione una persona de la tabla para editar.", "Editar", MessageBoxButton.OK, MessageBoxImage.Asterisk);
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
		CapturaRegistroFallida = false;
		MensajeRegistro = string.Empty;
		EstadoRegistro = "Editando registro";
		EstadoRegistroDetalle = "Puede volver a capturar la huella si es necesario";
		RegistroModalSolicitado?.Invoke();
	}

	private async Task ActualizarPersona()
	{
		if (!ValidarDatosRegistro())
		{
			return;
		}
		Persona persona = await _db.BuscarPorDNI(RegDNI);
		if (persona != null && persona.Id != _editandoPersonaId)
		{
			MensajeRegistro = "Ya existe otra persona registrada con DNI " + RegDNI + ".";
			return;
		}
		Persona persona2 = new Persona
		{
			Id = _editandoPersonaId,
			DNI = RegDNI,
			NombreCompleto = RegNombre,
			MotivoRegistro = RegMotivo,
			Observaciones = RegObservaciones
		};
		await _db.ActualizarPersona(persona2);
		if (_huellaTemporalTemplate != null && _huellaTemporalSize > 0)
		{
			await _db.ActualizarHuellaPersona(_editandoPersonaId, _huellaTemporalTemplate, _huellaTemporalSize);
		}
		CancelarEdicion();
		MessageBox.Show("Persona actualizada exitosamente.", "Exito", MessageBoxButton.OK, MessageBoxImage.Asterisk);
		await CargarDatos();
		RegistroModalCierreSolicitado?.Invoke();
	}

	private async Task EliminarPersona()
	{
		if (PersonaSeleccionada == null)
		{
			MessageBox.Show("Seleccione una persona de la tabla para eliminar.", "Eliminar", MessageBoxButton.OK, MessageBoxImage.Asterisk);
		}
		else
		{
			if (MessageBox.Show($"¿Esta seguro de eliminar a {PersonaSeleccionada.NombreCompleto} (DNI: {PersonaSeleccionada.DNI})?\n\n" + "Se eliminaran tambien sus verificaciones y requisitorias asociadas.\nEsta accion no se puede deshacer.", "Confirmar Eliminacion", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) != MessageBoxResult.Yes)
			{
				return;
			}
			try
			{
				await _db.EliminarPersona(PersonaSeleccionada.Id);
				if (ModoEdicion && _editandoPersonaId == PersonaSeleccionada.Id)
				{
					CancelarEdicion();
				}
				PersonaSeleccionada = null;
				ResultadoVisible = false;
				MessageBox.Show("Persona eliminada exitosamente.", "Eliminado", MessageBoxButton.OK, MessageBoxImage.Asterisk);
				await CargarDatos();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error al eliminar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
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

	private void AbrirAgregarRequisitoria()
	{
		if (PersonaSeleccionada == null)
		{
			MessageBox.Show("Seleccione una persona de la tabla para asignarle una requisitoria.", "Requisitoria", MessageBoxButton.OK, MessageBoxImage.Asterisk);
		}
		else if (PersonaSeleccionada.Requisitoria != null && PersonaSeleccionada.Requisitoria.Activa)
		{
			MessageBox.Show($"Esta persona ya tiene una requisitoria activa.\n\nDelito: {PersonaSeleccionada.Requisitoria.Delito}\nJuzgado: {PersonaSeleccionada.Requisitoria.Juzgado}\n\nUse «Editar requisitoria» para modificarla o darla de baja.", "Requisitoria existente", MessageBoxButton.OK, MessageBoxImage.Asterisk);
		}
		else
		{
			MensajeRequisitoria = string.Empty;
			ModoEdicionRequisitoria = false;
			_reqPersonaId = PersonaSeleccionada.Id;
			_reqEditandoId = 0;
			ReqPersonaInfo = PersonaSeleccionada.NombreCompleto + " — DNI: " + PersonaSeleccionada.DNI;
			LimpiarFormularioRequisitoria();
			PanelRequisitoriaVisible = true;
		}
	}

	private void AbrirEditarRequisitoria()
	{
		if (!SesionActual.EsAdministrador)
		{
			MessageBox.Show("No tiene permisos para editar requisitorias.", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			return;
		}
		if (PersonaSeleccionada == null)
		{
			MessageBox.Show("Seleccione una persona de la tabla.", "Requisitoria", MessageBoxButton.OK, MessageBoxImage.Asterisk);
			return;
		}
		if (PersonaSeleccionada.Requisitoria == null)
		{
			MessageBox.Show("Esta persona no tiene requisitoria registrada.\nUse «Agregar requisitoria» para crear una.", "Requisitoria", MessageBoxButton.OK, MessageBoxImage.Asterisk);
			return;
		}
		MensajeRequisitoria = string.Empty;
		ModoEdicionRequisitoria = true;
		_reqPersonaId = PersonaSeleccionada.Id;
		_reqEditandoId = PersonaSeleccionada.Requisitoria.Id;
		ReqPersonaInfo = PersonaSeleccionada.NombreCompleto + " — DNI: " + PersonaSeleccionada.DNI;
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
			MensajeRequisitoria = "Ingrese el delito.";
			return;
		}
		if (string.IsNullOrWhiteSpace(ReqJuzgado))
		{
			MensajeRequisitoria = "Ingrese el juzgado.";
			return;
		}
		if (string.IsNullOrWhiteSpace(ReqExpediente))
		{
			MensajeRequisitoria = "Ingrese el número de expediente.";
			return;
		}
		MensajeRequisitoria = string.Empty;
		try
		{
			if (ModoEdicionRequisitoria && _reqEditandoId > 0)
			{
				Requisitoria requisitoria = new Requisitoria
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
				MessageBox.Show("Requisitoria actualizada exitosamente.", "Exito", MessageBoxButton.OK, MessageBoxImage.Asterisk);
			}
			else
			{
				Requisitoria requisitoria2 = new Requisitoria
				{
					PersonaId = _reqPersonaId,
					Delito = ReqDelito,
					Juzgado = ReqJuzgado,
					NumeroExpediente = ReqExpediente,
					FechaEmision = ReqFechaEmision,
					Descripcion = ReqDescripcion,
					Activa = true
				};
				await _db.RegistrarRequisitoria(requisitoria2);
				MessageBox.Show("Requisitoria registrada exitosamente.\nEsta persona ahora aparecera con alerta roja.", "Exito", MessageBoxButton.OK, MessageBoxImage.Asterisk);
			}
			CerrarPanelRequisitoria();
			await CargarDatos();
			OnPropertyChanged("SeleccionadaTieneRequisitoria");
			OnPropertyChanged("SeleccionadaSinRequisitoria");
		}
		catch (Exception ex)
		{
			MessageBox.Show("Error al guardar requisitoria: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private async Task EliminarRequisitoria()
	{
		if (!SesionActual.EsAdministrador)
		{
			MessageBox.Show("No tiene permisos para eliminar requisitorias.", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Exclamation);
		}
		else if (PersonaSeleccionada == null || PersonaSeleccionada.Requisitoria == null)
		{
			MessageBox.Show("Seleccione una persona con requisitoria.", "Eliminar Requisitoria", MessageBoxButton.OK, MessageBoxImage.Asterisk);
		}
		else if (MessageBox.Show($"¿Eliminar permanentemente la requisitoria de {PersonaSeleccionada.NombreCompleto}?\n\nDelito: {PersonaSeleccionada.Requisitoria.Delito}\nExpediente: {PersonaSeleccionada.Requisitoria.NumeroExpediente}\n\n" + "Esta accion no se puede deshacer.", "Confirmar Eliminacion de Requisitoria", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
		{
			try
			{
				await _db.EliminarRequisitoria(PersonaSeleccionada.Requisitoria.Id);
				MessageBox.Show("Requisitoria eliminada permanentemente.", "Eliminada", MessageBoxButton.OK, MessageBoxImage.Asterisk);
				AlertaRequisitoria = false;
				CerrarPanelRequisitoria();
				await CargarDatos();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}
	}

	private void CerrarPanelRequisitoria()
	{
		PanelRequisitoriaVisible = false;
		ModoEdicionRequisitoria = false;
		MensajeRequisitoria = string.Empty;
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

	private async Task FiltrarPersonas()
	{
		if (string.IsNullOrWhiteSpace(Busqueda))
		{
			await CargarDatos();
			return;
		}
		try
		{
			List<Persona> source = await _db.ObtenerTodasPersonas();
			string filtro = Busqueda.ToLower();
			List<Persona> filtradas = source.Where((Persona p) => p.DNI.Contains(filtro) || p.NombreCompleto.ToLower().Contains(filtro) || p.MotivoRegistro.ToLower().Contains(filtro) || p.Observaciones.ToLower().Contains(filtro)).ToList();
			Application.Current.Dispatcher.Invoke(() =>
			{
				Personas = new ObservableCollection<Persona>(filtradas);
			});
		}
		catch (Exception)
		{
		}
	}
}

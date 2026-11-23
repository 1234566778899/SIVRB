using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Helpers;
using Models;
using Services;

namespace ViewModels;

public class LoginViewModel : BaseViewModel
{
	private readonly DatabaseService _db = new DatabaseService();

	private string _nombreUsuario = string.Empty;

	private string _password = string.Empty;

	private string _mensajeError = string.Empty;

	private bool _cargando;

	public string NombreUsuario
	{
		get
		{
			return _nombreUsuario;
		}
		set
		{
			SetProperty(ref _nombreUsuario, value, "NombreUsuario");
		}
	}

	public string Password
	{
		get
		{
			return _password;
		}
		set
		{
			SetProperty(ref _password, value, "Password");
		}
	}

	public string MensajeError
	{
		get
		{
			return _mensajeError;
		}
		set
		{
			SetProperty(ref _mensajeError, value, "MensajeError");
		}
	}

	public bool Cargando
	{
		get
		{
			return _cargando;
		}
		set
		{
			SetProperty(ref _cargando, value, "Cargando");
		}
	}

	public Usuario? UsuarioAutenticado { get; private set; }

	public ICommand LoginCommand { get; }

	public event Action? LoginExitoso;

	public LoginViewModel()
	{
		LoginCommand = new RelayCommand(async (object? _) =>
		{
			await IniciarSesion();
		}, (object? _) => !Cargando && !string.IsNullOrWhiteSpace(NombreUsuario) && !string.IsNullOrWhiteSpace(Password));
	}

	private async Task IniciarSesion()
	{
		Cargando = true;
		MensajeError = string.Empty;
		try
		{
			Usuario usuario = await _db.AutenticarUsuario(NombreUsuario.Trim(), Password);
			if (usuario != null)
			{
				UsuarioAutenticado = usuario;
				LoginExitoso?.Invoke();
			}
			else
			{
				MensajeError = "Usuario o contrasena incorrectos";
			}
		}
		catch (Exception)
		{
			MensajeError = "Error al conectar con la base de datos";
		}
		finally
		{
			Cargando = false;
		}
	}
}

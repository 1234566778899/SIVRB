using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Helpers;
using Models;
using Services;

namespace ViewModels;

public class GestionUsuariosViewModel : BaseViewModel
{
	private readonly DatabaseService _db = new DatabaseService();

	private ObservableCollection<Usuario> _usuarios = new ObservableCollection<Usuario>();

	private string _nuevoNombreUsuario = string.Empty;

	private string _nuevoPassword = string.Empty;

	private string _nuevoNombreCompleto = string.Empty;

	private string _nuevoRol = "Operador";

	private string _mensaje = string.Empty;

	public ObservableCollection<Usuario> Usuarios
	{
		get
		{
			return _usuarios;
		}
		set
		{
			SetProperty(ref _usuarios, value, "Usuarios");
		}
	}

	public string NuevoNombreUsuario
	{
		get
		{
			return _nuevoNombreUsuario;
		}
		set
		{
			SetProperty(ref _nuevoNombreUsuario, value, "NuevoNombreUsuario");
		}
	}

	public string NuevoPassword
	{
		get
		{
			return _nuevoPassword;
		}
		set
		{
			SetProperty(ref _nuevoPassword, value, "NuevoPassword");
		}
	}

	public string NuevoNombreCompleto
	{
		get
		{
			return _nuevoNombreCompleto;
		}
		set
		{
			SetProperty(ref _nuevoNombreCompleto, value, "NuevoNombreCompleto");
		}
	}

	public string NuevoRol
	{
		get
		{
			return _nuevoRol;
		}
		set
		{
			SetProperty(ref _nuevoRol, value, "NuevoRol");
		}
	}

	public string Mensaje
	{
		get
		{
			return _mensaje;
		}
		set
		{
			SetProperty(ref _mensaje, value, "Mensaje");
		}
	}

	public ICommand CrearUsuarioCommand { get; }

	public ICommand ToggleActivoCommand { get; }

	public ICommand EliminarUsuarioCommand { get; }

	public ICommand ResetPasswordCommand { get; }

	public GestionUsuariosViewModel()
	{
		CrearUsuarioCommand = new RelayCommand(async (object? _) =>
		{
			await CrearUsuario();
		}, (object? _) => !string.IsNullOrWhiteSpace(NuevoNombreUsuario) && !string.IsNullOrWhiteSpace(NuevoPassword) && !string.IsNullOrWhiteSpace(NuevoNombreCompleto));
		ToggleActivoCommand = new RelayCommand(async (object? p) =>
		{
			await ToggleActivo(p);
		});
		EliminarUsuarioCommand = new RelayCommand(async (object? p) =>
		{
			await EliminarUsuario(p);
		});
		ResetPasswordCommand = new RelayCommand(async (object? p) =>
		{
			await ResetPassword(p);
		});
		CargarUsuarios();
	}

	public async Task CargarUsuarios()
	{
		Usuarios = new ObservableCollection<Usuario>(await _db.ObtenerTodosUsuarios());
	}

	private async Task CrearUsuario()
	{
		if (await _db.CrearUsuario(NuevoNombreUsuario.Trim(), NuevoPassword, NuevoNombreCompleto.Trim(), NuevoRol))
		{
			Mensaje = "Usuario '" + NuevoNombreUsuario + "' creado exitosamente";
			NuevoNombreUsuario = string.Empty;
			NuevoPassword = string.Empty;
			NuevoNombreCompleto = string.Empty;
			NuevoRol = "Operador";
			await CargarUsuarios();
		}
		else
		{
			Mensaje = "Error: el nombre de usuario ya existe";
		}
	}

	private async Task ToggleActivo(object? parameter)
	{
		if (parameter is Usuario usuario)
		{
			if (usuario.NombreUsuario == "admin")
			{
				Mensaje = "No se puede desactivar al administrador principal";
				return;
			}
			await _db.ActualizarUsuario(usuario.Id, usuario.NombreCompleto, usuario.Rol, !usuario.Activo);
			Mensaje = (usuario.Activo ? ("Usuario '" + usuario.NombreUsuario + "' desactivado") : ("Usuario '" + usuario.NombreUsuario + "' activado"));
			await CargarUsuarios();
		}
	}

	private async Task EliminarUsuario(object? parameter)
	{
		if (parameter is Usuario usuario)
		{
			if (usuario.NombreUsuario == "admin")
			{
				Mensaje = "No se puede eliminar al administrador principal";
			}
			else if (MessageBox.Show("¿Eliminar al usuario '" + usuario.NombreUsuario + "'?\n\nEsta acción no se puede deshacer.", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
			{
				await _db.EliminarUsuario(usuario.Id);
				Mensaje = "Usuario '" + usuario.NombreUsuario + "' eliminado";
				await CargarUsuarios();
			}
		}
	}

	private async Task ResetPassword(object? parameter)
	{
		if (parameter is Usuario usuario)
		{
			string newPass = "Temp1234";
			await _db.CambiarPassword(usuario.Id, newPass);
			Mensaje = "Contrasena de '" + usuario.NombreUsuario + "' restablecida a: " + newPass;
		}
	}
}

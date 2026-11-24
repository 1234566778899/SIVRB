using System;
using System.Windows;
using Helpers;
using ViewModels;

namespace SIVRB.Views;

public partial class MainWindow : Window
{
	private RegistroModalWindow? _registroModal;

	public MainWindow()
	{
		InitializeComponent();
		// Unico ViewModel de la ventana (antes el XAML creaba otro y el lector se inicializaba dos veces)
		MainViewModel mainViewModel = new MainViewModel();
		mainViewModel.RegistroModalSolicitado += AbrirRegistroModal;
		mainViewModel.RegistroModalCierreSolicitado += CerrarRegistroModal;
		DataContext = mainViewModel;
		// Al cerrar (incluido "Cerrar sesión") se libera el lector para que la nueva ventana pueda abrirlo
		Closed += (object? _, EventArgs _) => mainViewModel.LiberarRecursos();
	}

	private void AbrirRegistroModal()
	{
		if (_registroModal != null)
		{
			_registroModal.Activate();
		}
		else
		{
			if (!(DataContext is MainViewModel viewModel))
			{
				return;
			}
			_registroModal = new RegistroModalWindow(viewModel)
			{
				Owner = this
			};
			_registroModal.Closed += (object? _, EventArgs _) =>
			{
				if (DataContext is MainViewModel mainViewModel && mainViewModel.CancelarEdicionCommand.CanExecute(null))
				{
					mainViewModel.CancelarEdicionCommand.Execute(null);
				}
				_registroModal = null;
			};
			_registroModal.ShowDialog();
		}
	}

	private void CerrarRegistroModal()
	{
		_registroModal?.Close();
	}

	private void BtnGestionUsuarios_Click(object sender, RoutedEventArgs e)
	{
		GestionUsuariosWindow gestionUsuariosWindow = new GestionUsuariosWindow();
		gestionUsuariosWindow.Owner = this;
		gestionUsuariosWindow.ShowDialog();
	}

	private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
	{
		// Liberar el lector antes de que la nueva ventana intente abrirlo
		(DataContext as MainViewModel)?.LiberarRecursos();
		SesionActual.Usuario = null;
		Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
		LoginWindow loginWindow = new LoginWindow();
		if (loginWindow.ShowDialog() == true)
		{
			if (loginWindow.DataContext is LoginViewModel loginViewModel)
			{
				SesionActual.Usuario = loginViewModel.UsuarioAutenticado;
			}
			MainWindow mainWindow = new MainWindow();
			Application.Current.MainWindow = mainWindow;
			Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
			mainWindow.Show();
			Close();
		}
		else
		{
			Application.Current.Shutdown();
		}
	}
}

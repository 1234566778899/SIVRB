using System.Windows;
using Helpers;
using Models;
using SIVRB.Views;
using Services;
using ViewModels;

namespace SIVRB;

public partial class App : Application
{
	public static Usuario? UsuarioActual
	{
		get
		{
			return SesionActual.Usuario;
		}
		set
		{
			SesionActual.Usuario = value;
		}
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		ShutdownMode = ShutdownMode.OnExplicitShutdown;
		DatabaseService.Initialize();
		LoginWindow loginWindow = new LoginWindow();
		if (loginWindow.ShowDialog() == true)
		{
			if (loginWindow.DataContext is LoginViewModel loginViewModel)
			{
				UsuarioActual = loginViewModel.UsuarioAutenticado;
			}
			ShutdownMode = ShutdownMode.OnMainWindowClose;
			MainWindow mainWindow = (MainWindow)(MainWindow = new MainWindow());
			mainWindow.Show();
		}
		else
		{
			Shutdown();
		}
	}
}

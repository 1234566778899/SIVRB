using System.Windows;
using ViewModels;

namespace SIVRB.Views;

public partial class GestionUsuariosWindow : Window
{
	public GestionUsuariosWindow()
	{
		InitializeComponent();
		DataContext = new GestionUsuariosViewModel();
	}

	private void NewPasswordField_PasswordChanged(object sender, RoutedEventArgs e)
	{
		if (DataContext is GestionUsuariosViewModel gestionUsuariosViewModel)
		{
			gestionUsuariosViewModel.NuevoPassword = NewPasswordField.Password;
		}
	}
}

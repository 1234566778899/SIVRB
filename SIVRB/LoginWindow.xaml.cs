using System.Windows;
using System.Windows.Input;
using ViewModels;

namespace SIVRB.Views;

public partial class LoginWindow : Window
{
	public LoginWindow()
	{
		InitializeComponent();
		LoginViewModel loginViewModel = new LoginViewModel();
		loginViewModel.LoginExitoso += OnLoginExitoso;
		DataContext = loginViewModel;
	}

	private void OnLoginExitoso()
	{
		DialogResult = true;
		Close();
	}

	private void PasswordField_PasswordChanged(object sender, RoutedEventArgs e)
	{
		if (DataContext is LoginViewModel loginViewModel)
		{
			loginViewModel.Password = PasswordField.Password;
		}
	}

	private void PasswordField_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return && DataContext is LoginViewModel loginViewModel && loginViewModel.LoginCommand.CanExecute(null))
		{
			loginViewModel.LoginCommand.Execute(null);
		}
	}

	private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		DragMove();
	}

	private void BtnSalir_Click(object sender, RoutedEventArgs e)
	{
		DialogResult = false;
		Close();
	}
}

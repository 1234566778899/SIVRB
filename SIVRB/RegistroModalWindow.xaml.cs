using System.Linq;
using System.Windows;
using System.Windows.Input;
using ViewModels;

namespace SIVRB.Views;

public partial class RegistroModalWindow : Window
{
	public RegistroModalWindow(MainViewModel viewModel)
	{
		InitializeComponent();
		DataContext = viewModel;
	}

	private void CloseButton_Click(object sender, RoutedEventArgs e)
	{
		if (DataContext is MainViewModel mainViewModel && mainViewModel.CancelarEdicionCommand.CanExecute(null))
		{
			mainViewModel.CancelarEdicionCommand.Execute(null);
		}
		Close();
	}

	private void TxtDNI_PreviewTextInput(object sender, TextCompositionEventArgs e)
	{
		e.Handled = !e.Text.All(char.IsDigit);
	}

	private void TxtDNI_Pasting(object sender, DataObjectPastingEventArgs e)
	{
		if (!e.DataObject.GetDataPresent(typeof(string)))
		{
			e.CancelCommand();
		}
		else if (!((string)e.DataObject.GetData(typeof(string))).All(char.IsDigit))
		{
			e.CancelCommand();
		}
	}
}

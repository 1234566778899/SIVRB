using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Helpers;

/// <summary>
/// true -> Collapsed, false -> Visible. Se usa para los mensajes de "sin datos",
/// que se muestran justo cuando no hay contenido.
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return (value is bool b && b) ? Visibility.Collapsed : Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Helpers;

/// <summary>
/// Convierte un porcentaje (0-100) en un ancho proporcional (GridLength en estrellas)
/// para dibujar barras que se adaptan al ancho disponible.
/// ConverterParameter="rest" devuelve la parte restante (100 - porcentaje).
/// </summary>
public class PercentToStarConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		double pct = 0.0;
		if (value is double d)
		{
			pct = d;
		}
		else if (value is int i)
		{
			pct = i;
		}
		pct = Math.Clamp(pct, 0.0, 100.0);
		if (string.Equals(parameter as string, "rest", StringComparison.OrdinalIgnoreCase))
		{
			pct = 100.0 - pct;
		}
		return new GridLength(pct, GridUnitType.Star);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

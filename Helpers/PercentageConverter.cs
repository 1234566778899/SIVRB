using System;
using System.Globalization;
using System.Windows.Data;

namespace Helpers;

public class PercentageConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		double result = 0.0;
		if (value is double num)
		{
			result = num;
		}
		else if (value is int num2)
		{
			result = num2;
		}
		else if (value != null)
		{
			double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
		}
		double result2 = 150.0;
		if (parameter != null)
		{
			double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out result2);
		}
		double num3 = result / 100.0 * result2;
		if (num3 < 1.0)
		{
			num3 = 1.0;
		}
		return num3;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}

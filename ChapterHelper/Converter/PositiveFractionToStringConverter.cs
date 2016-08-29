using Fractions;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ChapterHelper.Converter
{
    [ValueConversion(typeof(Fraction?), typeof(string))]
    public class PositiveFractionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return String.Empty;
            }

            return ((Fraction)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return Fraction.FromString((string)value);
            }
            catch
            {
                return null;
            }
        }
    }
}

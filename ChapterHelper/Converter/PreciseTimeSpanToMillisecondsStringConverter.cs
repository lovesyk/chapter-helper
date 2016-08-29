using Fractions;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ChapterHelper.Converter
{
    [ValueConversion(typeof(Fraction), typeof(string))]
    public class PreciseTimeSpanToMillisecondsStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((PreciseTimeSpan)value).TotalMilliseconds.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return PreciseTimeSpan.FromMilliseconds((double)value);
        }
    }
}

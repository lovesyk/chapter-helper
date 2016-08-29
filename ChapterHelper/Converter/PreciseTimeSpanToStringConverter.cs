using Fractions;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ChapterHelper.Converter
{
    [ValueConversion(typeof(Fraction), typeof(string))]
    public class PreciseTimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return String.Empty;
            }
            var time = (PreciseTimeSpan)value;
            
            return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}.{time.Nanoseconds:000000000}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

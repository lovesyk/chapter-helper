using System;
using System.Globalization;
using System.Windows.Data;

namespace ChapterHelper.Converter
{
    [ValueConversion(typeof(int), typeof(string))]
    public class PositiveIntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int input = (int)value;
            if (input < 0)
            {
                return String.Empty;
            }

            return input.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result;
            if (Int32.TryParse((string)value, out result))
            {
                return result;
            }
            return null;
        }
    }
}

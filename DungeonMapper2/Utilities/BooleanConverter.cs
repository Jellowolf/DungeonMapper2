using System;
using System.Globalization;
using System.Windows.Data;

namespace DungeonMapper2.Utilities
{
    public abstract class BooleanConverter<T> : IValueConverter
    {
        public bool Reverse { get; set; }

        public T True { get; set; }
        public T False { get; set; }

        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = value as bool? ?? false;
            return (Reverse ? !boolValue : boolValue) ? True : False;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && Equals((T)value, Reverse ? False : True);
        }
    }
}

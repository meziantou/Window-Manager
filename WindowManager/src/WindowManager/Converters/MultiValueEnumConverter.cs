using System;
using System.Globalization;
using System.Windows.Data;

namespace WindowManager.Converters
{
    public class MultiValueEnumConverter : IValueConverter
    {
        private ulong _value;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            _value = System.Convert.ToUInt64(value, CultureInfo.InvariantCulture);
            var param = System.Convert.ToUInt64(parameter, CultureInfo.InvariantCulture);
            var @equals = Equals(_value & param, param);
            return @equals;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ulong result;
            if (Equals((bool)value, true))
                result = _value | System.Convert.ToUInt64(parameter, CultureInfo.InvariantCulture);
            else
                result = _value & (~System.Convert.ToUInt64(parameter, CultureInfo.InvariantCulture));

            var o = Enum.ToObject(targetType, result);
            return o;
        }
    }
}
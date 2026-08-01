using System;
using System.Globalization;
using System.Windows.Data;

namespace RAIDAR_FRONT.Converters
{

    /// bool 값(true/false)을 지정된 텍스트 문자열로 변환하는 WPF ValueConverter

    public class BooleanToTextConverter : IValueConverter
    {
        public string TrueText { get; set; } = "연결 중";
        public string FalseText { get; set; } = "연결 끊김";
        public string NullText { get; set; } = "N/A";

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueText : FalseText;
            }

            return NullText;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                if (string.Equals(strValue, TrueText, StringComparison.OrdinalIgnoreCase)) return true;
                if (string.Equals(strValue, FalseText, StringComparison.OrdinalIgnoreCase)) return false;
            }
            return false;
        }
    }
}

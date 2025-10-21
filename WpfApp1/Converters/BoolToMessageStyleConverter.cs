// Converters/BoolToMessageStyleMultiConverter.cs
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data; // Needed for IMultiValueConverter

namespace Alex_Mai.Converters
{
    public class BoolToMessageStyleMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] should be the boolean (IsSentByUser)
            // values[1] should be the FrameworkElement (the UserControl/ChatView)
            if (values != null && values.Length == 2 &&
                values[0] is bool isSentByUser &&
                values[1] is FrameworkElement element) // Get the UserControl instance
            {
                string styleKey = isSentByUser ? "SentMessageStyle" : "ReceivedMessageStyle";
                // Find the resource starting from the UserControl passed as values[1]
                if (element.TryFindResource(styleKey) is Style style)
                {
                    return style;
                }
            }
            // Fallback if conversion fails
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            // Not needed for one-way conversion
            throw new NotImplementedException();
        }
    }
}
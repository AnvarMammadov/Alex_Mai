using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Alex_Mai.Converters
{
    public class CountToVisibilityConverter : IValueConverter
    {
        // Parameter:
        // "0" (or null/default): Visible if count IS 0
        // "NotZero": Visible if count is NOT 0
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool showWhenZero = true; // Default behavior
            if (parameter != null && parameter.ToString().Equals("NotZero", StringComparison.OrdinalIgnoreCase))
            {
                showWhenZero = false;
            }

            if (value is int count)
            {
                if (showWhenZero)
                {
                    return count == 0 ? Visibility.Visible : Visibility.Collapsed;
                }
                else // Show when not zero
                {
                    return count != 0 ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            // If value is not int, decide based on showWhenZero (usually hide)
            return showWhenZero ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
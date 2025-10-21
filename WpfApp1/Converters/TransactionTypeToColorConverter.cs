using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Alex_Mai.Models;

namespace Alex_Mai.Converters
{
    public class TransactionTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TransactionType type)
            {
                // Gəlir üçün yaşıl, xərc üçün qırmızımtıl (və ya ağ)
                return type == TransactionType.Income ? Brushes.LightGreen : Brushes.Salmon; // Və ya Brushes.WhiteSmoke
            }
            return Brushes.White; // Default
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
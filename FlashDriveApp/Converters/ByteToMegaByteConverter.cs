using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FlashDriveApp.Converters
{
    public class ByteToMegaByteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double byteValue = System.Convert.ToDouble(value);
            double megaByteValie = byteValue / 1024 / 1024;
            return $"{Math.Round(megaByteValie,2)} Mb";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

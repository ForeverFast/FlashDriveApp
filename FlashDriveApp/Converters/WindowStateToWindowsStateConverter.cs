using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace FlashDriveApp.Converters
{
    public class WindowStateToWindowsStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            WindowState windowState = (WindowState)value;

            switch (windowState)
            {
                case WindowState.Normal:
                case WindowState.Minimized:
                    return WindowState.Maximized;
                case WindowState.Maximized:
                    return WindowState.Normal;
            }

            return WindowState.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
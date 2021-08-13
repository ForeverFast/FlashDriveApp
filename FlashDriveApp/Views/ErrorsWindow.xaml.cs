using FlashDriveApp.ViewModels;
using System.Windows;

namespace FlashDriveApp.Views
{
    /// <summary>
    /// Логика взаимодействия для ErrorsWindow.xaml
    /// </summary>
    public partial class ErrorsWindow : Window
    {
        public ErrorsWindow(ErrorsViewModel errorsViewModel)
        {
            InitializeComponent();
            DataContext = errorsViewModel;
        }
    }
}

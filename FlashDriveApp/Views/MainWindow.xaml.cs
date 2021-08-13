using FlashDriveApp.ViewModels;
using System.Windows;

namespace FlashDriveApp.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(FlashDriveViewModel flashDriveViewModel)
        {
            InitializeComponent();
            DataContext = flashDriveViewModel;
        }
    }
}

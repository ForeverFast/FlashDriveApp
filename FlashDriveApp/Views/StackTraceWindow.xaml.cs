using System.Windows;

namespace FlashDriveApp.Views
{
    /// <summary>
    /// Логика взаимодействия для StackTraceWindow.xaml
    /// </summary>
    public partial class StackTraceWindow : Window
    {
        public StackTraceWindow(string trace)
        {
            InitializeComponent();
            TB.Text = trace;
        }
    }
}

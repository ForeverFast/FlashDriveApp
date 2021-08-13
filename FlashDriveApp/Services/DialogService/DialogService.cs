using FlashDriveApp.Models;
using FlashDriveApp.ViewModels;
using FlashDriveApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FlashDriveApp.Services
{
    public class DialogService : IDialogService
    {
        private readonly ErrorsViewModel _errorsViewModel;

        public bool ShowYesNoDialog(string message, string caption = "Оповещение")
        {
            var result = MessageBox.Show(message,
                caption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
                return true;
            else
                return false;
        }

        public void ShowErrorList(List<ErrorReport> errors)
        {
            ErrorsWindow errorsWindow = new ErrorsWindow(_errorsViewModel);
            _errorsViewModel.ResetData(errors, this);
            errorsWindow.ShowDialog();
            _errorsViewModel.ErrorReports.Clear();

            errorsWindow = null;
            GC.Collect();
        }

        public void ShowStakeTrace(string Trace)
        {
            StackTraceWindow stackTraceWindow = new StackTraceWindow(Trace);
            stackTraceWindow.ShowDialog();

            stackTraceWindow = null;
            GC.Collect();
        }

        public DialogService(ErrorsViewModel errorsViewModel)
        {
            _errorsViewModel = errorsViewModel;
        }
    }
}

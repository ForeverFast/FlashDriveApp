using FlashDriveApp.Commands;
using FlashDriveApp.Models;
using FlashDriveApp.Services;
using FlashDriveApp.ViewModels.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FlashDriveApp.ViewModels
{
    public class ErrorsViewModel : BaseViewModel
    {
        private IDialogService _dialogService;

        public ObservableCollection<ErrorReport> ErrorReports { get; set; }

        #region Команды

        public ICommand OpenStackTraceCommand { get; }

        private void OpenStackTraceExecute(object parameter)
        {
            var er = (ErrorReport)parameter;
            _dialogService.ShowStakeTrace(er.Trace);
        }

        #endregion


        public ErrorsViewModel()
        {
          
            ErrorReports = new ObservableCollection<ErrorReport>();

            OpenStackTraceCommand = new RelayCommand(OpenStackTraceExecute);
        }

        public void ResetData(List<ErrorReport> errors, IDialogService dialogService)
        {
            _dialogService = dialogService;

            ErrorReports.Clear();
            errors.ForEach(x => ErrorReports.Add(x));
        }
    }
}

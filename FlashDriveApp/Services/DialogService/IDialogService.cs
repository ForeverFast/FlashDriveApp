using FlashDriveApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashDriveApp.Services
{
    public interface IDialogService
    {
        bool ShowYesNoDialog(string message, string caption = "Оповещение");
        void ShowErrorList(List<ErrorReport> errors);
        void ShowStakeTrace(string Trace);
    }
}

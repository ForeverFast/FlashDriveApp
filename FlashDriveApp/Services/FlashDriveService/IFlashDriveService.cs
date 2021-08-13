using FlashDriveApp.Extentions;
using FlashDriveApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashDriveApp.Services
{
    public interface IFlashDriveService
    {
        FlashDrive GetDriveEntity(string path);
        List<FlashDrive> GetDrives();
        long GetSize(string path);

        public void SetFlashDriveStates(FlashDrive flash, string path);
        DataState GetDataState(string path, string root);
        bool StartRecording(string path, List<FlashDrive> flashDrives);

        void ShowErrors();
    }
}

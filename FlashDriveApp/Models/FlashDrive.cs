using FlashDriveApp.Extentions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashDriveApp.Models
{
    public class FlashDrive : OnPropertyChangedClass
    {
        #region Поля
        private DriveInfo _driveInfo;
        private bool _isCopyToNeed;
        private FlashDriveState _state;
        private DataState _dataState;
        private bool _isBusy;
        private int _currentPos;
        #endregion

        public string Path { get => DriveInfo.RootDirectory.FullName; }

        public long AvailableFreeSpace { get => DriveInfo.AvailableFreeSpace; }

        public long TotalSize { get => DriveInfo.TotalSize; }
        public bool IsCopyToNeed { get => _isCopyToNeed; set => SetProperty(ref _isCopyToNeed, value); }

        public FlashDriveState State { get => _state; set => SetProperty(ref _state, value); }

        public DataState DataState { get => _dataState; set => SetProperty(ref _dataState, value); }

        public DriveInfo DriveInfo { get => _driveInfo; set => SetProperty(ref _driveInfo, value); }


        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
        public int CurrentPos { get => _currentPos; set => SetProperty(ref _currentPos, value); }
    }
}

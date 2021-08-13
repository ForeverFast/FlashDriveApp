using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Input;
using FlashDriveApp.Commands;
using FlashDriveApp.Extentions;
using FlashDriveApp.Models;
using FlashDriveApp.Services;
using FlashDriveApp.ViewModels.Abstractions;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Threading;
using System.ComponentModel;

namespace FlashDriveApp.ViewModels
{
    public class FlashDriveViewModel : BaseViewModel
    {
        private readonly IFlashDriveService _flashDriveService;
        private readonly IDialogService _dialogService;

        #region Поля
        private string _targetFilePath;
        private long _size;
        #endregion

        /// <summary>
        /// Путь к файлу/папки для копирования
        /// </summary>
        public string TargetFilePath { get => _targetFilePath; set => SetProperty(ref _targetFilePath, value); }
        /// <summary>
        /// Размер файла/папки
        /// </summary>
        public long Size { get => _size; set => SetProperty(ref _size, value); }

        /// <summary>
        /// Список доступных флешек
        /// </summary>
        public ObservableCollection<FlashDrive> FlashDrives { get; }


        #region Команды
        public ICommand GetDriversCommand { get; }
        public ICommand OpenDialogCommand { get; }
        public ICommand ChangeCopyToStateCommand { get; }
        public ICommand StartRecordingCommand { get; }
        
        private void GetDriversExecute(object parameter)
        {
            FlashDrives.Clear();
            foreach (var drive in _flashDriveService.GetDrives())
            {
                _flashDriveService.SetFlashDriveStates(drive, TargetFilePath);
                FlashDrives.Add(drive);
            }   
        }

        private void OpenDialogExecute(object parameter)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                TargetFilePath = dlg.FileName;
                Size = _flashDriveService.GetSize(TargetFilePath);
              
                foreach (var item in FlashDrives)
                {
                    _flashDriveService.SetFlashDriveStates(item, TargetFilePath);
                }
            }
                
        }

        private void ChangeCopyToStateExecute(object parameter)
        {
            if (parameter != null)
            {
                FlashDrive flashDrive = (FlashDrive)parameter;
                if (flashDrive.State == FlashDriveState.Ready)
                {
                    flashDrive.IsCopyToNeed = !flashDrive.IsCopyToNeed;
                }
            }
        }

        private void StartRecordingExecute(object parameter)
        {
            IsBusy = true; 
            if (string.IsNullOrEmpty(TargetFilePath))
                return;

            var checkedList = new List<FlashDrive>();
            bool fileAlreadyExistFlag = false;

            foreach (var flashDrive in FlashDrives)
            {
                if (flashDrive.DataState == DataState.AlreadyExist)
                    fileAlreadyExistFlag = true;

                if (flashDrive.IsCopyToNeed == true && flashDrive.State == FlashDriveState.Ready)
                    checkedList.Add(flashDrive);
            }

            if (fileAlreadyExistFlag)
                if (!_dialogService.ShowYesNoDialog("На некоторых носителях уже есть такой файл. Перезаписать?\r\n" +
                                                   "(Текущие данные буду утеряны)"))
                {
                    foreach (var flashDrive in checkedList.ToArray())
                    {
                        if (flashDrive.DataState == DataState.AlreadyExist)
                            checkedList.Remove(flashDrive);
                    }
                }

            if (checkedList.Count > 0)
            {
                BackgroundWorker bgw = new BackgroundWorker();
                bgw.DoWork += (o, e) =>
                {
                    _flashDriveService.StartRecording(TargetFilePath, checkedList);
                };
                bgw.RunWorkerCompleted += (o, e) => {
                    IsBusy = false;
                    FlashDrives.ToList().ForEach(fd => _flashDriveService.SetFlashDriveStates(fd, TargetFilePath));
                };
                bgw.RunWorkerAsync();
            }
               
           
        }

        

        #endregion


        #region События 

        private readonly ManagementEventWatcher watcher;

        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            UInt16 eventType = (UInt16)e.NewEvent["EventType"];
            switch (eventType)
            {
                case 2:
                    var newDrive = _flashDriveService.GetDriveEntity((string)e.NewEvent["DriveName"]);
                    if (newDrive != null)
                        App.Current.Dispatcher.BeginInvoke(() => 
                        {
                            _flashDriveService.SetFlashDriveStates(newDrive, TargetFilePath);
                            FlashDrives.Add(newDrive);
                        });

                    break;

                case 3:
                    var path = (string)e.NewEvent["DriveName"]+"\\";
                    App.Current.Dispatcher.BeginInvoke(() =>
                    {
                        FlashDrives.Remove(FlashDrives.FirstOrDefault(x => x.Path == path));
                    });
                    break;
            }
        }

        #endregion


        public FlashDriveViewModel(IFlashDriveService flashDriveService,
                                   IDialogService dialogService) : base()
        {
            _flashDriveService = flashDriveService;
            _dialogService = dialogService;
            watcher = new ManagementEventWatcher();

            FlashDrives = new ObservableCollection<FlashDrive>();

            GetDriversCommand = new RelayCommand(GetDriversExecute, (o) => !IsBusy);
            OpenDialogCommand = new RelayCommand(OpenDialogExecute, (o) => !IsBusy);
            ChangeCopyToStateCommand = new RelayCommand(ChangeCopyToStateExecute, (o) => !IsBusy);
            StartRecordingCommand = new RelayCommand(StartRecordingExecute, (o) => !IsBusy);

           
            var query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 3 OR EventType = 2");
            watcher.Query = query;
            watcher.EventArrived += Watcher_EventArrived;
            watcher.Query = query;
            watcher.Start();
        }
    }
}

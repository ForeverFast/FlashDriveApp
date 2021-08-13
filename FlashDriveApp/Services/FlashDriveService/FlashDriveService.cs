using FlashDriveApp.Extentions;
using FlashDriveApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace FlashDriveApp.Services
{
    public class FlashDriveService : IFlashDriveService
    {
        private readonly IDialogService _dialogService;

        private const int BUFFER = 1024 * 1024 * 50;
        private double pgIncrementValue = 0;
        
        public List<ErrorReport> LastRecordErrors { get; set; }

        public FlashDrive GetDriveEntity(string path)
        {
            DriveInfo[] D = DriveInfo.GetDrives();
            var drive = D.FirstOrDefault(d => d.Name == path+"\\");
            if (drive.DriveType == DriveType.Removable && drive.IsReady == true)
            {
                var flashDrive = new FlashDrive();
                flashDrive.DriveInfo = drive;

                return flashDrive;
            }
            else
                return null;
        }

        public List<FlashDrive> GetDrives()
        {
            var list = new List<FlashDrive>();
            DriveInfo[] D = DriveInfo.GetDrives();
            foreach (DriveInfo DI in D)
            {
                
                if (DI.DriveType == DriveType.Removable && DI.IsReady == true)
                {
                    var item = new FlashDrive();
                    item.DriveInfo = DI;
                    
                    list.Add(item);
                }
      
            }

            return list;
        }

        public long GetSize(string path)
        {
            if (File.Exists(path))
            {
                return new FileInfo(path).Length;
            }
            else if (Directory.Exists(path))
            {
                return FolderSize(path);
            }

            return 0;
        }

        private long FolderSize(string path)
        {
            long i = 0;
            foreach (FileInfo file in new DirectoryInfo(path).GetFiles())
            {
                i += file.Length;
            }
            foreach (string s in Directory.GetDirectories(path))
            {
                i += FolderSize(s);
            }

            return i;
        }

        public DataState GetDataState(string path, string root)
        {
            var fileName = Path.GetFileName(path);
            return Directory.Exists(@$"{root}\{fileName}")
                ? DataState.AlreadyExist
                : DataState.NoExist;
        }

        public bool StartRecording(string path, List<FlashDrive> flashDrives)
        {
            if (File.Exists(path))
            {
                var fileSize = this.GetSize(path);
                foreach (FlashDrive flashDrive in flashDrives)
                {
                    try
                    {
                        if (flashDrive.AvailableFreeSpace > fileSize)
                        {
                            flashDrive.State = FlashDriveState.Record;
                            string targetPath = $@"{flashDrive.Path}\{Path.GetFileName(path)}";
                
                            if (File.Exists(targetPath))
                                File.Delete(targetPath);
                            
                            byte[] buffer = new byte[BUFFER];
                            using(FileStream writer = new FileStream(targetPath, FileMode.Open, FileAccess.Write),
                                reader = new FileStream(path, FileMode.Open, FileAccess.Read))
                            {
                                long read_len = reader.Length;
                                while (reader.Position < read_len)
                                {
                                    if (reader.Position + BUFFER > read_len)
                                        buffer = new byte[read_len - reader.Position];

                                    reader.Read(buffer, 0, buffer.Length);
                                    writer.Write(buffer, 0, buffer.Length);

                                    flashDrive.CurrentPos = (int)( (double)reader.Position / (double)read_len * 100.0);
                                }
                            }

                            buffer = null;
                            GC.Collect();

                            flashDrive.DataState = DataState.AlreadyExist;
                            flashDrive.OnAllPropertyChanged();
                        }
                        else
                        {
                            flashDrive.State = FlashDriveState.NotEnoughSpace;
                            //errorList.Add($"На флешке {flashDrive.Path} не хватает памяти для записи.");
                        }
                    }
                    catch (Exception ex)
                    {
                        flashDrive.State = FlashDriveState.Error;
                        //errorList.Add($"На флешке {flashDrive.Path} возникла ошибка:\r\n{ex}");
                    }

                }

               
            }
            else
            if (Directory.Exists(path))
            {
                pgIncrementValue = Math.Round(100.0 / GetMaxProgressBarValue(path), 2);

                var dirSize = this.GetSize(path);

                var threads = new List<Thread>();

                foreach (FlashDrive fd in flashDrives)
                {
                    Action<object> action = (o) =>
                    {
                        var flashDrive = (FlashDrive)o;
                        flashDrive.CurrentPos = 0;
                        if (flashDrive.AvailableFreeSpace > dirSize)
                        {
                            string targetPath = $@"{flashDrive.Path}\{Path.GetFileName(path)}";
                            if (Directory.Exists(targetPath))
                            {
                                flashDrive.State = FlashDriveState.DeleteData;
                                Directory.Delete(targetPath, true);
                            }
                            flashDrive.State = FlashDriveState.Record;

                            var localErrors = new List<ErrorReport>();
                            CopyDir(path, targetPath, flashDrive, localErrors);

                            if (localErrors.Count > 0)
                            {
                                LastRecordErrors.AddRange(localErrors);
                                flashDrive.DataState = DataState.RecordError;
                            }
                            else
                            {
                                flashDrive.CurrentPos = 100;                             
                                flashDrive.DataState = DataState.AlreadyExist;
                            }

                        }
                        else
                        {
                            flashDrive.State = FlashDriveState.NotEnoughSpace;
                            LastRecordErrors.Add(new ErrorReport()
                            {
                                Path = path,
                                Message = $"На флешке {flashDrive.Path} не хватает памяти для записи."
                            });
                        }

                        flashDrive.OnAllPropertyChanged();
                    };

                    Thread thread = new Thread(new ParameterizedThreadStart(action));
                    threads.Add(thread);
                    thread.Start(fd);
                }

                foreach (var thread in threads)
                    thread.Join();
            }

            this.ShowErrors();

            return true;
        }

        private int GetMaxProgressBarValue(string path)
        {
            int temp = 0;
            Directory.GetDirectories(path).ToList().ForEach(x => temp += GetMaxProgressBarValue(x) + 1);
            return temp;
        }

        private ErrorReport CreateErrorReport(string path, Exception ex)
        {
            return new ErrorReport()
            {
                Path = path,
                Message = ex.Message,
                Trace = ex.StackTrace
            };
        }

        private void CopyDir(string FromDir, string ToDir, FlashDrive flashDrive, List<ErrorReport> errors)
        {
            
            try
            {
                if (!Directory.Exists(ToDir))
                    Directory.CreateDirectory(ToDir);
            }
            catch (Exception ex)
            {
                errors.Add(CreateErrorReport(ToDir, ex));
            }

            int pgTempIncrementValue = (int)(pgIncrementValue / Directory.GetFiles(FromDir).Count());

            foreach (string s1 in Directory.GetFiles(FromDir))
            {
                string s2 = ToDir + "\\" + Path.GetFileName(s1);
                try
                {
                    File.Copy(s1, s2);
                    flashDrive.CurrentPos += pgTempIncrementValue;
                }
                catch(UnauthorizedAccessException ex)
                {
                    errors.Add(CreateErrorReport(s2, ex));
                }
                catch (DirectoryNotFoundException ex)
                {
                    errors.Add(CreateErrorReport(s2, ex));

                }
                catch (FileNotFoundException ex)
                {
                    errors.Add(CreateErrorReport(s2, ex));
                }
                catch (Exception ex)
                {
                    errors.Add(CreateErrorReport(s2, ex));
                }

            }
            foreach (string s in Directory.GetDirectories(FromDir))
            {
                CopyDir(s, ToDir + "\\" + Path.GetFileName(s), flashDrive, errors);
            }
        }

        public void SetFlashDriveStates(FlashDrive drive, string path)
        {
            long size = this.GetSize(path);

            if (drive.AvailableFreeSpace > size)
            {
                drive.State = FlashDriveState.Ready;
            }
            else if (drive.AvailableFreeSpace <= size)
            {
                drive.State = FlashDriveState.NotEnoughSpace;
            }

            if (string.IsNullOrEmpty(path))
            {
                drive.DataState = DataState.NoInputFile;
            }
            else
            {
                drive.DataState = this.GetDataState(path, drive.Path);
            }
        }
        public void ShowErrors()
        {
            if (LastRecordErrors.Count > 0)
            {
                App.Current.Dispatcher.BeginInvoke(() => _dialogService.ShowErrorList(LastRecordErrors));
            }
        }

        

        public FlashDriveService(IDialogService dialogService)
        {
            _dialogService = dialogService;

            LastRecordErrors = new List<ErrorReport>();
        }
    }
}

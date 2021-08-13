using FlashDriveApp.Extentions;

namespace FlashDriveApp.ViewModels.Abstractions
{
    public abstract class BaseViewModel : OnPropertyChangedClass
    {
        protected bool _isBusy;
        protected bool _isLoaded;
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
        public bool IsLoaded { get => _isLoaded; set => SetProperty(ref _isLoaded, value); }
    }
}

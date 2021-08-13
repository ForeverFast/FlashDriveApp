using FlashDriveApp.Services;
using FlashDriveApp.ViewModels;
using FlashDriveApp.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FlashDriveApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFlashDriveService, FlashDriveService>();
            services.AddSingleton<IDialogService, DialogService>();

            services.AddSingleton(typeof(FlashDriveViewModel));
            services.AddSingleton(typeof(MainWindow));

            services.AddSingleton(typeof(ErrorsViewModel));
        }
    }
}

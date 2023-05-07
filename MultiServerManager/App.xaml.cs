using System.IO;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MultiServerManager.Common;
using MultiServerManager.Service;
using MultiServerManager.ViewModel;
using System.Windows;
using MultiServerManager.Model.Config;
using MultiServerManager.Utils.Extensions;
using System;

namespace MultiServerManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class App : Application
    {
        public App()
        {
            var services = new ServiceCollection()
                .AddViewModel(new HomeViewModel
                {
                    ViewModelName = "Home",
                    ViewModelDescription = "Home",
                    Icon = "Home"
                })
                .AddViewModel(x => new ServerListViewModel(
                    x.GetRequiredService<ServerManagementService>(),
                    x.GetRequiredService<SnackbarService>())
                {
                    ViewModelName = "ServerList",
                    ViewModelDescription = "Server List Description",
                    Icon = "Server",
                    VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled,
                    HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled
                })
                .AddViewModel(x => new ConsoleViewModel(
                    x.GetRequiredService<ServerManagementService>())
                {
                    ViewModelName = "Console",
                    ViewModelDescription = "Server Console Description",
                    Icon = "Console",
                    VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled,
                    HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled
                })
                .AddViewModel(new SettingsViewModel
                {
                    ViewModelName = "Settings",
                    ViewModelDescription = "Settings Description",
                    Icon = "Cog"
                });

            services
                .AddSingleton(x => new NavigationService(x.GetServices<ViewModelBase>()))
                .AddSingleton<SnackbarService>()
                .AddSingleton<ServerManagementService>();

            Ioc.Default.ConfigureServices(services.BuildServiceProvider());


            Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "msm"));
            MSMConfig.Load();

            Directory.CreateDirectory(MSMConfig.Instance.GetTShockExecutableDirectoryPath());
            Directory.CreateDirectory(MSMConfig.Instance.GetServersDirectoryPath());
            Directory.CreateDirectory(MSMConfig.Instance.GetWorldsDirectoryPath());
            Directory.CreateDirectory(MSMConfig.Instance.GetPluginsDirectoryPath());
            
            InitializeComponent();
        }
    }
}

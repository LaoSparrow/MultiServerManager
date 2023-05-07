using System;
using CommunityToolkit.Mvvm.Input;
using MultiServerManager.Common;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.DependencyInjection;
using MultiServerManager.Core;
using MultiServerManager.Service;
using MultiServerManager.Utils.Extensions;

namespace MultiServerManager.ViewModel
{
    public partial class ServerListViewModel : ViewModelBase
    {
        private Lazy<NavigationService> LazyNavigationService { get; } =
            new(() => Ioc.Default.GetRequiredService<NavigationService>());
        private ServerManagementService ServerManagementService { get; }
        private SnackbarService SnackbarService { get; }

        public ObservableCollection<ServerContainer> Servers
        {
            get => ServerManagementService.Servers;
            set => ServerManagementService.Servers = value;
        }

        public ServerContainer? CurrentActiveServer
        {
            get => ServerManagementService.CurrentActiveServer;
            set => ServerManagementService.CurrentActiveServer = value;
        }

        public ServerListViewModel(ServerManagementService serverManagement, SnackbarService snackbar)
        {
            ServerManagementService = serverManagement;
            SnackbarService = snackbar;

            PropertyChangedEventManager.AddHandler(
                ServerManagementService,
                (_, _) => OnPropertyChanged(nameof(Servers)),
                nameof(ServerManagementService.Servers));
            PropertyChangedEventManager.AddHandler(
                ServerManagementService,
                (_, _) => OnPropertyChanged(nameof(CurrentActiveServer)),
                nameof(ServerManagementService.CurrentActiveServer));
        }

        [RelayCommand]
        private void AddServer() => Servers.Add(new ServerContainer { Id = NextAvailableID() });

        [RelayCommand]
        private void RemoveServer()
        {
            var query = Servers.Where(x => x.IsSelected).ToArray();
            foreach (var i in query)
            {
                Servers.Remove(i);
            }
        }

        [RelayCommand]
        private void StartServer()
        {
            Servers.Where(x => x.IsSelected).ForEach(x => x.IsRunning = true);
        }

        [RelayCommand]
        private void SaveServerList()
        {
            ServerManagementService.ApplyChangesToConfig();
            SnackbarService.MessageQueue.Enqueue("Saved!");
        }

        [RelayCommand]
        private void QuickActionConsole(ServerContainer container)
        {
            CurrentActiveServer = container;
            LazyNavigationService.Value.NavigateTo<ConsoleViewModel>();
        }

        [RelayCommand]
        private void QuickActionPlugins(ServerContainer container)
        {
            //TODO: Navigate to plugins
            //CurrentActiveServer = container;
            //NavigationService.NavigateTo<ConsoleViewModel>();
        }

        private int NextAvailableID() => Servers.Count == 0 ? 1 : Servers.Max(x => x.Id) + 1;
    }
}

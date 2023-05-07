using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using MultiServerManager.Common;
using MultiServerManager.Core;
using MultiServerManager.Service;

// ReSharper disable UnusedMember.Local

namespace MultiServerManager.ViewModel;

public partial class ConsoleViewModel : ViewModelBase
{
    private ServerManagementService ServerManagementService { get; }

    public ConsoleViewModel(ServerManagementService serverManagement)
    {
        ServerManagementService = serverManagement;


        PropertyChangedEventManager.AddHandler(
            ServerManagementService,
            (_, _) => OnPropertyChanged(nameof(RunningServers)),
            nameof(ServerManagementService.Servers));
        PropertyChangedEventManager.AddHandler(
            ServerManagementService,
            (_, _) => OnPropertyChanged(nameof(CurrentActiveServer)),
            nameof(ServerManagementService.CurrentActiveServer));
    }

    public ServerContainer[] RunningServers => ServerManagementService.Servers.Where(x => x.IsRunning).ToArray();

    //[ObservableProperty]
    //private ServerContainer server = new();

    public ServerContainer? CurrentActiveServer
    {
        get => ServerManagementService.CurrentActiveServer;
        set => ServerManagementService.CurrentActiveServer = value;
    }

    [RelayCommand]
    private void AddTextToConsole(string text)
    {
        if (!CurrentActiveServer?.IsRunning ?? false)
            return;

        CurrentActiveServer?.SendText(text);
    }

    [RelayCommand]
    private void ClearConsole() => CurrentActiveServer?.ConsoleLines.Clear();
}
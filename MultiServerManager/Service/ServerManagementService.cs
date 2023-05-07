using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using MultiServerManager.Core;
using MultiServerManager.Model.Config;

namespace MultiServerManager.Service;

public partial class ServerManagementService : ObservableObject
{
    public ServerManagementService()
    {
        servers = new ObservableCollection<ServerContainer>(ServerListConfig.Instance.Servers);
    }

    [ObservableProperty]
    private ObservableCollection<ServerContainer> servers;

    [ObservableProperty]
    private ServerContainer? currentActiveServer;

    public void ApplyChangesToConfig()
    {
        ServerListConfig.Instance.Servers = servers.ToList();
        ServerListConfig.Save();
    }
}
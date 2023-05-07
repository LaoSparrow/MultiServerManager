using System;
using System.Collections.Generic;
using System.IO;
using MultiServerManager.Common;
using MultiServerManager.Core;

namespace MultiServerManager.Model.Config;

public class ServerListConfig : ConfigBase<ServerListConfig>
{
    protected override string ConfigFilePath => Path.Combine(AppContext.BaseDirectory, "msm", "servers.json");

    public List<ServerContainer> Servers = new();
}
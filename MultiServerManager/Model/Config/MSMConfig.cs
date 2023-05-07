using System;
using System.IO;
using MultiServerManager.Common;
using Newtonsoft.Json;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertToConstant.Global

namespace MultiServerManager.Model.Config;

public class MSMConfig : ConfigBase<MSMConfig>
{
    protected override string ConfigFilePath => Path.Combine(AppContext.BaseDirectory, "msm", "msm.json");

    // ReSharper disable once InconsistentNaming
    [JsonProperty("tshock_executable_directory")]
    public string TShockExecutableDirectory = "tshock";
    [JsonProperty("servers_directory")]
    public string ServersDirectory = "servers";
    [JsonProperty("worlds_directory")]
    public string WorldsDirectory = "worlds";
    [JsonProperty("plugins_directory")]
    public string PluginsDirectory = "plugins";

    public string GetTShockExecutableDirectoryPath() => Path.GetFullPath(TShockExecutableDirectory, AppContext.BaseDirectory);
    public string GetServersDirectoryPath() => Path.GetFullPath(ServersDirectory, AppContext.BaseDirectory);
    public string GetWorldsDirectoryPath() => Path.GetFullPath(WorldsDirectory, AppContext.BaseDirectory);
    public string GetPluginsDirectoryPath() => Path.GetFullPath(PluginsDirectory, AppContext.BaseDirectory);
}
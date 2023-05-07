using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using MultiServerManager.Common.Collections;
using MultiServerManager.Model.Config;
using MultiServerManager.Utils.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MultiServerManager.Core;

public enum TileProviderType
{
    Stock,
    HeapTile,
    Constileation,
}

public record ServerConsoleLine(string Text, Color Foreground, Color Background);

[JsonObject(MemberSerialization.OptIn)]
public partial class ServerContainer : ObservableObject, IDisposable
{
    [ObservableProperty]
    private bool isSelected;

    #region Infos

    [JsonProperty]
    [ObservableProperty]
    private int id;

    [JsonProperty]
    [ObservableProperty]
    private string name = "New Server";

    [JsonProperty]
    [ObservableProperty]
    private string description = "";

    [JsonProperty("tshock_version")]
    [ObservableProperty]
    private string tshockVersion = "5.1.3";

    // Args

    [JsonProperty]
    [ObservableProperty]
    private string ip = "0.0.0.0";

    [JsonProperty]
    [ObservableProperty]
    private ushort port = 7777;

    [JsonProperty]
    [ObservableProperty]
    private string world = "";

    [JsonProperty("max_players")]
    [ObservableProperty]
    private uint maxPlayers = 8;

    [JsonProperty]
    [ObservableProperty]
    private int lang = 7;

    [JsonProperty("tile_provider")]
    [JsonConverter(typeof(StringEnumConverter))]
    [ObservableProperty]
    private TileProviderType tileProvider = TileProviderType.Stock;

    [JsonProperty("additional_args")]
    [ObservableProperty]
    private List<string> additionalArgs = new();
    #endregion


    private const int MAX_HISTORY_LINES = 2048;

    [ObservableProperty]
    private ObservableQueue<ServerConsoleLine> consoleLines = new();

    [ObservableProperty]
    private string title = "New Terraria Server";

    public bool IsRunning
    {
        get => process != null;
        set
        {
            if (value && !IsRunning)
            {
                Start();
                OnPropertyChanged();
            }
            else if (!value && IsRunning)
            {
                process!.Kill();
                OnPropertyChanged();
            }
        }
    }

    private Process? process;
    private Color foreground = Colors.LightGray;
    private Color background = Colors.Black;


    public void Start()
    {
        Directory.CreateDirectory(Path.Combine(MSMConfig.Instance.GetServersDirectoryPath(), Name));

        process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "MultiServerManager.TShockBootstrap.exe",
                ArgumentList =
                {
                    "--bootstrap-server-executable", Path.Combine(MSMConfig.Instance.GetTShockExecutableDirectoryPath(), TshockVersion, "TShock.Server.exe"),
                    "--bootstrap-server-plugins-directory", MSMConfig.Instance.GetPluginsDirectoryPath(),
                    "--bootstrap-server-ignored-plugins-file", Path.Combine(MSMConfig.Instance.GetServersDirectoryPath(), Name, "ignoredplugins.txt"),
                    "--bootstrap-parent-process-id", Environment.ProcessId.ToString(),
                    "-ip", Ip,
                    "-port", Port.ToString(),
                    "-world", Path.Combine(MSMConfig.Instance.GetWorldsDirectoryPath(), World),
                    "-maxplayers", MaxPlayers.ToString(),
                    "-lang", Lang.ToString(),
                    TileProvider.GetArg()
                },
                WorkingDirectory = Path.Combine(MSMConfig.Instance.GetServersDirectoryPath(), Name),
#if DEBUG
                CreateNoWindow = false,
#else
                CreateNoWindow = true,
#endif
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardInputEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            },
            EnableRaisingEvents = true
        };
        AdditionalArgs.ForEach(x => process.StartInfo.ArgumentList.Add(x));
        process.Exited += delegate
        {
            AppendConsoleLine($"---process exited with code = {process!.ExitCode}---\n");
            process!.Dispose();
            process = null;
            OnPropertyChanged(nameof(IsRunning));
        };
        process.OutputDataReceived += delegate (object _, DataReceivedEventArgs args)
        {
            AppendConsoleLine(args.Data + "\n");
        };
        process.ErrorDataReceived += delegate (object _, DataReceivedEventArgs args)
        {
            AppendConsoleLine(args.Data + "\n");
        };
        process.Start();
        process.StandardInput.AutoFlush = true;
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.StandardInput.WriteLine();
    }

    public void SendText(string text)
    {
        if (process == null)
            throw new InvalidOperationException();

        AppendConsoleLine(text + "\n");
        process.StandardInput.WriteLine(text);
    }

    private void AppendConsoleLine(string text)
    {
        var fg = foreground;
        var bg = background;
        var i = text.IndexOf('\u0001');
        if (i != -1)
        {
            var flag = text.Substring(i + 1, 5);
            var data = text.Substring(i + 6).TrimEnd();
            switch (flag)
            {
                case "title":
                    Title = data;
                    break;
                case "fgclr":
                    foreground = (Color?)typeof(Colors).GetProperty(data)?.GetValue(null) ?? Colors.LightGray;
                    break;
                case "bgclr":
                    background = (Color?)typeof(Colors).GetProperty(data)?.GetValue(null) ?? Colors.Black;
                    break;
            }

            text = text.Substring(0, i);
            if (string.IsNullOrEmpty(text))
                return;
        }

        ConsoleLines.Enqueue(new ServerConsoleLine(text, fg, bg));
        if (ConsoleLines.Count > MAX_HISTORY_LINES)
            Enumerable.Range(0, MAX_HISTORY_LINES / 2).ForEach(_ => ConsoleLines.Dequeue());
    }

    public void Dispose()
    {
        process?.Kill();
        process?.Dispose();
        GC.SuppressFinalize(this);
    }
}

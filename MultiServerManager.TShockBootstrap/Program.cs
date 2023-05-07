using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using SingleFileExtractor.Core;
// ReSharper disable MemberCanBePrivate.Global

namespace MultiServerManager.TShockBootstrap
{
    internal static class Program
    {
        private const string NATIVE_LIBRARY_EXTRACT_PATH = "native";

        private static string serverExecutablePath = Path.GetFullPath("TShock.Server.exe");
        private static string serverPluginsDirectoryPath = Path.Combine(AppContext.BaseDirectory, "ServerPlugins");
        private static string serverIgnoredPluginsFilePath =
            Path.Combine(AppContext.BaseDirectory, "ServerPlugins", "ignoredplugins.txt");

        private static int parentProcessId = -1;

        private static readonly Dictionary<string, Assembly?> assembliesResolvingMap = new();

        private static void Main(string[] args)
        {
            ParseCommandLines(args);
            LoadAssembliesFromBundle();
            AssemblyResolvingRegister();
            ParentProcessListening();
            Patch();
            RunServer(args);
        }

        private static void ParseCommandLines(string[] args)
        {
            Action<string>? valueSetCallback = null;
            foreach (var a in args)
            {
                if (valueSetCallback != null)
                {
                    valueSetCallback(a);
                    valueSetCallback = null;
                    continue;
                }

                valueSetCallback = a switch
                {
                    "--bootstrap-server-executable" => x => serverExecutablePath = Path.GetFullPath(x),
                    "--bootstrap-server-plugins-directory" => x => serverPluginsDirectoryPath = Path.GetFullPath(x),
                    "--bootstrap-server-ignored-plugins-file" => x => serverIgnoredPluginsFilePath = Path.GetFullPath(x),
                    "--bootstrap-parent-process-id" => x => parentProcessId = int.Parse(x),
                    _ => null
                };
            }
        }

        private static void LoadAssembliesFromBundle()
        {
            var reader = new ExecutableReader(serverExecutablePath);
            if (!reader.IsSingleFile)
                throw new InvalidOperationException("Attempt to load a non-bundle executable");

            Console.WriteLine("Loading Assemblies...");
            foreach (var f in reader.Bundle.Files)
            {
                if (!f.RelativePath.EndsWith(".dll"))
                    continue;

                using var stream = f.AsStream();
                var image = new byte[stream.Length];
                _ = stream.Read(image, 0, image.Length);

                try
                {
                    var asm = Assembly.Load(image);
                    Console.WriteLine(asm.GetName().Name);
                    var asmName = asm.GetName().Name;
                    if (asmName != null)
                        assembliesResolvingMap[asmName] = asm;
                }
                catch (BadImageFormatException)
                {
                    if (!Directory.Exists(NATIVE_LIBRARY_EXTRACT_PATH))
                        Directory.CreateDirectory(NATIVE_LIBRARY_EXTRACT_PATH);

                    var path = Path.Combine(NATIVE_LIBRARY_EXTRACT_PATH, Path.GetFileName(f.RelativePath));
                    if (!File.Exists(path))
                        File.WriteAllBytes(path, image);
                }
            }
        }

        private static void AssemblyResolvingRegister()
        {
            AssemblyLoadContext.Default.Resolving += ResolvingFromBin;
            Assembly? ResolvingFromBin(AssemblyLoadContext ctx, AssemblyName? asmName)
            {
                if (asmName?.Name == null)
                    return null;

                if (assembliesResolvingMap.TryGetValue(asmName.Name, out var asm) && asm != null)
                    return asm;

                var loc = Path.Combine(Path.GetDirectoryName(serverExecutablePath)!, "bin", asmName.Name + ".dll");
                if (File.Exists(loc))
                    asm = ctx.LoadFromAssemblyPath(loc);

                loc = Path.ChangeExtension(loc, ".exe");
                if (File.Exists(loc))
                    asm = ctx.LoadFromAssemblyPath(loc);

                if (asm != null)
                    assembliesResolvingMap[asmName.Name] = asm;

                return asm;
            }

            AssemblyLoadContext.Default.ResolvingUnmanagedDll += ResolvingNative;
            IntPtr ResolvingNative(Assembly asm, string name)
            {
                var loc = Path.Combine(NATIVE_LIBRARY_EXTRACT_PATH, name);
                if (File.Exists(loc))
                    return NativeLibrary.Load(loc);

                return IntPtr.Zero;
            }
        }

        private static void ParentProcessListening()
        {
            if (parentProcessId == -1)
                return;

            var p = Process.GetProcessById(parentProcessId);
            p.Exited += delegate
            {
                p.Dispose();
                OTAPI.Hooks.Main.InvokeCommandProcess("exit", "exit");

                Task.Delay(15 * 1000).ContinueWith(_ => Environment.Exit(0));
            };
            p.EnableRaisingEvents = true;

            if (p.HasExited)
                Environment.Exit(0);
        }

        private static void Patch()
        {
            HookEndpointManager.Add(typeof(Console).GetProperty("ForegroundColor")?.SetMethod, SetFColor);
            HookEndpointManager.Add(typeof(Console).GetProperty("BackgroundColor")?.SetMethod, SetBColor);
            HookEndpointManager.Add(typeof(Console).GetProperty("Title")?.SetMethod, SetTitle);
            HookEndpointManager.Add(typeof(Console).GetMethod("ResetColor"), ResetColor);

            HookEndpointManager.Add(
                typeof(TerrariaApi.Server.ServerApi).GetProperty("ServerPluginsDirectoryPath")?.GetMethod,
                GetServerPluginsDirectoryPath);
            HookEndpointManager.Modify(
                typeof(TerrariaApi.Server.ServerApi).GetMethod("LoadPlugins", BindingFlags.Static | BindingFlags.NonPublic),
                LoadPluginsILPatcher);
            HookEndpointManager.Add(
                typeof(ReLogic.OS.Windows.WindowService).GetMethod("SetQuickEditEnabled"),
                SetQuickEditEnabled);
            HookEndpointManager.Add(
                typeof(Terraria.Program).GetMethod("InitializeConsoleOutput"),
                InitializeConsoleOutput);
        }

        private static void RunServer(string[] args)
        {
            ResetColor();
            TerrariaApi.Server.Program.Main(args);
        }


        #region Hooks

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetBColor(ConsoleColor value)
        {
            Console.WriteLine($"\u0001bgclr{value}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetFColor(ConsoleColor value)
        {
            switch (value)
            {
                case ConsoleColor.Gray:
                    Console.WriteLine("\u0001fgclrLightGray");
                    return;
                case ConsoleColor.DarkGray:
                    Console.WriteLine("\u0001fgclrGray");
                    return;
            }
            Console.WriteLine($"\u0001fgclr{value}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetTitle(string value)
        {
            Console.WriteLine($"\u0001title{value}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ResetColor()
        {
            SetFColor(ConsoleColor.Gray);
            SetBColor(ConsoleColor.Black);
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetServerPluginsDirectoryPath() => serverPluginsDirectoryPath;

        [MethodImpl(MethodImplOptions.NoInlining)]
        // ReSharper disable once InconsistentNaming
        public static void LoadPluginsILPatcher(ILContext il)
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(
                    //MoveType.After,
                    //i => i.MatchLdstr("ignoredplugins.txt"),
                    //i => i.MatchCall(typeof(Path), "Combine"),
                    i => i.MatchStloc(0)))
            {
                c.EmitDelegate(IgnoredPluginsFilePathPatch);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string IgnoredPluginsFilePathPatch(string path) => serverIgnoredPluginsFilePath;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void SetQuickEditEnabled(ReLogic.OS.Windows.WindowService service, bool enabled)
        {

        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializeConsoleOutput()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
        }

        #endregion
    }
}
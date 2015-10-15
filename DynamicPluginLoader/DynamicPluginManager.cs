using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CM3D2.DynamicLoader.Plugin
{
    public static class DynamicPluginManager
    {
        public static bool IsInitialized { get; private set; }
        public static string DataPath { get; private set; }
        public static string PluginsPath { get; private set; }
        public static string AppDomainNameSuffix { get; private set; }
        public static TextWriter Log { get; private set; }
        public static TextWriter DebugLog { get; private set; }
        public static AppDomains AppDomains { get; private set; }

        static DynamicLoaderPlugin DynamicLoaderPlugin;
        static Helper.DirectoryWatcher DirectoryWatcher;
        static List<string> LoadRequestAssemblyPaths = new List<string>();
        static List<AppDomain> UnloadRequestAppDomains = new List<AppDomain>();

        public static void Init(DynamicLoaderPlugin dynamicLoaderPlugin)
        {
            if (IsInitialized)
            {
                return;
            }
            IsInitialized = true;
            Log = Console.Out;
#if DEBUG
            DebugLog = Console.Out;
#else
            DebugLog = TextWriter.Null;
#endif
            DataPath = dynamicLoaderPlugin.DataPath;

            PluginsPath = dynamicLoaderPlugin.Preferences["Config"]["PluginsPath"].Value;
            if (string.IsNullOrEmpty(PluginsPath))
            {
                PluginsPath = @".\UnityInjector\DynamicPlugins";
            }
			PluginsPath = Path.GetFullPath(PluginsPath);
			DebugLog.WriteLine("DynamicPluginManager : PluginsPath = {0}", PluginsPath);

			AppDomainNameSuffix = ".appdomain";

            if (!System.IO.Directory.Exists(PluginsPath))
            {
                System.IO.Directory.CreateDirectory(PluginsPath);
            }

            DynamicLoaderPlugin = dynamicLoaderPlugin;
            AppDomains = new AppDomains(Log);
            DirectoryWatcher = new Helper.DirectoryWatcher(PluginsPath, "*.dll");
        }

        static void Load(string assemblyPath)
        {
            if (LoadRequestAssemblyPaths.Contains(assemblyPath))
            {
                return;
            }
            LoadRequestAssemblyPaths.Add(assemblyPath);
        }

        static void Unload(AppDomain appDomain)
        {
            if (appDomain == null || UnloadRequestAppDomains.Contains(appDomain))
            {
                return;
            }
            UnloadRequestAppDomains.Add(appDomain);
        }

        static void Unload(string assemblyPath)
        {
            Unload(AppDomains.GetAppDomainByAssemblyPath(assemblyPath));
        }

        static IEnumerator Reload(string dllFileName)
        {
            string dllPath = Path.GetFullPath(Path.Combine(PluginsPath, dllFileName));
            DebugLog.WriteLine("DynamicPluginManager.Unload({0})", dllPath);
            Unload(dllPath);
            yield return null;
            DebugLog.WriteLine("DynamicPluginManager.Load({0})", dllPath);
            Load(dllPath);
            yield break;
        }

        public static void OnLevelWasLoaded(int level)
        {
            AppDomains.ForEachPlugin("OnLevelWasLoaded", (plugin) =>
            {
                plugin.OnLevelWasLoaded(level);
            });
        }

        public static void Update()
        {
            DirectoryWatcher.Update((fileSystemEventArgs) =>
            {
                if (fileSystemEventArgs.ChangeType == WatcherChangeTypes.Created ||
					fileSystemEventArgs.ChangeType == WatcherChangeTypes.Changed
					)
                {
	                string fileName = Path.GetFileName(fileSystemEventArgs.Name);
	                DynamicLoaderPlugin.StartCoroutine(Reload(fileName));
                    return;
                }
            });

            foreach (string assemblyPath in LoadRequestAssemblyPaths)
            {
                AppDomains.Load(assemblyPath);
            }
            LoadRequestAssemblyPaths.Clear();

            AppDomains.ForEachPlugin("Update", (plugin) =>
            {
                plugin.Update();
            });

            foreach (AppDomain appDomain in UnloadRequestAppDomains)
            {
                AppDomains.Unload(appDomain);
            }
            UnloadRequestAppDomains.Clear();
        }

        public static void LateUpdate()
        {
            AppDomains.ForEachPlugin("LateUpdate", (plugin) =>
            {
                plugin.LateUpdate();
            });
        }

        public static void OnGUI()
        {
            AppDomains.ForEachPlugin("OnGUI", (plugin) =>
            {
                plugin.OnGUI();
            });
        }

        public static void OnDestroy()
        {
            AppDomains.ForEachPlugin("OnDestroy", (plugin) =>
            {
                plugin.OnDestroy();
            });
        }

        public static void ReportException(Exception ex)
        {
            ReportException(ex, "DynamicLoader");
        }

        public static void ReportException(Exception ex, string title)
        {
            Log.WriteLine("\n\n{0} : {1}", title, ex.Message);
            foreach (StackFrame f in (new StackTrace(ex, true).GetFrames()))
            {
                Log.WriteLine(
                    "{0}({1}.{2}) : {3}.{4}",
                    f.GetFileName(), f.GetFileLineNumber(), f.GetFileColumnNumber(),
                    f.GetMethod().DeclaringType, f.GetMethod()
                );
            }
        }
    }
}

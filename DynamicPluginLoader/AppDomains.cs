using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CM3D2.DynamicLoader.Plugin
{
    public class AppDomains
    {
        Dictionary<AppDomain, List<DynamicPluginBase>> dict =
            new Dictionary<AppDomain, List<DynamicPluginBase>>();
        HashSet<string> blackList = new HashSet<string>();
        TextWriter Log = null;

        public static string AppDomainNameSuffix {
            get {
                return DynamicPluginManager.AppDomainNameSuffix;
            }
        }

        public AppDomains(TextWriter textWriter) {
            Log = textWriter;
        }

        public bool IsLoaded(AppDomain appDomain)
        {
            if (appDomain == null)
            {
                return false;
            }
            return dict.ContainsKey(appDomain);
        }

        public bool IsLoaded(string assemblyPath)
        {
            return IsLoaded(GetAppDomainByAssemblyPath(assemblyPath));
        }

        public List<DynamicPluginBase> Load(string assemblyPath)
        {
            try
            {
                if (IsLoaded(assemblyPath) || !File.Exists(assemblyPath))
                {
                    return null;
                }

                string appDomainName = MakeAppDomainName(assemblyPath);
                AppDomain appDomain = AppDomain.CreateDomain(appDomainName);

                var plugins = new List<DynamicPluginBase>();
                string sha1String = null;
                try
                {
                    // http://stackoverflow.com/questions/425077/
                    byte[] dllImage = File.ReadAllBytes(assemblyPath);
                    sha1String = GetSha1HexString(dllImage) + "|" + dllImage.Length.ToString();
                    if (blackList.Contains(sha1String))
                    {
                        throw new Exception("Blacklisted");
                    }

                    Assembly assembly = Assembly.Load(dllImage);
                    foreach (Type t in assembly.GetTypes())
                    {
                        if (typeof(DynamicPluginBase).IsAssignableFrom(t) && !t.IsAbstract)
                        {
                            DynamicPluginBase plugin = (DynamicPluginBase)Activator.CreateInstance(t);
                            plugins.Add(plugin);
                        }
                    }

                    foreach (DynamicPluginBase plugin in plugins)
                    {
                        plugin.PrimaryAppDomain = AppDomain.CurrentDomain;
                        plugin.OnPluginLoad();
                    }
                }
                catch (Exception ex)
                {
                    DynamicPluginManager.ReportException(ex);
                    plugins.Clear();
                }

                if (plugins.Count() == 0)
                {
                    if (!string.IsNullOrEmpty(sha1String))
                    {
                        Log.WriteLine("{0} : {1} has been blacklisted", GetType().Name, assemblyPath);
                        blackList.Add(sha1String);
                    }
                    AppDomain.Unload(appDomain);
                    return null;
                }

                dict[appDomain] = plugins;
                return plugins;
            }
            catch (Exception ex)
            {
                DynamicPluginManager.ReportException(ex);
            }
            return null;
        }

        public bool Unload(AppDomain appDomain)
        {
            bool result = false;
            try
            {
                if (appDomain == null)
                {
                    throw new Exception("appDomain == null");
                }

                List<DynamicPluginBase> plugins;
                if (!dict.TryGetValue(appDomain, out plugins))
                {
                    throw new Exception(string.Format("There is no appDomain {0}", appDomain));
                }
                dict.Remove(appDomain);

                foreach (DynamicPluginBase plugin in plugins)
                {
                    plugin.OnPluginUnload();
                    GameObject.Destroy(plugin);
                }
                AppDomain.Unload(appDomain);
                result = true;
            }
            catch (Exception ex)
            {
                DynamicPluginManager.ReportException(ex);
            }
            return result;
        }

        public bool Unload(string assemblyPath)
        {
            AppDomain appDomain = GetAppDomainByAssemblyPath(assemblyPath);
            return Unload(appDomain);
        }

        public AppDomain GetAppDomainByAssemblyPath(string assemblyPath)
        {
            string assemblyAppDomainName = MakeAppDomainName(assemblyPath);
            foreach (AppDomain appDomain in dict.Keys)
            {
                string appDomainName = appDomain.FriendlyName;
                if (string.Equals(assemblyAppDomainName, appDomainName, StringComparison.OrdinalIgnoreCase))
                {
                    return appDomain;
                }
            }
            return null;
        }

        public void ForEachPlugin(string actionName, Action<DynamicPluginBase> action)
        {
            foreach (List<DynamicPluginBase> plugins in dict.Values)
            {
                foreach (DynamicPluginBase plugin in plugins)
                {
                    try
                    {
                        action(plugin);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine("\n\n{0} : {1}", actionName, ex.Message);
                        DynamicPluginManager.ReportException(ex);
                    }
                }
            }
        }

        public static string MakeAppDomainName(string assemblyPath)
        {
            string name = Path.GetFileNameWithoutExtension(assemblyPath).ToLower();
            return name + AppDomainNameSuffix;
        }

        public static string GetSha1HexString(byte[] bytes)
        {
            string hash;
            using (var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider())
            {
                hash = BitConverter.ToString(sha1.ComputeHash(bytes)).Replace("-", "");
            }
            return hash;
        }
    }
}

using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace CM3D2.DynamicLoader.Plugin
{
    [PluginName("CM3D2 DynamicLoader"), PluginVersion("0.1.1.0")]
    public class DynamicLoaderPlugin : PluginBase
    {
        void Awake()
        {
            GameObject.DontDestroyOnLoad(this);
            DynamicPluginManager.Init(this);
        }

        void OnLevelWasLoaded(int level)
        {
            DynamicPluginManager.OnLevelWasLoaded(level);
        }

        void Update()
        {
            DynamicPluginManager.Update();
        }

        void LateUpdate()
        {
            DynamicPluginManager.LateUpdate();
        }

        void OnGUI()
        {
            DynamicPluginManager.OnGUI();
        }

        void OnDestroy()
        {
            DynamicPluginManager.OnDestroy();
        }
    }
}

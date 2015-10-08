using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace CM3D2.DynamicLoader.Plugin
{
    [PluginFilter("CM3D2x64"), PluginFilter("CM3D2x86"),
    PluginFilter("CM3D2VRx64"), PluginFilter("CM3D2VRx86"),
    PluginFilter("CM3D2OHx64"), PluginFilter("CM3D2OHx86"),
    PluginFilter("CM3D2OHVRx64"), PluginFilter("CM3D2OHVRx86"),
    PluginName("CM3D2 DynamicLoader"),
    PluginVersion("0.1.0.0")]
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

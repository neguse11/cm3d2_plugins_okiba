using ExIni;
using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;

namespace CM3D2.DynamicLoader.Plugin
{
    public abstract class DynamicPluginBase : UnityInjector.PluginBase
    {
        /// <summary>
        /// UnityやUnityInjectorが動作しているAppDomain。
        /// DynamicPluginBaseは、Unity本体とは別のAppDomainで動作しているため、
        /// GetType(string)等が使えない場合は以下のPrimaryAppDomain経由で使用する
        /// </summary>
        public AppDomain PrimaryAppDomain;

        protected DynamicPluginBase()
        {
        }

        /// <summary>
        /// プラグインのDLL(アセンブリ)がロードされた時に呼ばれる。
        /// ただし、コンストラクタのほうが先に呼び出されるので注意。
        /// </summary>
        public virtual void OnPluginLoad() { }

        /// <summary>
        /// プラグインのDLL(アセンブリ)がアンロードされる直前に呼ばれる。
        /// ただし、アプリ終了時には呼び出されないこともあるので注意。
        /// </summary>
        public virtual void OnPluginUnload() { }

        /// <summary>
        /// MonoBehaviour.OnLevelWasLoadedから呼ばれる
        /// </summary>
        public virtual void OnLevelWasLoaded(int level) { }

        /// <summary>
        /// MonoBehaviour.Updateから呼ばれる
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// MonoBehaviour.LateUpdateから呼ばれる
        /// </summary>
        public virtual void LateUpdate() { }

        /// <summary>
        /// MonoBehaviour.OnGUIから呼ばれる
        /// </summary>
        public virtual void OnGUI() { }

        /// <summary>
        /// MonoBehaviour.OnDestroyから呼ばれる
        /// </summary>
        public virtual void OnDestroy() { }
    }
}

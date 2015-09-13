using System;
using System.Collections.Generic;
using System.IO;
using UnityInjector;
using UnityInjector.Attributes;

namespace CM3D2.VoiceNormalizer.Plugin
{
    [PluginFilter("CM3D2x64"),
    PluginFilter("CM3D2x86"),
    PluginFilter("CM3D2VRx64"),
    PluginName("CM3D2 VoiceNormalizer"),
    PluginVersion("0.1.0.0")]
    public class VoiceNormalizer : PluginBase
    {
        public static string PluginName { get { return "CM3D2.VoiceNormalizer"; } }

        public void Awake()
        {
            UnityEngine.GameObject.DontDestroyOnLoad(this);
            VoiceNormalizationTable.Filename = @"UnityInjector\Config\VoiceNormalizerTable.txt";
        }

        public void OnLevelWasLoaded(int level)
        {
            CM3D2.VoiceNormalizer.Managed.Callbacks.AudioSourceMgr.LoadFromWf.Callbacks[PluginName] = AudioSourceMgr_LoadFromWf_Callback;
        }

        void AudioSourceMgr_LoadFromWf_Callback(AudioSourceMgr audioSourceMgr, string f_strFileName)
        {
            // audioSourceMgr.aryAmpRateを無理やり書き換えて音量制御を行う
            if (audioSourceMgr.SoundType == AudioSourceMgr.Type.Voice)
            {
                float s = 2f;
#if DEBUG
                Console.WriteLine("AudioSourceMgr.LoadFromWf(f_strFileName={0})", f_strFileName);
#endif
                float[] aryAmpRate = Helper.GetInstanceField(typeof(AudioSourceMgr), audioSourceMgr, "aryAmpRate") as float[];
                int v;
                var dict = VoiceNormalizationTable.Dict;
                string fn = Path.GetFileNameWithoutExtension(f_strFileName);
                if (dict.TryGetValue(fn, out v))
                {
                    s *= 32768.0f / (float)v;
                }
#if DEBUG
                Console.WriteLine(" --> s = {0}", s);
#endif
                aryAmpRate[(int)AudioSourceMgr.Type.Voice] = s;
            }
        }
    }

    internal static class VoiceNormalizationTable
    {
        static Dictionary<string, int> dict = null;
        public static string Filename { get; set; }

        public static Dictionary<string, int> Dict
        {
            get
            {
                if (dict == null)
                {
                    try
                    {
                        var d = new Dictionary<string, int>();
                        if (Filename == null)
                        {
#if DEBUG
                            Console.WriteLine("VoiceNormalizationTable : Filename == null");
#endif
                            return d;
                        }
                        char[] delims = { ',', '\n' };
                        string fstr = File.ReadAllText(Filename);
                        string[] fstrs = fstr.Split(delims);
                        for (int i = 0; i < (fstrs.Length / 2) * 2; i += 2)
                        {
                            string k = fstrs[i + 0];
                            string v = fstrs[i + 1];
                            d[k] = Int32.Parse(v);
                        }
                        dict = d;
                    }
                    catch (Exception ex)
                    {
                        Helper.ShowException(ex);
                    }
                }
                return dict;
            }
        }
    }
}

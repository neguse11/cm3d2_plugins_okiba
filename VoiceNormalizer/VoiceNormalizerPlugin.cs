using System;
using System.Collections.Generic;
using System.IO;
using UnityInjector;
using UnityInjector.Attributes;

namespace CM3D2.VoiceNormalizer.Plugin
{
    [PluginName("CM3D2 VoiceNormalizer"), PluginVersion("0.1.3.0")]
    public class VoiceNormalizer : UnityInjector.PluginBase
    {
        public static string PluginName { get { return "CM3D2.VoiceNormalizer"; } }

        public void Awake()
        {
            UnityEngine.GameObject.DontDestroyOnLoad(this);
            VoiceNormalizationTable.Filename = Path.Combine(DataPath, "VoiceNormalizerTable.txt");
        }

        public void OnLevelWasLoaded(int level)
        {
            CM3D2.VoiceNormalizer.Managed.Callbacks.AudioSourceMgr.LoadFromWf.Callbacks[PluginName] = AudioSourceMgr_LoadFromWf_Callback;
        }

        void AudioSourceMgr_LoadFromWf_Callback(AudioSourceMgr audioSourceMgr, string f_strFileName, bool stream)
        {
            // audioSourceMgr.aryAmpRateを無理やり書き換えて音量制御を行う
            if (audioSourceMgr.SoundType == AudioSourceMgr.Type.Voice)
            {
                float s = 2f;
#if DEBUG
                Console.WriteLine("AudioSourceMgr.LoadFromWf(f_strFileName={0}, stream={1})", f_strFileName, stream);
#endif
                float[] aryAmpRate = Helper.GetInstanceField(typeof(AudioSourceMgr), audioSourceMgr, "aryAmpRate") as float[];
				VoiceNormalizationTable.Value val;
                var dict = VoiceNormalizationTable.Dict;
                string fn = Path.GetFileNameWithoutExtension(f_strFileName);
                if (dict.TryGetValue(fn, out val))
                {
                    s *= 32768f / (float)val.peak;
                    // s *= 0.5f / val.rms;
                }
                aryAmpRate[(int)AudioSourceMgr.Type.Voice] = s;
            }
        }
    }

    internal static class VoiceNormalizationTable
    {
		public struct Value {
			public int		peak;
			public float	rms;
		}

		static Dictionary<string, Value> dict = null;
        public static string Filename { get; set; }

        public static Dictionary<string, Value> Dict
        {
            get
            {
                if (dict == null)
                {
                    try
                    {
                        var d = new Dictionary<string, Value>();
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
                        for (int i = 0; i < (fstrs.Length / 3) * 3; i += 3)
                        {
                            string k = fstrs[i + 0];
                            string peak = fstrs[i + 1];
                            string rms = fstrs[i + 2];
							d[k] = new Value { peak = Int32.Parse(peak), rms = float.Parse(rms) };
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

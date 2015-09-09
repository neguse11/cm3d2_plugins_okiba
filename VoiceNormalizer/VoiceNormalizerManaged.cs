using System;
using System.Collections.Generic;

namespace CM3D2.VoiceNormalizer.Managed
{
    namespace Callbacks
    {
        namespace AudioSourceMgr
        {
            public static class LoadFromWf
            {
                public delegate void Callback(global::AudioSourceMgr that, string f_strFileName);
                public static Callbacks<Callback> Callbacks = new Callbacks<Callback>();

                public static void Invoke(global::AudioSourceMgr that, string f_strFileName)
                {
                    try
                    {
                        foreach (Callback callback in Callbacks.Values)
                        {
                            callback(that, f_strFileName);
                        }
                    }
                    catch (Exception e)
                    {
                        Helper.ShowException(e);
                    }
                }
            }
        }

        public class Callbacks<T> : SortedDictionary<string, T> { }
    }
}

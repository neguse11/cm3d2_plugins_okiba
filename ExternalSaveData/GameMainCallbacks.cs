using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace CM3D2.ExternalSaveData.Managed.GameMainCallbacks
{
    public static class Deserialize
    {
        public delegate void Callback(GameMain that, int f_nSaveNo);
        public static Callbacks<Callback> Callbacks = new Callbacks<Callback>();

        public static void Invoke(GameMain that, int f_nSaveNo)
        {
            try
            {
                foreach (Callback callback in Callbacks.Values)
                {
                    callback(that, f_nSaveNo);
                }
            }
            catch (Exception e)
            {
                Helper.ShowException(e);
            }
        }
    }

    public static class Serialize
    {
        public delegate void Callback(GameMain that, int f_nSaveNo, string f_strComment);
        public static Callbacks<Callback> Callbacks = new Callbacks<Callback>();

        public static void Invoke(GameMain that, int f_nSaveNo, string f_strComment)
        {
            try
            {
                foreach (Callback callback in Callbacks.Values)
                {
                    callback(that, f_nSaveNo, f_strComment);
                }
            }
            catch (Exception e)
            {
                Helper.ShowException(e);
            }
        }
    }

    public static class DeleteSerializeData
    {
        public delegate void Callback(GameMain that, int f_nSaveNo);
        public static Callbacks<Callback> Callbacks = new Callbacks<Callback>();

        public static void Invoke(GameMain that, int f_nSaveNo)
        {
            try
            {
                foreach (Callback cb in Callbacks.Values)
                {
                    cb(that, f_nSaveNo);
                }
            }
            catch (Exception e)
            {
                Helper.ShowException(e);
            }
        }
    }

    public class Callbacks<T> : SortedDictionary<string, T> { }
}

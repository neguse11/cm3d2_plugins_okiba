using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

#if DYNAMIC_PLUGIN
using CM3D2.DynamicLoader.Plugin;
using System.Reflection;

// 以下の AssemblyVersion は削除しないこと
[assembly: AssemblyVersion("1.0.*")]
#endif

namespace CM3D2.EditSceneUndo.Plugin
{
    [PluginFilter("CM3D2x64"), PluginFilter("CM3D2x86"),
    PluginFilter("CM3D2VRx64"), PluginFilter("CM3D2VRx86"),
    PluginFilter("CM3D2OHx64"), PluginFilter("CM3D2OHx86"),
    PluginFilter("CM3D2OHVRx64"), PluginFilter("CM3D2OHVRx86"),
    PluginName("CM3D2 EdieSceneUndo"),
    PluginVersion("0.1.0.0")]
#if DYNAMIC_PLUGIN
    class EdieSceneUndoPlugin : DynamicPluginBase
#else
    class EdieSceneUndoPlugin : PluginBase
#endif
    {
        const int HistoryMax = 10000;
#if DEBUG
        public static TextWriter Log = Console.Out;
#else
        public static TextWriter Log = TextWriter.Null;
#endif
        List<string> outputs = new List<string>();
        History<PropSnapShot> history = new History<PropSnapShot>();

        PropSnapShot tempShot = new PropSnapShot();
        PropSnapShot prev = new PropSnapShot();
        int changeCount = 0;

        public EdieSceneUndoPlugin()
        {
            Log.WriteLine("{0} : {0}(id={1})", this.GetType().Name, GetInstanceID());
        }

#if DYNAMIC_PLUGIN
        public override void OnPluginLoad() {
            Log.WriteLine("{0} : PluginLoad(id={1})", this.GetType().Name, GetInstanceID());
        }

        public override void OnPluginUnload() {
            Log.WriteLine("{0} : PluginUnload(id={1})", this.GetType().Name, GetInstanceID());
        }
#endif

#if DYNAMIC_PLUGIN
        override
#endif
        public void OnLevelWasLoaded(int level)
        {
        }

#if DYNAMIC_PLUGIN
        override
#endif
        public void Update()
        {
            outputs.Clear();

            outputs.Add(string.Format("Update"));

            SceneEdit sceneEdit = FindSceneEdit();
            if (sceneEdit == null)
            {
                outputs.Add(string.Format("sceneEdit == null"));
                return;
            }

            PropSnapShot cur = CapturePropSnapShot();
            if (!cur.Equals(prev))
            {
                // マウス左ボタンがドラッグ中なら何もしない
                if (!Input.GetMouseButton(0))
                {
                    changeCount++;
                    Log.WriteLine("--- : Record");
                    RecordHistory();
                    prev = PropSnapShot.Clone(cur);
                }
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                Log.WriteLine("^Z : Undo");
                UndoHistory();
            }
            if (Input.GetKeyDown(KeyCode.Y))
            {
                Log.WriteLine("^Y : Redo");
                RedoHistory();
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                tempShot = PropSnapShot.Clone(cur);
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                StorePropSnapShot(tempShot);
            }

            outputs.Add(string.Format("historyCursor = {0}", history.historyCursor));
            outputs.Add(string.Format("changeCount = {0}", changeCount));
        }

        void RecordHistory()
        {
            PropSnapShot cur = CapturePropSnapShot();
            if (prev != null && !cur.Equals(prev))
            {
                history.Record(PropSnapShot.Clone(cur));
            }

            history.Trim(HistoryMax);

            prev = PropSnapShot.Clone(cur);
        }

        void UndoHistory()
        {
            PropSnapShot his;
            if (history.Undo(out his))
            {
                prev = PropSnapShot.Clone(his);
                StorePropSnapShot(his);
            }
        }

        void RedoHistory()
        {
            PropSnapShot his;
            if (history.Redo(out his))
            {
                prev = PropSnapShot.Clone(his);
                StorePropSnapShot(his);
            }
        }

        static SceneEdit FindSceneEdit()
        {
            GameObject go = GameObject.Find("__SceneEdit__");
            if (go == null)
            {
                return null;
            }
            return go.GetComponent<SceneEdit>();
        }

#if DYNAMIC_PLUGIN
        override
#endif
        public void OnGUI()
        {
#if DEBUG
            int ox = 0;
            int oy = 0;
            int x = ox;
            int y = oy;
            int w = 256;
            int h = 22;
            int dy = h;

            GUI.Label(new Rect(x, y, w, h), string.Format("{0}, outputs.Count()={1}", GetType().Name, outputs.Count()));
            y += dy;

            for(int i = 0; i < outputs.Count(); i++) {
                GUI.Label(new Rect(x, y, w, h), outputs[i]);
                y += dy;
                if(y+dy >= Screen.height) {
                    y = 0;
                    x += w;
                }
            }
#endif
        }

        PropSnapShot CapturePropSnapShot()
        {
            PropSnapShot s = new PropSnapShot();
            int maidNumber = 0;
            Maid maid = GameMain.Instance.CharacterMgr.GetMaid(maidNumber);
            if (maid == null)
            {
                return s;
            }

            foreach (MPN mpn in Enum.GetValues(typeof(MPN)))
            {
                try
                {
                    MaidProp maidProp = maid.GetProp(mpn);
                    if (maidProp != null)
                    {
                        s[mpn] = maidProp;
                    }
                }
                catch (Exception)
                {
                }
            }

            MaidParts maidParts = maid.Parts;
            MaidParts.PartsColor[] newMaidParts = null;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(ms);
                maidParts.Serialize(bw);
                bw.Flush();
                ms.Position = 0;
                using (BinaryReader br = new BinaryReader(ms))
                {
                    newMaidParts = MaidParts.DeserializePre(br);
                }
            }
            for (int i = 0; i < (int)MaidParts.PARTS_COLOR.MAX; i++)
            {
                MaidParts.PARTS_COLOR col = (MaidParts.PARTS_COLOR)i;
                s.SetColor(col, newMaidParts[i]);
            }
            return s;
        }

        void StorePropSnapShot(PropSnapShot s)
        {
            int maidNumber = 0;
            Maid maid = GameMain.Instance.CharacterMgr.GetMaid(maidNumber);
            if (maid == null)
            {
                return;
            }

            SceneEdit sceneEdit = FindSceneEdit();
            if (sceneEdit == null)
            {
                return;
            }

            foreach (MPN mpn in Enum.GetValues(typeof(MPN)))
            {
                if (mpn == MPN.null_mpn)
                {
                    continue;
                }
                try
                {
                    MaidProp maidProp = s[mpn];
                    if (!string.IsNullOrEmpty(maidProp.strFileName))
                    {
                        maid.SetProp(mpn, maidProp.strFileName, maidProp.nFileNameRID, false);
                    }
                    else
                    {
                        maid.SetProp(mpn, maidProp.value, false);
                    }
                }
                catch (Exception)
                {
                }
            }

            foreach (KeyValuePair<MaidParts.PARTS_COLOR, MaidParts.PartsColor> kv in s.maidPartsColors)
            {
                maid.Parts.SetPartsColor(kv.Key, kv.Value);
            }

            // 値の更新
            maid.AllProcProp();
            sceneEdit.UpdateSliders();
        }

        class PropSnapShot
        {
            public Dictionary<MPN, MaidProp> maidProps = new Dictionary<MPN, MaidProp>();
            public Dictionary<MaidParts.PARTS_COLOR, MaidParts.PartsColor> maidPartsColors = new Dictionary<MaidParts.PARTS_COLOR, MaidParts.PartsColor>();

            public MaidProp this[MPN mpn]
            {
                get
                {
                    return maidProps[mpn];
                }
                set
                {
                    maidProps[mpn] = value;
                }
            }

            public MaidParts.PartsColor GetColor(MaidParts.PARTS_COLOR partsColor)
            {
                return maidPartsColors[partsColor];
            }

            public void SetColor(MaidParts.PARTS_COLOR partsColor, MaidParts.PartsColor val)
            {
                maidPartsColors[partsColor] = val;
            }

            public PropSnapShot()
            {
            }

            public static PropSnapShot Clone(PropSnapShot s)
            {
                PropSnapShot newObj = new PropSnapShot();
                foreach (MPN mpn in Enum.GetValues(typeof(MPN)))
                {
                    MaidProp srcMaidProp = s.maidProps[mpn];
                    MaidProp newMaidProp = new MaidProp();

                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryWriter bw = new BinaryWriter(ms);
                        srcMaidProp.Serialize(bw);
                        bw.Flush();
                        ms.Position = 0;
                        using (BinaryReader br = new BinaryReader(ms))
                        {
                            newMaidProp.Deserialize(br);
                        }
                    }
                    newObj.maidProps[mpn] = newMaidProp;
                }

                for (int i = 0; i < (int)MaidParts.PARTS_COLOR.MAX; i++)
                {
                    MaidParts.PARTS_COLOR col = (MaidParts.PARTS_COLOR)i;
                    var c = s.GetColor(col);
                    newObj.SetColor(col, c);
                }
                return newObj;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }

                var s = obj as PropSnapShot;
                if (s == null)
                {
                    return false;
                }

                if (maidProps.Count != s.maidProps.Count)
                {
                    return false;
                }

                foreach (KeyValuePair<MPN, MaidProp> kv in maidProps)
                {
                    MPN mpn = kv.Key;
                    MaidProp maidProp = kv.Value;
                    MaidProp p;
                    if (!s.maidProps.TryGetValue(mpn, out p))
                    {
                        return false;
                    }
                    if (maidProp.strFileName != p.strFileName ||
                        maidProp.strTempFileName != p.strTempFileName ||
                        maidProp.idx != p.idx ||
                        maidProp.name != p.name ||
                        maidProp.type != p.type ||
                        maidProp.value_Default != p.value_Default ||
                        maidProp.value != p.value ||
                        maidProp.temp_value != p.temp_value ||
                        maidProp.value_LinkMAX != p.value_LinkMAX ||
                        maidProp.nFileNameRID != p.nFileNameRID ||
                        maidProp.nTempFileNameRID != p.nTempFileNameRID ||
                        maidProp.max != p.max ||
                        maidProp.min != p.min
                    //  maidProp.boDut              != p.boDut              || // 内部処理用なので無視すること
                    //  maidProp.boTempDut          != p.boTempDut          ||
                    )
                    {
                        return false;
                    }
                }

                for (int i = 0; i < (int)MaidParts.PARTS_COLOR.MAX; i++)
                {
                    MaidParts.PARTS_COLOR col = (MaidParts.PARTS_COLOR)i;
                    MaidParts.PartsColor lhs = maidPartsColors[col];
                    MaidParts.PartsColor rhs = s.maidPartsColors[col];
                    if (
                        lhs.m_bUse != rhs.m_bUse ||
                        lhs.m_nMainHue != rhs.m_nMainHue ||
                        lhs.m_nMainChroma != rhs.m_nMainChroma ||
                        lhs.m_nMainBrightness != rhs.m_nMainBrightness ||
                        lhs.m_nMainContrast != rhs.m_nMainContrast ||
                        lhs.m_nShadowRate != rhs.m_nShadowRate ||
                        lhs.m_nShadowHue != rhs.m_nShadowHue ||
                        lhs.m_nShadowChroma != rhs.m_nShadowChroma ||
                        lhs.m_nShadowBrightness != rhs.m_nShadowBrightness ||
                        lhs.m_nShadowContrast != rhs.m_nShadowContrast
                    )
                    {
                        return false;
                    }
                }

                return true;
            }

            public override int GetHashCode()
            {
                int x = 0;
                foreach (MPN mpn in Enum.GetValues(typeof(MPN)))
                {
                    x ^= maidProps[mpn].GetHashCode();
                }
                return x;
            }
        }
    }


    public class History<T> : List<T> where T : class
    {
        public int historyCursor = -1;

        public bool HistoryMode
        {
            get
            {
                return historyCursor >= 0;
            }
        }

        public History()
        {
        }

        public void Trim(int maxCount)
        {
            if (Count + 1 > maxCount)
            {
                RemoveRange(1, Count - maxCount);
            }
        }

        public void Record(T entry)
        {
            if (historyCursor >= 0)
            {
                // ヒストリカーソル以後の履歴を削除する
                RemoveRange(historyCursor, Count - historyCursor);
            }
            historyCursor = -1;

            Add(entry);
        }

        public bool Undo(out T result)
        {
            EdieSceneUndoPlugin.Log.WriteLine("Undo : historyCursor = {0}, Count = {1}", historyCursor, Count);
            if (historyCursor < 0)
            {
                if (Count > 0)
                {
                    historyCursor = Count - 1;
                }
            }

            --historyCursor;

            if (historyCursor < 0)
            {
                historyCursor = 0;
                result = null;
                return false;
            }

            result = this[historyCursor];
            EdieSceneUndoPlugin.Log.WriteLine("  --> : historyCursor = {0}", historyCursor);
            return true;
        }

        public bool Redo(out T result)
        {
            EdieSceneUndoPlugin.Log.WriteLine("Redo : historyCursor = {0}, Count = {1}", historyCursor, Count);
            if (historyCursor < 0)
            {
                result = null;
                return false;
            }

            ++historyCursor;
            if (historyCursor >= Count)
            {
                historyCursor = Count - 1;
                result = null;
                return false;
            }

            result = this[historyCursor];
            EdieSceneUndoPlugin.Log.WriteLine("  --> : historyCursor = {0}", historyCursor);
            return true;
        }
    }
}

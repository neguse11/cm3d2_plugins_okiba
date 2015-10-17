// USE_NOTIFIER : 定義すると右上に通知を出す（テスト中）
// #define USE_NOTIFIER


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;
using UnityInjector.Attributes;

#if USE_NOTIFIER
using CM3D2.Plugin.Notifier.V1;
#endif

#if DYNAMIC_PLUGIN
using CM3D2.DynamicLoader.Plugin;

// 以下の AssemblyVersion は削除しないこと
[assembly: AssemblyVersion("1.0.*")]
#endif

namespace CM3D2.EditSceneUndo.Plugin
{
    [PluginName("CM3D2 EditSceneUndo"), PluginVersion("0.2.0.0")]
#if DYNAMIC_PLUGIN
    class EditSceneUndoPlugin : DynamicPluginBase
#else
    class EditSceneUndoPlugin : UnityInjector.PluginBase
#endif
    {
        const int HistoryMax = 100;
#if DEBUG
        public static StringWriter Log = new StringWriter();
        public static StringWriter Win = new StringWriter();
#else
        public static TextWriter Log = TextWriter.Null;
        public static TextWriter Win = TextWriter.Null;
#endif
        History<PropSnapShot> history = new History<PropSnapShot>();

        PropSnapShot firstShot = null;
        PropSnapShot[] tempShot = new PropSnapShot[4];
        PropSnapShot prev = new PropSnapShot();
        PropSnapShot cur = new PropSnapShot();
        int changeCount = 0;
        GameObject gearMenuButton = null;
        GuiWindow guiWindow = new GuiWindow();
        Vector2 scrollPosition = new Vector2(0f, 0f);
        Settings settings = new Settings();
        string iniSection = "Config";
        bool isEditScene = false;
        bool lastVisibility = false;
        bool lastEditScene = false;

#if DYNAMIC_PLUGIN
        public override void OnPluginLoad()
        {
            Awake();
        }

        public override void OnPluginUnload()
        {
            OnDestroy();
        }
#endif

        void Notify(string message)
        {
#if USE_NOTIFIER
            Notifier.Notify(message);
#endif
        }

        void Notify(string format, params object[] args)
        {
            Notify(string.Format(format, args));
        }

        void Awake()
        {
            base.ReloadConfig();
            settings.Load((key) =>
            {
                return base.Preferences[iniSection][key].Value;
            });
            guiWindow.WindowRect.x = settings.WindowX;
            guiWindow.WindowRect.y = settings.WindowY;
            guiWindow.WindowRect.width = settings.WindowW;
            guiWindow.WindowRect.height = settings.WindowH;
            lastVisibility = settings.Enable;

            gearMenuButton = GearMenu.Buttons.Add(Name, this, Icon.Png, (go) => { Toggle(); });
            var pluginNameAttr = Attribute.GetCustomAttribute(GetType(), typeof(PluginNameAttribute)) as PluginNameAttribute;
            var pluginVersionAttr = Attribute.GetCustomAttribute(GetType(), typeof(PluginVersionAttribute)) as PluginVersionAttribute;
            guiWindow.Title = string.Format(
                "{0} {1}",
                (pluginNameAttr == null) ? Name : pluginNameAttr.Name,
                (pluginVersionAttr == null) ? string.Empty : pluginVersionAttr.Version);
        }

#if DYNAMIC_PLUGIN
        public override
#endif
        void OnDestroy()
        {
            settings.Enable = lastVisibility;
            settings.WindowX = guiWindow.WindowRect.x;
            settings.WindowY = guiWindow.WindowRect.y;
            settings.WindowW = guiWindow.WindowRect.width;
            settings.WindowH = guiWindow.WindowRect.height;
            settings.Save((key, value) =>
            {
                base.Preferences[iniSection][key].Value = value;
            });
            base.SaveConfig();
        }

#if DYNAMIC_PLUGIN
        public override
#endif
        void OnLevelWasLoaded(int level)
        {
            ClearHistory();
        }

#if DYNAMIC_PLUGIN
        public override
#endif
        void Update()
        {
#if DEBUG
            Win.GetStringBuilder().Length = 0;
#endif
            Win.WriteLine(string.Format("{0}.Update", GetType().Name));

            SceneEdit sceneEdit = FindSceneEdit();
            if (sceneEdit == null)
            {
                lastEditScene = false;
                Enable(false);
                Win.WriteLine(string.Format("sceneEdit == null"));
                return;
            }
            if(! lastEditScene) {
                guiWindow.Visible = lastVisibility;
            } else {
                lastVisibility = guiWindow.Visible;
            }

            isEditScene = true;
            lastEditScene = true;
            cur = CapturePropSnapShot();

            // 空の場合、最初のスナップショットとして保存
			if(firstShot == null) {
                firstShot = PropSnapShot.Clone(cur);
            }

            if (cur != null && prev != null && !cur.Equals(prev))
            {
                // マウス左ボタンがドラッグ中なら何もしない
                if (!Input.GetMouseButton(0))
                {
                    changeCount++;
                    Log.WriteLine("Record (prev={0:X8}, cur={1:X8})", prev.GetHashCode(), cur.GetHashCode());
                    RecordHistory();
                }
            }

            if (settings.Reset.GetKeyDown())
            {
                Log.WriteLine("Reset");
                Notify("Reset");
                RecallFirstShot();
            }
            if (settings.Undo.GetKeyDown())
            {
                Log.WriteLine("Undo");
                Notify("Undo");
                UndoHistory();
            }
            if (settings.Redo.GetKeyDown())
            {
                Log.WriteLine("Redo");
                Notify("Redo");
                RedoHistory();
            }

            Win.WriteLine(string.Format("historyCursor = {0}", history.historyCursor));
            Win.WriteLine(string.Format("changeCount = {0}", changeCount));
        }

        void Toggle()
        {
            if(! isEditScene)
            {
                Enable(false);
            } else {
                Enable(!guiWindow.Visible);
            }
        }

        void Enable(bool b) {
            bool upd = (b != guiWindow.Visible);
            guiWindow.Visible = b;
            if(upd) {
                if (guiWindow.Visible)
                {
                    GearMenu.Buttons.SetFrameColor(gearMenuButton, Color.black);
                }
                else
                {
                    GearMenu.Buttons.ResetFrameColor(gearMenuButton);
                }
            }
        }

        void ClearHistory()
        {
            history.Clear();
            firstShot = null;
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

        // 最初のスナップショットで上書きする
        void RecallFirstShot()
        {
            if (firstShot != null)
            {
                // 現在の状態をヒストリに登録
                RecordHistory();
                // 最初のスナップショットで上書き
                StorePropSnapShot(firstShot);
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
        public override
#endif
        void OnGUI()
        {
#if DEBUG
			GUI.Window(0, new Rect(0, 0, Screen.width/4, Screen.height), (windowId) => {
				GUI.backgroundColor = Color.black;
	            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), Win.ToString() + Log.ToString());
			}, "Debug");
#endif
            if(!isEditScene)
            {
                return;
            }
            guiWindow.DrawGui(123456789, RenderGuiWindow);
        }

        void RenderGuiWindow(int windowID, GuiWindow guiWindow)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(10);
                if (GUILayout.Button("<< 戻る　"))
                {
                    Notify("Undo");
                    UndoHistory();
                }
                GUILayout.Space(30);
                if (GUILayout.Button("　進む >>"))
                {
                    Notify("Redo");
                    RedoHistory();
                }
                GUILayout.Space(10);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.Box("暫定保存");
            GUILayout.BeginHorizontal();
            {
                string[] names = new string[] { "A", "B", "C", "D" };
                for (int i = 0; i < names.Length; i++)
                {
                    if (GUILayout.Button(names[i]))
                    {
                        Notify("QuickSave({0})", i);
                        tempShot[i] = PropSnapShot.Clone(cur);
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            GUILayout.Box("暫定読出");
            GUILayout.BeginHorizontal();
            {
                string[] names = new string[] { "A", "B", "C", "D" };
                for (int i = 0; i < names.Length; i++)
                {
                    if (GUILayout.Button(names[i]))
                    {
                        Notify("QuickLoad({0})", i);
                        StorePropSnapShot(tempShot[i]);
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(40);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(30);
                if (GUILayout.Button("最初に戻す(Undo可)"))
                {
                    Notify("Reset");
                    RecallFirstShot();
                }
                GUILayout.Space(30);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
            GUILayout.EndScrollView();
        }

        // 状態をキャプチャする
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

        // 状態を更新する (ヒストリには入れない)
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
//					x ^= maidProps[mpn].GetHashCode();
					MaidProp p;
					if(maidProps.TryGetValue(mpn, out p)) {
						x ^= p.GetHashCode();
					}
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

            EditSceneUndoPlugin.Log.WriteLine("  --> : historyCursor = {0}", historyCursor);
            Add(entry);
        }

        public bool Undo(out T result)
        {
            EditSceneUndoPlugin.Log.WriteLine("Undo : historyCursor = {0}, Count = {1}", historyCursor, Count);
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
            EditSceneUndoPlugin.Log.WriteLine("  --> : historyCursor = {0}", historyCursor);
            return true;
        }

        public bool Redo(out T result)
        {
            EditSceneUndoPlugin.Log.WriteLine("Redo : historyCursor = {0}, Count = {1}", historyCursor, Count);
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
            EditSceneUndoPlugin.Log.WriteLine("  --> : historyCursor = {0}", historyCursor);
            return true;
        }
    }
}


// 設定ファイル
namespace CM3D2.EditSceneUndo.Plugin
{
	public class Settings
	{
	    public bool Enable = false;                             // 表示状態
	    public KeySetting Undo = new KeySetting("Control+Z");   // 戻る
	    public KeySetting Redo = new KeySetting("Control+Y");   // 進む
	    public KeySetting Reset = new KeySetting();             // 最初に戻す
	    public KeySetting QuickStore1 = new KeySetting();
	    public KeySetting QuickStore2 = new KeySetting();
	    public KeySetting QuickStore3 = new KeySetting();
	    public KeySetting QuickStore4 = new KeySetting();
	    public KeySetting QuickLoad1 = new KeySetting();
	    public KeySetting QuickLoad2 = new KeySetting();
	    public KeySetting QuickLoad3 = new KeySetting();
	    public KeySetting QuickLoad4 = new KeySetting();
	    public float WindowX = 300f;
	    public float WindowY = 80f;
	    public float WindowW = 300f;
	    public float WindowH = 500f;

		// 設定の読み込み
		// 注意：インターフェースは必ずExIniと分離すること
	    public void Load(Func<string, string> getString)
	    {
	        GetBool(getString("Enable"), ref Enable);
	        GetKeySetting(getString("Undo"), ref Undo);
	        GetKeySetting(getString("Redo"), ref Redo);
	        GetKeySetting(getString("Reset"), ref Reset);
	        GetKeySetting(getString("QuickStore1"), ref QuickStore1);
	        GetKeySetting(getString("QuickStore2"), ref QuickStore2);
	        GetKeySetting(getString("QuickStore3"), ref QuickStore3);
	        GetKeySetting(getString("QuickStore4"), ref QuickStore4);
	        GetKeySetting(getString("QuickLoad1"), ref QuickLoad1);
	        GetKeySetting(getString("QuickLoad2"), ref QuickLoad2);
	        GetKeySetting(getString("QuickLoad3"), ref QuickLoad3);
	        GetKeySetting(getString("QuickLoad4"), ref QuickLoad4);
	        GetFloat(getString("WindowX"), ref WindowX);
	        GetFloat(getString("WindowY"), ref WindowY);
	        GetFloat(getString("WindowW"), ref WindowW);
	        GetFloat(getString("WindowH"), ref WindowH);
	    }

		// 設定の書き込み
		// 注意：インターフェースは必ずExIniと分離すること
	    public void Save(Action<string, string> setString)
	    {
	        setString("Enable", Enable.ToString());
	        setString("Undo", Undo.ToString());
	        setString("Redo", Redo.ToString());
	        setString("Reset", Reset.ToString());
	        setString("QuickStore1", QuickStore1.ToString());
	        setString("QuickStore2", QuickStore2.ToString());
	        setString("QuickStore3", QuickStore3.ToString());
	        setString("QuickStore4", QuickStore4.ToString());
	        setString("QuickLoad1", QuickLoad1.ToString());
	        setString("QuickLoad2", QuickLoad2.ToString());
	        setString("QuickLoad3", QuickLoad3.ToString());
	        setString("QuickLoad4", QuickLoad4.ToString());
	        setString("WindowX", WindowX.ToString());
	        setString("WindowY", WindowY.ToString());
	        setString("WindowW", WindowW.ToString());
	        setString("WindowH", WindowH.ToString());
	    }

	    static void GetBool(string boolString, ref bool output)
	    {
	        bool v;
	        if (bool.TryParse(boolString, out v))
	        {
	            output = v;
	        }
	    }

	    static void GetFloat(string floatString, ref float output)
	    {
	        float v;
	        if (float.TryParse(floatString, out v))
	        {
	            output = v;
	        }
	    }

	    static void GetKeySetting(string keyString, ref KeySetting output)
	    {
	        var ks = new KeySetting(keyString);
	        if (ks.IsGood)
	        {
	            output = ks;
	        }
	    }
	}
}


// 文字列とキーコードセットの変換
//
//      ・キーコード名は Unity の enum KeyCode の名前に従う
//      ・"LeftControl+0" のように、'+' 記号を使ってモディファイアをつけられる
//      ・"LeftShift+X|LeftAlt+2" のように、'|' 記号を使って複数のキーを OR 条件でまとめられる
//      ・数字については「0～9」が「Alpha0～Alpha9」に変換される
//      ・"Shift+X", "Control+X", "Alt+X" は "LeftShift+X|RightShift+X|LeftShift+RightShift+X" のように左右のキーの OR に展開される
//      ・"Ctrl"は"Control"に読み替えられる
//      ・モディファイアがついている場合、他のモディファイアが押されている場合は反応しない。例えば"Ctrl+Z"は"Ctrl+Shift+Z"には反応しない
//
public class KeySetting
{
	// キーの組み合わせ
	// "LeftControl+Z" のような、キーコード文字列を '+' で連結した AND 条件を保持、判定する。
	// 
    class KeySet
    {
		// キーの組み合わせ条件
		// true なら押下、false なら押下されていないことを条件とする
        Dictionary<KeyCode, bool> Keys;

		// モディファイアではないキー
		// ここに有効なキーコードが入っていない場合、押下条件が満たされることが無い
        KeyCode MainKey = KeyCode.None;

        public bool IsGood
        {
            get
            {
                return MainKey != KeyCode.None;
            }
        }

        public KeySet()
        {
            Clear();
        }

        public KeySet(string keyString) : this()
        {
            FromString(keyString);
        }

        public void Clear()
        {
            Keys = new Dictionary<KeyCode, bool>();
            MainKey = KeyCode.None;
        }

        public bool GetKey()
        {
            if (!IsGood)
            {
                return false;
            }
            // any : 「全てのキーが押されていない」という条件が Keys に入っていた場合に
            // false を返すようにするための保険
            bool any = false;
            foreach (var kv in Keys)
            {
                KeyCode keyCode = kv.Key;
                bool expected = kv.Value;
                bool gk = Input.GetKey(keyCode);
                if (gk != expected)
                {
                    return false;
                }
                any |= gk;
            }
            return any;
        }

        public bool GetKeyDown()
        {
            return GetKey() && Input.GetKeyDown(MainKey);
        }

        public new string ToString()
        {
            string result = string.Empty;
            if (!IsGood)
            {
                return result;
            }
            foreach (var kv in Keys)
            {
                KeyCode keyCode = kv.Key;
                bool expected = kv.Value;
                if (keyCode == MainKey || expected == false)
                {
                    continue;
                }
                string str = KeyCodeToString(keyCode);
                if (str.Length == 0)
                {
                    continue;
                }
                result += str + "+";
            }
            result += KeyCodeToString(MainKey);
            return result;
        }

        public void FromString(string keyString)
        {
	        if (string.IsNullOrEmpty(keyString))
	        {
	            return;
	        }
            string[] keys = keyString.Split(new char[] { '+', '-' });
            foreach (KeyCode keyCode in ModifierKeys)
            {
                Keys[keyCode] = false;
            }

            foreach (string key in keys)
            {
                KeyCode keyCode = StringToKeyCode(key.Trim(), KeyCode.None);
                if (keyCode == KeyCode.None)
                {
                    continue;
                }
                Keys[keyCode] = true;
                if (Array.IndexOf(ModifierKeys, keyCode) < 0)
                {
                    MainKey = keyCode;
                }
            }
        }

        public static string KeyCodeToString(KeyCode keyCode)
        {
            try
            {
                // Alpha0..Alpha9 は '0'..'9' に変換
                if (keyCode >= KeyCode.Alpha0 && keyCode <= KeyCode.Alpha9)
                {
                    return ((char)('0' + keyCode - KeyCode.Alpha0)).ToString();
                }
                return keyCode.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static KeyCode StringToKeyCode(string keyString, KeyCode defaultKeyCode)
        {
            try
            {
                if (string.IsNullOrEmpty(keyString))
                {
                    return defaultKeyCode;
                }

                if (keyString.Length == 1)
                {
                    Char c = keyString[0];
                    // '0'..'9' は "AlphaX" に変換してKeyCodeに
                    if (c >= '0' && c <= '9')
                    {
                        return string.Format("Alpha{0}", c).ToEnum(defaultKeyCode);
                    }
                    // 'a'..'z' は大文字に変換してKeyCodeに
                    if (c >= 'a' && c <= 'z')
                    {
                        return string.Format("{0}", Char.ToUpper(c)).ToEnum(defaultKeyCode);
                    }
                }

                return keyString.ToEnum(defaultKeyCode);
            }
            catch
            {
                return defaultKeyCode;
            }
        }
    }

    static readonly KeyCode[] ModifierKeys = {
        KeyCode.LeftShift,      KeyCode.RightShift,
        KeyCode.LeftControl,    KeyCode.RightControl,
        KeyCode.LeftAlt,        KeyCode.RightAlt,
        KeyCode.LeftWindows,    KeyCode.RightWindows,
        KeyCode.AltGr,
    };

    List<KeySet> KeySets { get; set; }

    public bool IsGood
    {
        get
        {
            return KeySets.Count > 0;
        }
    }

    public KeySetting()
    {
        Clear();
    }

    public KeySetting(string keyString) : this()
    {
        FromString(keyString);
    }

    public void Clear()
    {
        KeySets = new List<KeySet>();
    }

    public bool GetKey()
    {
        foreach (KeySet keySet in KeySets)
        {
            if (keySet.GetKey())
            {
                return true;
            }
        }
        return false;
    }

    public bool GetKeyDown()
    {
        foreach (KeySet keySet in KeySets)
        {
            if (keySet.GetKeyDown())
            {
                return true;
            }
        }
        return false;
    }

    public new string ToString()
    {
        string result = string.Empty;
        foreach (KeySet keySet in KeySets)
        {
            string keyStr = keySet.ToString();
            if (keyStr.Length != 0)
            {
                if (result.Length != 0)
                {
                    result += " | ";
                }
                result += keyStr;
            }
        }
        return result;
    }

    public void FromString(string keyString)
    {
        if (string.IsNullOrEmpty(keyString))
        {
            return;
        }
        string[] keys = Expand(keyString.Split(new char[] { '|' }));
        foreach (string key in keys)
        {
            KeySet keySet = new KeySet(key.Trim());
            if (keySet.IsGood)
            {
                KeySets.Add(keySet);
            }
        }
    }

    // モディファイアキーの展開
    string[] Expand(string[] keyStrings)
    {
        List<string> result = new List<string>(keyStrings);
        result = Expand(result, "Shift", new string[] { "LeftShift", "RightShift", "LeftShift+RightShift" });
        result = Expand(result, "Control", new string[] { "LeftControl", "RightControl", "LeftControl+RightControl" });
        result = Expand(result, "Ctrl", new string[] { "LeftControl", "RightControl", "LeftControl+RightControl" });
        result = Expand(result, "Alt", new string[] { "LeftAlt", "RightAlt", "LeftAlt+RightAlt" });
        return result.ToArray();
    }

    // 特定のモディファイアキーの展開
    List<string> Expand(List<string> list, string src, string[] dests)
    {
        List<string> result = new List<string>();
        foreach (string str in list)
        {
            if (!str.Contains(src))
            {
                result.Add(str);
                continue;
            }

            // str 内に dests が含まれないことを確認
            // 注意：この判定をしないと
            // "Alt+X"
            // -> "LeftAlt+X', "RightAlt+X"
            // -> "LeftLeftAlt+X", "LeftRightAlt+X", "RightLeftAlt+X", "RightRightAlt+X"
            // のように際限なく増えていってしまうので、省略しないこと
            bool flag = true;
            foreach (string d in dests)
            {
                if (str.Contains(d))
                {
                    flag = false;
                    break;
                }
            }
            if (!flag)
            {
                result.Add(str);
                continue;
            }

            // str 内の src を dests に置き換えたものを追加
            foreach (string d in dests)
            {
                result.Add(str.Replace(src, d));
            }
        }
        return result;
    }
}


// ウィンドウ定型処理
public class GuiWindow
{
    public delegate void GuiWindowFunction(int windowID, GuiWindow guiWindow);

    public string Title = "GUI Window";

    public Rect WindowRect = new Rect(Screen.width / 3, margin, Screen.width / 3, Screen.height / 3);
    public Rect TitleBarRect = new Rect(0, 0, Screen.width, handleSize);
    public float MinWidth = 128f;
    public float MinHeight = margin * 4f + handleSize;
    public float HandleSize = handleSize;
    public bool Collision = true;

    bool visible = false;
    public bool Visible
    {
        get
        {
            return visible;
        }
        set
        {
            if (!visible && value)
            {
                WindowRect.x = Mathf.Clamp(WindowRect.x, 0f, Screen.width - WindowRect.width);
                WindowRect.y = Mathf.Clamp(WindowRect.y, 0f, Screen.height - WindowRect.height);
            }
            visible = value;
        }
    }

    const float margin = 16f;
    const float handleSize = 24f;

    bool handleClicked = false;
    Vector2 clickedPosition = new Vector2(0f, 0f);
    Rect originalWindow = new Rect(0, 0, Screen.width, handleSize);

    public GuiWindow() : this("GUI Window")
    {
    }

    public GuiWindow(string title)
    {
        this.Title = title;
    }

    public void DrawGui(int id, GuiWindowFunction guiWindowFunc)
    {
        if (!Visible)
        {
            return;
        }
        GUI.WindowFunction wndproc = (windowId) =>
        {
            guiWindowFunc(windowId, this);
            GUI.DragWindow(TitleBarRect);
        };

        WindowRect = GUILayout.Window(id, WindowRect, wndproc, Title);

        DrawGuiPost(id);
    }

    public void DrawGui(int id, GUI.WindowFunction windowFunc)
    {
        GUI.WindowFunction wndproc = (windowId) =>
        {
            windowFunc(windowId);
            GUI.DragWindow(TitleBarRect);
        };

        WindowRect = GUILayout.Window(id, WindowRect, wndproc, Title);

        DrawGuiPost(id);
    }

    void DrawGuiPost(int id)
    {
        Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

        // その３>>84 より
        {
            bool enableGameGui = true;
            bool m = Input.GetAxis("Mouse ScrollWheel") != 0;
            for (int i = 0; i < 3; i++)
            {
                m |= Input.GetMouseButtonDown(i);
            }
            if (m)
            {
                enableGameGui = !WindowRect.Contains(mousePos);
            }
            GameMain.Instance.MainCamera.SetControl(enableGameGui);
            UICamera.InputEnable = enableGameGui;
        }

        // 右下をつかんでリサイズ
        // http://forum.unity3d.com/threads/is-there-a-resize-equivalent-to-gui-dragwindow.10144/#post-72530
        if (!Input.GetMouseButton(0))
        {
            handleClicked = false;
        }
        else if (handleClicked)
        {
            // 解像度変更に対応するためにここで計算すること
            float MaxWidth = Screen.width - margin * 2f;
            float MaxHeight = Screen.height - margin * 2f;
            WindowRect.width = Mathf.Clamp(originalWindow.width + (mousePos.x - clickedPosition.x), MinWidth, MaxWidth);
            WindowRect.height = Mathf.Clamp(originalWindow.height + (mousePos.y - clickedPosition.y), MinHeight, MaxHeight);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            Rect windowHandle = new Rect(
                WindowRect.x + WindowRect.width - HandleSize,
                WindowRect.y + WindowRect.height - HandleSize,
                HandleSize, HandleSize);
            if (windowHandle.Contains(mousePos))
            {
                handleClicked = true;
                clickedPosition = mousePos;
                originalWindow = WindowRect;
            }
        }
    }
}


internal static class Extension
{
    public static TEnum ToEnum<TEnum>(this string strEnumValue, TEnum defaultValue)
    {
        if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
            return defaultValue;
        return (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
    }
}


internal static class Icon
{
    public static byte[] Png =
    {
        0x89,0x50,0x4e,0x47,0x0d,0x0a,0x1a,0x0a,0x00,0x00,0x00,0x0d,0x49,0x48,0x44,0x52,
        0x00,0x00,0x00,0x20,0x00,0x00,0x00,0x20,0x08,0x02,0x00,0x00,0x00,0xfc,0x18,0xed,
        0xa3,0x00,0x00,0x00,0x03,0x73,0x42,0x49,0x54,0x08,0x08,0x08,0xdb,0xe1,0x4f,0xe0,
        0x00,0x00,0x00,0x09,0x70,0x48,0x59,0x73,0x00,0x00,0x16,0x25,0x00,0x00,0x16,0x25,
        0x01,0x49,0x52,0x24,0xf0,0x00,0x00,0x02,0x14,0x49,0x44,0x41,0x54,0x48,0x89,0xbd,
        0x96,0xcd,0x8e,0xaa,0x30,0x14,0xc7,0x8f,0x88,0x44,0x30,0xda,0x10,0x74,0x67,0x04,
        0x36,0x46,0x37,0xc6,0x9d,0x3e,0xc9,0x6d,0xe6,0x41,0x6e,0x32,0x8f,0x30,0xc9,0x7d,
        0x12,0x1e,0xc3,0x1d,0x4b,0x77,0x24,0xc6,0x40,0x70,0xe9,0x47,0x00,0x13,0x34,0x10,
        0xa8,0x77,0x51,0x83,0x8c,0xc3,0xd7,0x30,0xd7,0xfb,0xdf,0x10,0xda,0xf2,0xff,0xf5,
        0x9c,0xd3,0xd2,0x36,0x6e,0xb7,0x1b,0xbc,0x52,0xcc,0x4b,0xdd,0x01,0x80,0xa5,0x8f,
        0x30,0x0c,0x7d,0xdf,0x77,0x1c,0xc7,0x75,0xdd,0x20,0x08,0x00,0xa0,0x46,0x64,0xcd,
        0x66,0x93,0xe7,0x79,0x84,0x90,0x28,0x8a,0x82,0x20,0xb0,0x2c,0xfb,0x00,0xf8,0xbe,
        0x6f,0x9a,0xe6,0x66,0xb3,0x89,0xe3,0x38,0x8a,0xa2,0x7a,0x93,0x65,0x18,0x06,0x00,
        0xda,0xed,0xf6,0x64,0x32,0x51,0x14,0xa5,0xd7,0xeb,0x3d,0x00,0x87,0xc3,0x61,0xbb,
        0xdd,0x2e,0x16,0x0b,0x45,0x51,0xe8,0xb8,0xda,0x5a,0xaf,0xd7,0x86,0x61,0x20,0x84,
        0x28,0xe0,0xdf,0xd7,0x60,0x3e,0x9f,0xa7,0x5f,0xef,0x80,0xd3,0xe9,0xd4,0xed,0x76,
        0x7f,0x3e,0x7d,0x00,0x08,0xc3,0x30,0x08,0x02,0xcf,0xf3,0x3e,0x01,0x2e,0x97,0x0b,
        0xcb,0xb2,0x99,0xee,0x9a,0xa6,0x7d,0x0b,0xc0,0x71,0x1c,0x21,0xe4,0x7a,0xbd,0x7e,
        0x02,0xc4,0x71,0x9c,0x39,0x5a,0xd3,0xb4,0x9d,0x65,0x7d,0x0b,0xf0,0x64,0x58,0x94,
        0x90,0xda,0xee,0x69,0xb1,0x55,0xdc,0xff,0x7c,0x7c,0x3c,0xf5,0x8e,0x54,0x75,0x3a,
        0x9d,0x22,0x84,0x64,0x59,0xae,0x03,0x28,0x9d,0xfb,0xce,0xb2,0x76,0x96,0x35,0x52,
        0x55,0x00,0x28,0x66,0x64,0xa7,0x08,0x63,0x4c,0x3f,0x2e,0xd6,0xce,0xb2,0x74,0x5d,
        0x2f,0x1e,0x93,0x9b,0x22,0x8c,0x71,0x12,0xc7,0xef,0xf7,0xf7,0xa4,0xdd,0xb6,0x6d,
        0x5d,0xd7,0x93,0xf8,0x76,0x96,0xa5,0x69,0x1a,0xc6,0x38,0xcf,0xa7,0xa8,0xc8,0x99,
        0x71,0xc8,0xb2,0x8c,0x31,0xfe,0xf5,0xf6,0x96,0x74,0x15,0x27,0xb3,0x64,0x5b,0xe5,
        0xe5,0x4a,0x96,0xe5,0xe5,0x72,0x59,0xfc,0x6d,0x25,0x00,0x65,0x64,0xb6,0x97,0xae,
        0x9f,0xaa,0x80,0x3c,0xd9,0xb6,0xfd,0x42,0x00,0x2d,0x75,0x95,0x91,0xb9,0xab,0x28,
        0xcf,0xd7,0xf3,0x3c,0xc3,0x30,0xd2,0x85,0x2d,0x5e,0xd0,0xe5,0x80,0xaf,0xdb,0x38,
        0xad,0x91,0xaa,0x16,0xac,0x51,0xc8,0x4c,0x11,0x21,0x84,0x10,0x52,0x0a,0xae,0xe2,
        0x0e,0x79,0x11,0x94,0x9e,0x0a,0xf4,0x5f,0x34,0x9b,0xcd,0x4a,0x27,0x91,0x01,0x78,
        0x72,0x4f,0x6f,0xe3,0x3c,0x45,0x51,0x44,0x8f,0xf8,0x0c,0x37,0xfa,0x68,0x34,0x1a,
        0xa5,0x2e,0x05,0xfa,0xea,0x9e,0x18,0xde,0x01,0x3c,0xcf,0x03,0x40,0xc5,0xd4,0x17,
        0x8b,0x10,0xc2,0x30,0x0c,0x35,0x84,0xff,0x70,0xf1,0xba,0x03,0x24,0x49,0x72,0x5d,
        0x77,0xbf,0xdf,0xff,0x30,0x08,0x42,0x88,0x69,0x9a,0xad,0x56,0x0b,0x21,0x44,0x5b,
        0xee,0xb9,0xeb,0xf7,0xfb,0xc3,0xe1,0x70,0xb5,0x5a,0x25,0x1d,0xb5,0x01,0x9e,0xe7,
        0x8d,0xc7,0x63,0x51,0x14,0x69,0x4b,0x83,0x5e,0x11,0x83,0x20,0x38,0x9f,0xcf,0xc7,
        0xe3,0xd1,0x71,0x1c,0xdf,0xf7,0xa1,0xee,0xd5,0x51,0x10,0x04,0x49,0x92,0x06,0x83,
        0x41,0xa7,0xd3,0xe1,0x38,0xee,0x01,0x78,0x9d,0x5e,0x5e,0xe4,0xbf,0x1a,0xa7,0xe7,
        0x15,0xdb,0x16,0x94,0x23,0x00,0x00,0x00,0x00,0x49,0x45,0x4e,0x44,0xae,0x42,0x60,
        0x82,
    };
}


#if USE_NOTIFIER
// 変更した場合はネームスペースの名前の数値を増やすこと (V987 を更新したら V988 にする)
// ・なぜ変更するのか？
//      AddComponentしたものをDestroyImmediateした後、
//      定義は異なる(アセンブリが異なる)が同一のネームスペース、クラスを持つインスタンスを追加すると、
//      OnGUI等でDestroyImmediateしたはずのコンポーネントのメソッドが呼ばれる。
//
//      これを回避するため、ネームスペースを変更して、異なるものであることを明示する
//
namespace CM3D2.Plugin.Notifier.V1
{
    public static class Notifier
    {
        // バージョン番号：ネームペースの末尾と同じ値にすること (V987 なら 987 にする)
        public static readonly int Version = 1; // V1

        // 標準の表示時間
        public static readonly float DefaultDuration = 5.0f;
        public static readonly Color DefaultColor = Color.red;

        static GameObject UnityInjector_ = null;

        static GameObject UnityInjector
        {
            get
            {
                if (UnityInjector_ == null)
                {
                    UnityInjector_ = GameObject.Find("UnityInjector");
                }
                return UnityInjector_;
            }
        }

        public static void Notify(string message)
        {
            Notify(DefaultColor, DefaultDuration, message);
        }

        public static void Notify(Color color, string message)
        {
            Notify(color, DefaultDuration, message);
        }

        public static void Notify(string format, params object[] args)
        {
            Notify(DefaultColor, DefaultDuration, format, args);
        }

        public static void Notify(Color color, string format, params object[] args)
        {
            Notify(color, DefaultDuration, format, args);
        }

        public static void Notify(float duration, string format, params object[] args)
        {
            Notify(DefaultColor, duration, format, args);
        }

        public static void Notify(Color color, float duration, string format, params object[] args)
        {
            Notify(color, duration, string.Format(format, args));
        }

        public static void Notify(Color color, float duration, string message)
        {
            // 共有コンポーネントを見つける (無い場合は登録)
            //  見ず知らずのほかのプラグインと協調するために、エントリの保持、レンダリング部分は
            //  共有コンポーネントとして外部に出している。
            MonoBehaviour sv = NotifierSharedComponent.GetInstance(UnityInjector);
            if (sv == null)
            {
                return;
            }

            // 共有コンポーネント経由でコルーチンを登録
            sv.StartCoroutine(Coroutine(sv, color, duration, message));
        }

        static IEnumerator Coroutine(MonoBehaviour sv, Color color, float duration, string message)
        {
            // エントリ登録
            object entry = NotifierSharedComponent.AddEntry(sv);

            // メッセージと色を指定
            NotifierSharedComponent.SetMessage(entry, message);
            NotifierSharedComponent.SetColor(entry, color);

            // 指定された期間待つ
            yield return new WaitForSeconds(duration);

            // エントリ削除
            NotifierSharedComponent.RemoveEntry(sv, entry);
        }
    }

    //  知らないプラグインと値を共有するためのコンポーネント
    class NotifierSharedComponent : SharedComponentBase
    {
        // 最大表示時間。これを過ぎても削除を行わない場合、自動的に削除される
        static readonly float MaxDuration = 30.0f;

        // 違うクラス間で同じ仲間を見つけるための共有コンポーネント名。バージョン変更時も同じ値にすること
        static readonly string SharedComponentName = "CM3D2.SharedComponent.Notifier";

        int IdCounter = 0;
        float AutoClearTime = 0f;
        SortedDictionary<int, NotifierEntry> Messages = new SortedDictionary<int, NotifierEntry>();
        GUIStyle guiStyle = new GUIStyle();

        NotifierSharedComponent()
        {
            guiStyle.alignment = TextAnchor.UpperRight;
            guiStyle.fontSize = 16;
            guiStyle.normal.textColor = Color.red;
        }

        int CreateId()
        {
            return IdCounter++;
        }

        // インスタンス取得(無い場合は登録)
        public static MonoBehaviour GetInstance(GameObject goUnityInjector)
        {
            return SharedComponentBase.GetInstance(
                goUnityInjector,
                typeof(NotifierSharedComponent),
                SharedComponentName,
                Notifier.Version);
        }

        // エントリ追加
        //  戻り値の型は隠蔽し、アクセサを別に提供すること。
        public static object AddEntry(MonoBehaviour instance)
        {
            return SharedComponentBase.Invoke(instance, "AddEntry_");
        }

        // エントリ削除
        public static void RemoveEntry(MonoBehaviour instance, object entry)
        {
            SharedComponentBase.Invoke(instance, "RemoveEntry_", new object[] { entry });
        }

        // 表示する文字列を設定
        //  NotifierEntry の型は知らないものとして、リフレクション経由で扱うこと
        public static void SetMessage(object notifierEntry, string message)
        {
            if (notifierEntry == null)
            {
                return;
            }
            SetField(notifierEntry.GetType(), notifierEntry, "message", message);
        }

        // 色を設定
        //  NotifierEntry の型は知らないものとして、リフレクション経由で扱うこと
        public static void SetColor(object notifierEntry, Color color)
        {
            if (notifierEntry == null)
            {
                return;
            }
            var guiStyle = GetField<GUIStyle>(notifierEntry.GetType(), notifierEntry, "guiStyle");
            if (guiStyle != null)
            {
                guiStyle.normal.textColor = color;
            }
        }

        // ここから下のメソッドは NotifierEntry のことを知っているものとする
        public object AddEntry_()
        {
            enabled = true;
            AutoClearTime = Time.realtimeSinceStartup + MaxDuration;

            var notifierEntry = new NotifierEntry()
            {
                id = CreateId(),
                message = string.Empty,
                guiStyle = new GUIStyle(guiStyle),
                height = guiStyle.fontSize
            };
            Messages[notifierEntry.id] = notifierEntry;
            return notifierEntry;
        }

        public void RemoveEntry_(object entry)
        {
            var notifierEntry = entry as NotifierEntry;
            if (notifierEntry == null)
            {
                return;
            }

            // 注意：キーが無い場合でもRemoveで例外は起きない
            // https://msdn.microsoft.com/en-us/library/7ydsbw8w%28v=vs.110%29.aspx#Anchor_2
            Messages.Remove(notifierEntry.id);
            if (Messages.Count == 0)
            {
                enabled = false;
            }
        }

        void Update()
        {
            // 最後の登録から一定以上時間が経過したら自動的に削除する
            if (Time.realtimeSinceStartup >= AutoClearTime)
            {
                Messages.Clear();
                enabled = false;
            }
        }

        void OnGUI()
        {
            int y = 0;
            foreach (KeyValuePair<int, NotifierEntry> kv in Messages)
            {
                NotifierEntry e = kv.Value;
                GUI.Label(
                    new Rect(0, y, Screen.width, e.height),
                    e.message,
                    e.guiStyle);
                y += e.height;
            }
        }

        class NotifierEntry
        {
            public int id;
            public string message;
            public GUIStyle guiStyle;
            public int height;
        }
    }

    //
    //  ・"UnityInjector"に対してAddComponentで追加される
    //  ・このため、通常のUpdateやOnGUIも動作する
    //
    class SharedComponentBase : MonoBehaviour
    {
        public static readonly string SharedComponentNameFieldName = "ComponentName";
        public static readonly string VersionFieldName = "Version";

        public string ComponentName = null;
        public int Version = 0;

        //  インスタンスの取得/登録
        //  go                  コンポーネントを保持するGameObject。通常は"UnityInjector"のインスタンス
        //  targetComponentType 取得/登録したいコンポーネントの型
        //  sharedComponentName 共有コンポーネントの名前
        //  version             未登録だった場合に登録したいバージョン
        //
        //  登録されているコンポーネントのバージョンが、versionStringと同じか新しい場合、そのインスタンスを返す
        //  未登録もしくは登録済みのもののバージョンが古い場合、新たに登録し、そのインスタンスを返す
        //
        public static MonoBehaviour GetInstance(GameObject go, Type targetComponentType, string sharedComponentName, int version)
        {
            if (go == null)
            {
                return null;
            }

            MonoBehaviour result = null;
            foreach (MonoBehaviour mb in go.GetComponents<MonoBehaviour>())
            {
                string scName = GetField<string>(mb, SharedComponentNameFieldName);
                int ver = GetField<int>(mb, VersionFieldName);

                // 識別できる名前を持っていなければ無視
                if (scName == null || scName != sharedComponentName)
                {
                    continue;
                }

                // 登録済みのコンポーネントが名無しか、自分より古いなら削除
                if (ver == 0 || ver < version)
                {
                    GameObject.DestroyImmediate(mb);
                    continue;
                }

                result = mb;
            }

            // 自分より新しいものを見つけたのでそれを返す
            if (result != null)
            {
                return result;
            }

            // 見つからなかったので新規登録
            var v = go.AddComponent(targetComponentType);
            if (v != null)
            {
                SetField(targetComponentType, v, VersionFieldName, version);
                SetField(targetComponentType, v, SharedComponentNameFieldName, sharedComponentName);
            }
            return v as MonoBehaviour;
        }

        public static T GetField<T>(object instance, string fieldName)
        {
            return GetField<T>(instance.GetType(), instance, fieldName);
        }

        public static T GetField<T>(Type type, object instance, string fieldName)
        {
            try
            {
                FieldInfo fi = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                return (T)fi.GetValue(instance);
            }
            catch
            {
                return default(T);
            }
        }

        public static void SetField(Type type, object instance, string fieldName, object value)
        {
            try
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (field == null)
                {
                    return;
                }
                field.SetValue(instance, value);
            }
            catch
            {
            }
        }

        public static object Invoke(object instance, string methodName)
        {
            return Invoke(instance, methodName, new object[] { });
        }

        public static object Invoke(object instance, string methodName, object[] args)
        {
            try
            {
                MethodInfo mi = instance.GetType().GetMethod(methodName);
                return mi.Invoke(instance, args);
            }
            catch
            {
                return null;
            }
        }
    }
}
#endif

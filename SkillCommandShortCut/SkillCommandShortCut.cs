using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityInjector.Attributes;

namespace CM3D2.SkillCommandShortCut.Plugin
{
    [PluginName("CM3D2 Skill Command Shortcut"), PluginVersion("0.1.5.0")]
    public class SkillCommandShortCut : UnityInjector.PluginBase
    {
        Config config = new Config();
        GameObject gearMenuButton = null;
        Dictionary<KeyCode, int> dicKeyNameToIndex;
        List<Label> labels;
        const float labelWidth = 18f;
        GUIStyle labelStyle = null;
        GUIStyle nullStyle = null;
        int maxLabelHeight = 48;

        void Awake()
        {
            Init();
        }

        void Init()
        {
            GameObject.DontDestroyOnLoad(this);
            gearMenuButton = GearMenu.Buttons.Add(Name, this, Icon.Png, (go) => { enabled = !enabled; });
            config.LoadIni(Preferences);

            dicKeyNameToIndex = new Dictionary<KeyCode, int>();
            for (int i = 0; i < config.Shortcuts.Length; i++)
            {
                dicKeyNameToIndex.Add(config.Shortcuts[i], i);
            }
            enabled = false;
            enabled = config.Enable;
        }

        void OnApplicationQuit()
        {
            config.Enable = enabled;
            config.SaveIni(Preferences);
            SaveConfig();
        }

        void OnEnable()
        {
            GearMenu.Buttons.SetFrameColor(gearMenuButton, Color.red);
        }

        void OnDisable()
        {
            GearMenu.Buttons.ResetFrameColor(gearMenuButton);
        }

        void OnGUI()
        {
            if (labelStyle == null)
            {
                nullStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
                labelStyle = new GUIStyle(GUI.skin.GetStyle("Box"));
                labelStyle.margin = new RectOffset(0, 0, 0, 0);
                labelStyle.padding = new RectOffset(0, 3, 0, 0);
                labelStyle.alignment = TextAnchor.MiddleCenter;
            }
            if (!IsGuiVisible())
            {
                return;
            }

            if (labels == null)
            {
                return;
            }

            foreach (Label label in labels)
            {
                GUI.Label(label.rect, label.name.ToUpper(), label.style);
            }
        }

        void Update()
        {
            try
            {
                Update2();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : {0}", e);
            }
        }

        public void Update2()
        {
            labels = null;

            if (!IsGuiVisible())
            {
                return;
            }

            if (UpdateDialogClick())
            {
                return;
            }
            if (UpdateNextButtonClick())
            {
                return;
            }
            if (UpdateCancelButtonClick())
            {
                return;
            }

            //  昼コミュニケーション選択
            //  夜伽メイド会話選択
            if (Application.loadedLevel == 15)
            {
                GameObject go = GameObject.Find("__GameMain__/SystemUI Root/MessageWindowPanel/SelectorViewer/Contents/SelectButtonParent");
                UpdateLabels(go, "selectButton", false);
                UIButton uiButton = UpdateKeys();
                if (uiButton != null)
                {
                    return;
                }
            }

            //  昼コミュニケーションメイド選択
            //  夜伽メイド選択画面
            if (Application.loadedLevel == 1)
            {
                GameObject go = GameObject.Find("UI Root/Parent/CharacterSelectPanel/Contents/MaidSkillUnitParent");
                UpdateMaidLabels(go, "MaidPlate(Clone)", "Button");
                UIButton uiButton = UpdateKeys();
                if (uiButton != null)
                {
                    return;
                }
            }

            //  夜伽ステージ選択
            if (Application.loadedLevel == 14)
            {
                GameObject go = GameObject.Find("UI Root/StageSelectPanel/StageSelectViewer/StageViewer/Contents/StageUnitParent");
                UpdateMaidLabels(go, "StageUnit(Clone)", "Parent");
                UIButton uiButton = UpdateKeys();
                if (uiButton != null)
                {
                    return;
                }
            }

            // 夜伽コマンド実行
            {
                GameObject go = GameObject.Find("/UI Root/YotogiPlayPanel/CommandViewer/SkillViewer/MaskGroup/SkillGroup/CommandParent/CommandUnit");
                UpdateLabels(go, "cm:", true);
                UIButton uiButton = UpdateKeys();
                if (uiButton != null)
                {
                    return;
                }
            }
        }

        void UpdateLabels(GameObject ui, string prefix, bool offset)
        {
            if (ui == null || !ui.activeInHierarchy || !GameMain.Instance.MainCamera.IsFadeStateNon())
            {
                return;
            }

            labels = new List<Label>();

            foreach (Transform t in ui.transform)
            {
                if (!t.name.StartsWith(prefix) || t.gameObject == null)
                {
                    continue;
                }

                int index = labels.Count;
                var kv = dicKeyNameToIndex.FirstOrDefault(e => e.Value == index);
                if (kv.Key == Config.InvalidKeyCode)
                {
                    continue;
                }

                Rect rect = GetBoxColliderRect(t.gameObject);
                rect.width = labelWidth;
                if (offset)
                {
                    rect.x -= rect.width;
                }

                if (rect.height > maxLabelHeight)
                {
                    rect.y += (rect.height - maxLabelHeight) / 2;
                    rect.height = maxLabelHeight;
                }

                labels.Add(new Label
                {
                    gameObject = t.gameObject,
                    index = index,
                    rect = rect,
                    name = Config.KeyCodeToString(kv.Key),
                    style = labelStyle
                });
            }
        }

        void UpdateMaidLabels(GameObject ui, string prefix, string childName)
        {
            if (ui == null || !ui.activeInHierarchy || !GameMain.Instance.MainCamera.IsFadeStateNon())
            {
                return;
            }

            labels = new List<Label>();

            foreach (Transform tt in ui.transform)
            {
                if (!tt.name.StartsWith(prefix) || tt.gameObject == null)
                {
                    continue;
                }

                Transform t = tt.Find(childName);
                if (t == null || t.gameObject == null)
                {
                    continue;
                }

                int index = labels.Count;
                var kv = dicKeyNameToIndex.FirstOrDefault(e => e.Value == index);
                if (kv.Key == Config.InvalidKeyCode)
                {
                    continue;
                }

                Rect rect = GetBoxColliderRect(t.gameObject);
                var style = labelStyle;
                string name = Config.KeyCodeToString(kv.Key);
                if (rect.width == 0 && rect.height == 0)
                {
                    style = nullStyle;
                    name = "";
                }
                else
                {
                    rect.width = labelWidth;
                }

                if (rect.height > maxLabelHeight)
                {
                    rect.y += (rect.height - maxLabelHeight) / 2;
                    rect.height = maxLabelHeight;
                }

                labels.Add(new Label
                {
                    gameObject = t.gameObject,
                    index = index,
                    rect = rect,
                    name = name,
                    style = style
                });
            }
        }

        UIButton UpdateKeys()
        {
            var kv = dicKeyNameToIndex.FirstOrDefault(e => Input.GetKeyDown(e.Key));
            if (kv.Key == Config.InvalidKeyCode || kv.Key == KeyCode.None)
            {
                return null;
            }

            if (labels == null)
            {
                return null;
            }

            int index = kv.Value;
            Label label = labels.FirstOrDefault(e => e.index == index);
            if (label == null)
            {
                return null;
            }

            UIButton uiButton = label.gameObject.GetComponent<UIButton>();
            if (uiButton == null || !uiButton.isEnabled)
            {
                return null;
            }
            uiButton.SendMessage("OnClick");
            return uiButton;
        }

        // ダイアログの「OK」「Cancel」をクリックする
        bool UpdateDialogClick()
        {
            SystemDialog systemDialog = GameMain.Instance.SysDlg;
            if (systemDialog.gameObject.activeSelf)
            {
                if (Input.GetKeyDown(config.Ok))
                {
                    SystemDialogOnClickOk(systemDialog);
                    return true;
                }
                if (Input.GetKeyDown(config.Cancel))
                {
                    SystemDialogOnClickCancel(systemDialog);
                    return true;
                }
            }
            return false;
        }

        bool UpdateNextButtonClick()
        {
            if (!Input.GetKeyDown(config.Ok))
            {
                return false;
            }

            // メイド選択
            if (Application.loadedLevel == 1)
            {
                // メイド選択画面での「OK」ボタン
                if (ClickGameObject("/UI Root/Parent/ButtonParent/OK"))
                {
                    return true;
                }
                return false;
            }

            // メニュー
            if (Application.loadedLevel == 3)
            {
                // 昼/夜メニューの「Next」ボタン
                if (ClickGameObject("/UI Root/DailyPanel/Next"))
                {
                    return true;
                }
                // 昼/夜結果報告画面の「OK」ボタン
                if (ClickGameObject("/UI Root/ResultWorkPanel/Ok"))
                {
                    return true;
                }
                // 収支報告画面の「OK」ボタン
                if (ClickGameObject("/UI Root/ResultIncomePanel/Ok"))
                {
                    return true;
                }
                return false;
            }

            // 夜伽
            if (Application.loadedLevel == 14)
            {
                // 夜伽ステージ選択時の「OK」ボタン
                if (ClickGameObject("/UI Root/StageSelectPanel/Ok"))
                {
                    return true;
                }

                // 夜伽スキル選択時の「Next」ボタン
                if (ClickGameObject("/UI Root/SkillSelectPanel/CommonPanel/UnderButtonGroup/Next"))
                {
                    return true;
                }

                // 夜伽スキル実行中の「Next」ボタン
                //              if (ClickGameObject("/UI Root/YotogiPlayPanel/CommonPanel/UnderButtonGroup/Next"))
                if (ClickGameObject("/UI Root/YotogiPlayPanel/UndressingViewer/CommonPanel/UnderButtonGroup/Next"))
                {
                    return true;
                }

                // 夜伽完了画面の「Next」ボタン
                if (ClickGameObject("/UI Root/ResultPanel/CommonPanel/UnderButtonGroup/Next"))
                {
                    return true;
                }
                return false;
            }

            return false;
        }

        bool UpdateCancelButtonClick()
        {
            if (!Input.GetKeyDown(config.Cancel))
            {
                return false;
            }

            if (Application.loadedLevel == 1)
            {
                // 夜伽メイド選択画面の「Cancel」ボタン
                if (ClickGameObject("/UI Root/Parent/ButtonParent/Cancel"))
                {
                    return true;
                }
                return false;
            }

            return false;
        }

        bool IsGuiVisible()
        {
            Camera camera = UICamera.currentCamera;
            return camera != null && camera.enabled;
        }

        static bool ClickGameObject(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
            {
                UIButton uiButton = go.GetComponent<UIButton>();
                if (uiButton != null)
                {
                    if (uiButton.isEnabled)
                    {
                        uiButton.SendMessage("OnClick");
                        return true;
                    }
                }
            }
            else
            {
#if DEBUG
                Console.WriteLine("{0} is not found", name);
#endif
            }
            return false;
        }

        static Rect GetBoxColliderRect(GameObject gameObject)
        {
            BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                return new Rect();
            }

            // http://answers.unity3d.com/questions/295408/
            Vector3 pt = boxCollider.transform.TransformPoint(boxCollider.center);
            pt = UICamera.currentCamera.WorldToScreenPoint(pt);
            float width = boxCollider.size.x;
            float height = boxCollider.size.y;

            return new Rect(
                pt.x - width / 2,
                Screen.height - pt.y - height / 2,
                width,
                height
            );
        }

        static void SystemDialogOnClickOk(SystemDialog systemDialog)
        {
            // class SystemDialog { private void OnClickOk(); }
            MethodInfo methodInfo = typeof(SystemDialog).GetMethod(
                "OnClickOk",
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { },
                null
            );
            methodInfo.Invoke(systemDialog, new object[] { });
        }

        static void SystemDialogOnClickCancel(SystemDialog systemDialog)
        {
            // class SystemDialog { private void OnClickCancel(); }
            MethodInfo methodInfo = typeof(SystemDialog).GetMethod(
                "OnClickCancel",
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { },
                null
            );
            methodInfo.Invoke(systemDialog, new object[] { });
        }
    }

    internal class Label
    {
        public GameObject gameObject;
        public int index;
        public Rect rect;
        public string name;
        public GUIStyle style;
    }
}


internal class Config
{
    public bool Enable;
    public KeyCode Ok;
    public KeyCode Cancel;
    public KeyCode[] Shortcuts = new KeyCode[20];
    public string DefaultShortcuts = "123456789abcdefghijk";
    public const KeyCode InvalidKeyCode = KeyCode.RightApple; // 別にアップルが嫌いなわけではない
    string section = "Config";

    public Config()
    {
        Enable = false;
        Ok = KeyCode.Return;
        Cancel = KeyCode.Backspace;
        for (int i = 0; i < 20; i++)
        {
            Shortcuts[i] = StringToKeyCode(DefaultShortcuts[i].ToString());
        }
    }

    public void LoadIni(ExIni.IniFile ini)
    {
        bool b;
        if (bool.TryParse(ini[section]["Enable"].Value, out b))
        {
            Enable = b;
        }
        Ok = GetKeyCode(ini, section, "Ok", Ok);
        Cancel = GetKeyCode(ini, section, "Cancel", Cancel);
        for (int i = 0; i < 20; i++)
        {
            Shortcuts[i] = GetKeyCode(ini, section, string.Format("Shortcut_{0}", i), Shortcuts[i]);
        }
    }

    public void SaveIni(ExIni.IniFile ini)
    {
        ini[section]["Enable"].Value = Enable.ToString();
        SetKeyCode(ini, section, "Ok", Ok);
        SetKeyCode(ini, section, "Cancel", Cancel);
        for (int i = 0; i < 20; i++)
        {
            SetKeyCode(ini, section, string.Format("Shortcut_{0}", i), Shortcuts[i]);
        }
    }

    public static void SetKeyCode(ExIni.IniFile ini, string section, string key, KeyCode keyCode)
    {
        ini[section][key].Value = KeyCodeToString(keyCode);
    }

    public static KeyCode GetKeyCode(ExIni.IniFile ini, string section, string key, KeyCode defautlValue)
    {
        KeyCode keyCode = StringToKeyCode(GetString(ini, section, key));
        if (keyCode == InvalidKeyCode)
        {
            return defautlValue;
        }
        return keyCode;
    }

    public static KeyCode StringToKeyCode(string str)
    {
        if (str == null)
        {
            return InvalidKeyCode;
        }
        if (str.Length == 0)
        {
            return InvalidKeyCode;
        }

        KeyCode keyCode = FirstLetterToUpper(str.ToLower()).ToEnum(InvalidKeyCode);
        if (keyCode != InvalidKeyCode)
        {
            return keyCode;
        }

        if (str.Length == 1)
        {
            Char c = str[0];
            if (c >= '0' && c <= '9')
            {
                return string.Format("Alpha{0}", c).ToEnum(InvalidKeyCode);
            }
            if (c >= 'a' && c <= 'z')
            {
                return string.Format("{0}", Char.ToUpper(c)).ToEnum(InvalidKeyCode);
            }
        }
        return InvalidKeyCode;
    }

    static string GetString(ExIni.IniFile ini, string section, string key)
    {
        if (!ini.HasSection(section))
        {
            return null;
        }
        if (!ini[section].HasKey(key))
        {
            return null;
        }
        return ini[section][key].Value;
    }

    public static string KeyCodeToString(KeyCode keyCode)
    {
        if (keyCode >= KeyCode.Alpha0 && keyCode <= KeyCode.Alpha9)
        {
            return ((char)('0' + keyCode - KeyCode.Alpha0)).ToString();
        }
        return keyCode.ToString();
    }

    // http://stackoverflow.com/a/4405876/2132223
    public static string FirstLetterToUpper(string str)
    {
        if (str == null)
            return null;

        if (str.Length > 1)
            return char.ToUpper(str[0]) + str.Substring(1);

        return str.ToUpper();
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
        0x01,0x49,0x52,0x24,0xf0,0x00,0x00,0x01,0xc3,0x49,0x44,0x41,0x54,0x48,0x89,0xed,
        0x96,0xbd,0x8a,0xc2,0x40,0x10,0xc7,0x27,0x1a,0x83,0x1b,0xd1,0x25,0x44,0x3b,0x8b,
        0x54,0x22,0x16,0xb6,0xe9,0x04,0x2b,0x21,0xef,0x73,0xe0,0x23,0x08,0xbe,0x89,0x0f,
        0x20,0x58,0xda,0xf8,0x00,0x29,0x74,0x11,0x09,0x1c,0x58,0xf9,0x41,0x12,0x61,0x23,
        0x09,0x66,0xbd,0x22,0xe2,0x47,0x6e,0xcf,0x33,0x81,0x58,0xf9,0x6f,0x42,0x66,0x76,
        0xff,0xbf,0x5d,0x86,0x61,0x56,0x38,0x9f,0xcf,0x90,0xa5,0x72,0x99,0xba,0x03,0x80,
        0x18,0x7d,0x82,0x20,0xa0,0x94,0xda,0xb6,0xed,0x38,0x8e,0xef,0xfb,0x00,0x90,0xe2,
        0x66,0xf9,0x7c,0x1e,0x21,0x84,0x31,0x56,0x14,0x45,0x96,0x65,0x51,0x14,0x6f,0x00,
        0x4a,0xa9,0x65,0x59,0xcb,0xe5,0x32,0x0c,0xc3,0xd3,0xe9,0x94,0xee,0xb0,0xb9,0x5c,
        0x0e,0x00,0x8a,0xc5,0x62,0xb3,0xd9,0xd4,0x34,0xad,0x52,0xa9,0xdc,0x00,0xdb,0xed,
        0x76,0xb5,0x5a,0xe9,0xba,0xae,0x69,0x5a,0xb4,0x2e,0x9d,0x82,0x20,0x98,0xcf,0xe7,
        0x8b,0xc5,0x02,0x63,0x1c,0x01,0xde,0x55,0x83,0xfd,0x7e,0x5f,0x2e,0x97,0xb9,0xc7,
        0x37,0x4d,0x73,0x32,0x1e,0x73,0x37,0xf7,0x0c,0xa3,0xdd,0x6e,0xdf,0x47,0x24,0x49,
        0x6a,0xb5,0x5a,0xa6,0x69,0xba,0xae,0xfb,0x00,0xf0,0x3c,0x4f,0x14,0x45,0xae,0x3b,
        0x21,0xa4,0xd3,0xed,0x72,0x01,0x84,0x10,0x00,0xf8,0xcd,0x60,0x8c,0x1d,0x8f,0xc7,
        0x07,0x40,0x18,0x86,0x5c,0x8b,0xc9,0x78,0xdc,0xe9,0x76,0x75,0x5d,0xe7,0x66,0xa3,
        0x05,0x31,0x40,0xcc,0xf0,0xff,0x1a,0x3c,0x71,0x7f,0x92,0xba,0x2a,0x59,0x91,0x87,
        0x83,0xc1,0x70,0x30,0x48,0xb4,0x25,0x01,0x60,0x34,0x1a,0x25,0xb2,0x4e,0x0c,0xf8,
        0xb6,0xac,0x6c,0x01,0x5f,0xfd,0x7e,0xb6,0x80,0x74,0xfa,0x00,0x3e,0x80,0x7b,0x5d,
        0x7b,0x38,0x51,0x33,0xbf,0x6b,0x1e,0xbc,0xa2,0xac,0x1a,0x6d,0x36,0x9b,0xa5,0x48,
        0x5d,0x75,0xb9,0x81,0x20,0x08,0xdc,0x67,0x44,0xcf,0x30,0x08,0x21,0x7f,0x19,0xad,
        0xd7,0xeb,0x9e,0x61,0x70,0x53,0x82,0x20,0x3c,0x00,0x10,0x42,0x9e,0xe7,0x31,0xc6,
        0x62,0x43,0x2d,0x1a,0x26,0xaf,0x8f,0x4c,0x00,0x88,0x4c,0x10,0x42,0x0f,0x80,0x58,
        0x3a,0xc6,0xe0,0xce,0x2c,0xae,0x18,0x63,0xb1,0xc8,0xc5,0x4b,0x55,0x55,0xc7,0x71,
        0x36,0x9b,0xcd,0x8b,0x46,0x4f,0x64,0x59,0x56,0xa1,0x50,0xc0,0x18,0x47,0xbf,0x97,
        0x1b,0x54,0xab,0xd5,0x7a,0xbd,0x3e,0x9d,0x4e,0xaf,0x89,0x74,0x62,0x8c,0xb9,0xae,
        0xdb,0x68,0x34,0x14,0x45,0x89,0x22,0x97,0xda,0xfa,0xbe,0x7f,0x38,0x1c,0x76,0xbb,
        0x9d,0x6d,0xdb,0x94,0x52,0x48,0xfb,0x74,0x94,0x65,0x59,0x55,0xd5,0x5a,0xad,0x56,
        0x2a,0x95,0x24,0x49,0xba,0x01,0xb2,0x53,0xe6,0x9d,0xfc,0x03,0x83,0xe9,0xbe,0x59,
        0xe0,0xe9,0x60,0xc3,0x00,0x00,0x00,0x00,0x49,0x45,0x4e,0x44,0xae,0x42,0x60,0x82,
    };
}

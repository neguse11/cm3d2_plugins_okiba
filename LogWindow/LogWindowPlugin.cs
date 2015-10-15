using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityInjector.Attributes;

namespace CM3D2.LogWindow.Plugin
{
    [PluginName("CM3D2 LogWindow"), PluginVersion("0.2.0.0")]
    public class LogWindow : UnityInjector.PluginBase
    {
        GameObject gearMenuButton = null;
        ConsoleMirror consoleMirror;
        List<string> lines = new List<string>();

        string configToggleKey = string.Empty;
        bool configVisible = false;
        bool configConsole = true;
        bool configMirror = false;
        string configMirrorPath = "LogWindow.log";
        bool configCollision = false;
        bool configAutoScroll = true;
        int configMaxLines = 300;

        Vector2 scrollPosition;

        GuiWindow guiWindow = new GuiWindow("Debug Log");
        GUIContent clearLabel = new GUIContent("Clear");
        GUIContent autoScrollLabel = new GUIContent("Auto Scroll");
        int lastLineCount = 0;

        GUIStyle logStyle;
        GUIStyle buttonStyle;
        GUIStyle toggleStyle;

        public void Awake()
        {
            GameObject.DontDestroyOnLoad(this);
            Init();
        }

        void Init()
        {
            gearMenuButton = GearMenu.Buttons.Add(Name, this, Icon.Png, (go) => { Toggle(); });

            configVisible = GetPreferences("Config", "Visible", configVisible);
            configConsole = GetPreferences("Config", "Console", configConsole);
            configMirror = GetPreferences("Config", "Mirror", configMirror);
            configMirrorPath = GetPreferences("Config", "MirrorPath", configMirrorPath);
            configAutoScroll = GetPreferences("Config", "AutoScroll", configAutoScroll);
            configMaxLines = GetPreferences("Config", "MaxLines", configMaxLines);
            configToggleKey = GetPreferences("Config", "Toggle", configToggleKey);
            configCollision = GetPreferences("Config", "Collision", configCollision);

            configToggleKey = configToggleKey.ToLower();

            consoleMirror = new ConsoleMirror(configMirror ? configMirrorPath : "", callback);
            Application.RegisterLogCallback(new Application.LogCallback(LogCallbackHandler));

            guiWindow.Visible = configVisible;
        }

        void Update()
        {
            if (!string.IsNullOrEmpty(configToggleKey) && Input.GetKeyDown(configToggleKey))
            {
                Toggle();
            }
        }

        void Toggle()
        {
            configVisible = !configVisible;
            guiWindow.Visible = configVisible;
            if (configVisible)
            {
                GearMenu.Buttons.SetFrameColor(gearMenuButton, Color.black);
            }
            else
            {
                GearMenu.Buttons.ResetFrameColor(gearMenuButton);
            }
        }

        void OnGUI()
        {
            if (lines.Count > configMaxLines)
            {
                lines.RemoveRange(0, lines.Count - configMaxLines);
            }

            guiWindow.DrawGui(123456, ConsoleWindow);
        }

        void ConsoleWindow(int windowID, GuiWindow guiWindow)
        {
            if (configAutoScroll)
            {
                if (lines.Count > lastLineCount)
                {
                    lastLineCount = lines.Count;
                    scrollPosition.y = 1024f * 1024f;
                }
            }

            if (logStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
                toggleStyle = new GUIStyle(GUI.skin.GetStyle("Toggle"));
                logStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
                logStyle.margin = new RectOffset(0, 0, 0, 0);
                logStyle.padding = new RectOffset(0, 0, 0, 0);
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            {
                foreach(var line in lines)
                {
                    GUILayout.Label(line, logStyle);
                }
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(clearLabel, buttonStyle))
                {
                    Clear();
                }
                configAutoScroll = GUILayout.Toggle(configAutoScroll, autoScrollLabel, toggleStyle, GUILayout.ExpandWidth(false));
                GUILayout.Space(guiWindow.HandleSize);
            }
            GUILayout.EndHorizontal();
        }

        void Clear()
        {
            lines.Clear();
            lastLineCount = 0;
        }

        void callback(string str)
        {
            lines.Add(str);
            if (configConsole)
            {
                consoleMirror.ConsoleWrite(str);
                consoleMirror.ConsoleWrite("\n");
            }
            if (configMirror)
            {
                consoleMirror.LogFileWrite(str);
            }
        }

        static void LogCallbackHandler(string message, string stackTrace, LogType type)
        {
            Console.ForegroundColor = type == LogType.Error ? ConsoleColor.Red : (type != LogType.Warning ? ConsoleColor.Gray : ConsoleColor.Yellow);
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        string GetPreferences(string section, string key, string defaultValue)
        {
            if (!Preferences.HasSection(section) || !Preferences[section].HasKey(key) || string.IsNullOrEmpty(Preferences[section][key].Value))
            {
                Preferences[section][key].Value = defaultValue;
                SaveConfig();
            }
            return Preferences[section][key].Value;
        }

        bool GetPreferences(string section, string key, bool defaultValue)
        {
            if (!Preferences.HasSection(section) || !Preferences[section].HasKey(key) || string.IsNullOrEmpty(Preferences[section][key].Value))
            {
                Preferences[section][key].Value = defaultValue.ToString();
                SaveConfig();
            }
            bool b = defaultValue;
            bool.TryParse(Preferences[section][key].Value, out b);
            return b;
        }

        int GetPreferences(string section, string key, int defaultValue)
        {
            if (!Preferences.HasSection(section) || !Preferences[section].HasKey(key) || string.IsNullOrEmpty(Preferences[section][key].Value))
            {
                Preferences[section][key].Value = defaultValue.ToString();
                SaveConfig();
            }
            int i = defaultValue;
            int.TryParse(Preferences[section][key].Value, out i);
            return i;
        }
    }

    internal class ConsoleMirror : IDisposable
    {
        TextWriter oldConsoleOut;
        MirrorWriter mirrorWriter;

        FileStream logFileStream;
        StreamWriter logStreamWriter;

        public ConsoleMirror(string logFileName, Action<string> callback)
        {
            try
            {
                oldConsoleOut = Console.Out;
                mirrorWriter = new MirrorWriter(callback);
                Console.SetOut(mirrorWriter);
                if (!string.IsNullOrEmpty(logFileName))
                {
                    logFileStream = File.Open(logFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                    logStreamWriter = new StreamWriter(logFileStream);
                    logStreamWriter.AutoFlush = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void ConsoleWrite(string str)
        {
            oldConsoleOut.Write(str);
        }

        public void LogFileWrite(string str)
        {
            if (logStreamWriter != null)
            {
                logStreamWriter.Write(str);
            }
        }

        public void Dispose()
        {
            Console.SetOut(oldConsoleOut);
            mirrorWriter = null;

            logStreamWriter.Flush();
            logStreamWriter = null;

            logFileStream.Flush();
            logFileStream.Close();
            logFileStream = null;
        }
    }

    internal class MirrorWriter : TextWriter
    {
        MemoryStream memoryStream;
        StreamWriter streamWriter;
        Action<string> callback;

        public override Encoding Encoding { get { return Encoding.Default; } }

        public MirrorWriter(Action<string> callback)
        {
            this.callback = callback;
            memoryStream = new MemoryStream(4096);
            streamWriter = new StreamWriter(memoryStream);
        }

        public override void Write(char value)
        {
            if (value == 0x0a)
            {
                Flush();
            }
            else
            {
                streamWriter.Write(value);
            }
        }

        public override void Flush()
        {
            streamWriter.Flush();
            memoryStream.Position = 0;
            if (callback != null)
            {
                string s = (new StreamReader(memoryStream)).ReadToEnd();
                callback(s);
            }
            memoryStream.SetLength(0);
            streamWriter = new StreamWriter(memoryStream);
        }
    }
}


public class GuiWindow
{
    public delegate void GuiWindowFunction(int windowID, GuiWindow guiWindow);

    public string Title = "GUI Window";

    public Rect WindowRect = new Rect(Screen.width / 3, margin, Screen.width / 3, Screen.height - (margin * 2));
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

internal static class Icon
{
    public static byte[] Png =
    {
        0x89,0x50,0x4e,0x47,0x0d,0x0a,0x1a,0x0a,0x00,0x00,0x00,0x0d,0x49,0x48,0x44,0x52,
        0x00,0x00,0x00,0x20,0x00,0x00,0x00,0x20,0x08,0x02,0x00,0x00,0x00,0xfc,0x18,0xed,
        0xa3,0x00,0x00,0x00,0x03,0x73,0x42,0x49,0x54,0x08,0x08,0x08,0xdb,0xe1,0x4f,0xe0,
        0x00,0x00,0x00,0x09,0x70,0x48,0x59,0x73,0x00,0x00,0x16,0x25,0x00,0x00,0x16,0x25,
        0x01,0x49,0x52,0x24,0xf0,0x00,0x00,0x01,0xd9,0x49,0x44,0x41,0x54,0x48,0x89,0xd5,
        0x96,0x4d,0x8a,0xc2,0x30,0x14,0xc7,0x53,0xad,0xc5,0x54,0x34,0x94,0xea,0x4e,0x48,
        0x57,0x22,0x5e,0xc0,0x9b,0x08,0x9e,0x40,0x70,0x2f,0x78,0x04,0xc1,0x83,0x08,0x73,
        0x0b,0x2f,0xa1,0x20,0x52,0x70,0xeb,0x07,0xfd,0x80,0x54,0x52,0x6c,0x9c,0x45,0x4a,
        0xd4,0x1a,0xab,0xd6,0xe9,0x0c,0xf3,0xdf,0xa4,0x79,0x4d,0xdf,0x2f,0x2f,0x79,0x2f,
        0x8d,0x72,0x3e,0x9f,0x41,0x9e,0x2a,0xe4,0xea,0x1d,0x00,0xa0,0xf2,0x26,0x0c,0x43,
        0x42,0x88,0xe3,0x38,0xae,0xeb,0x52,0x4a,0x01,0x00,0x19,0x22,0x2b,0x16,0x8b,0x10,
        0x42,0x84,0x90,0x61,0x18,0xba,0xae,0xab,0xaa,0x7a,0x01,0x10,0x42,0x6c,0xdb,0x5e,
        0xad,0x56,0x51,0x14,0x9d,0x4e,0xa7,0x6c,0x93,0x2d,0x14,0x0a,0x00,0x80,0x72,0xb9,
        0xdc,0x6e,0xb7,0x2d,0xcb,0xaa,0xd5,0x6a,0x17,0xc0,0x6e,0xb7,0x5b,0xaf,0xd7,0xdd,
        0x6e,0xd7,0xb2,0x2c,0x3e,0x2e,0x9b,0xc2,0x30,0x5c,0x2c,0x16,0xcb,0xe5,0x12,0x21,
        0xc4,0x01,0xb9,0xef,0x41,0x0c,0x38,0x1c,0x0e,0xd5,0x6a,0xf5,0xc3,0xe9,0x03,0x00,
        0x34,0x4d,0xeb,0x74,0x3a,0x94,0x52,0xcf,0xf3,0x6e,0x00,0x41,0x10,0xa8,0xaa,0xfa,
        0xa1,0x77,0xc1,0x60,0x8c,0x1d,0x8f,0xc7,0x1b,0x40,0x14,0x45,0x62,0xc4,0x74,0x32,
        0x99,0x4e,0x26,0x1f,0x62,0x84,0xc3,0xdf,0xaa,0x83,0x57,0xe4,0xfb,0xbe,0x78,0xe6,
        0x19,0xf2,0xc8,0x98,0x05,0xb0,0xd9,0x6c,0xbe,0x66,0x33,0xd1,0xed,0xf5,0xfb,0x18,
        0x63,0xa9,0x31,0xf1,0xe1,0xab,0x4b,0x74,0xed,0x48,0x74,0xa5,0xc6,0x8c,0x00,0xae,
        0xd1,0x78,0x3c,0x1a,0x8f,0x13,0xc6,0xc1,0x70,0x78,0x6f,0xcc,0x08,0x90,0x4a,0xba,
        0xf4,0x3f,0x09,0x48,0xd7,0x3f,0x01,0x5c,0x27,0x6b,0x42,0x0f,0xd3,0xf4,0xba,0x98,
        0xc5,0x1e,0x4a,0x2b,0x3c,0xbd,0xec,0x5f,0x8d,0xa0,0xd7,0xef,0xdf,0x77,0x13,0x46,
        0xa9,0x24,0x11,0x48,0x73,0x0e,0x63,0x3c,0x18,0x0e,0x45,0x97,0x67,0x0e,0xc6,0x38,
        0x3d,0x38,0x39,0xe0,0x91,0xd2,0xd3,0xf1,0x91,0xfe,0xf4,0xb0,0x63,0x8c,0x3d,0xfd,
        0x43,0x3c,0x3d,0xd8,0xe3,0xef,0x15,0x45,0x91,0xbc,0x7b,0xf3,0xff,0x73,0xbd,0xe7,
        0xc2,0x61,0x1c,0x01,0x84,0x30,0x08,0x82,0x57,0xa6,0x9c,0xd0,0x7d,0x46,0x70,0x27,
        0x10,0xc2,0x78,0x96,0x6f,0xb9,0xbb,0xd7,0xd3,0x3b,0x4e,0x0c,0x30,0x4d,0xd3,0x75,
        0xdd,0xed,0x76,0xcb,0x18,0x7b,0x0b,0xc0,0x6f,0x57,0x42,0x8c,0x31,0xdb,0xb6,0x4b,
        0xa5,0x12,0x42,0x28,0x1e,0xc0,0x9b,0x7a,0xbd,0xde,0x6c,0x36,0xe7,0xf3,0xb9,0x78,
        0x91,0x4d,0x8c,0x31,0xcf,0xf3,0x5a,0xad,0x96,0x61,0x18,0xdc,0xa2,0xf0,0x2b,0x22,
        0xa5,0xd4,0xf7,0xfd,0xfd,0x7e,0xef,0x38,0x0e,0x21,0x04,0x64,0xbd,0x3a,0xea,0xba,
        0x6e,0x9a,0x66,0xa3,0xd1,0xa8,0x54,0x2a,0x9a,0xa6,0x5d,0x00,0xf9,0x29,0xf7,0x42,
        0xfb,0x06,0xbf,0x9b,0xc4,0x29,0x55,0x89,0x17,0xb0,0x00,0x00,0x00,0x00,0x49,0x45,
        0x4e,0x44,0xae,0x42,0x60,0x82,
    };
}

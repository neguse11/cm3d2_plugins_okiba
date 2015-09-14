using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityInjector.Attributes;

namespace CM3D2.LogWindow.Plugin
{
    [PluginFilter("CM3D2x64"),
    PluginFilter("CM3D2x86"),
    PluginFilter("CM3D2VRx64"),
    PluginName("CM3D2 LogWindow"),
    PluginVersion("0.1.2.0")]
    public class LogWindow : UnityInjector.PluginBase
    {
        ConsoleMirror consoleMirror;
        List<string> lines = new List<string>();

        string configToggleKey = "f8";
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

            Console.WriteLine("LogWindow");

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
            if (Input.GetKeyDown(configToggleKey))
            {
                configVisible = !configVisible;
                guiWindow.Visible = configVisible;
            }
        }

        void OnGUI()
        {
            if (lines.Count > configMaxLines)
            {
                lines.RemoveRange(0, lines.Count - configMaxLines);
            }

            if (configVisible)
            {
                guiWindow.DrawGui(123456, ConsoleWindow);
            }
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
                for (int i = 0; i < lines.Count; ++i)
                {
                    GUILayout.Label(lines[i], logStyle);
                }
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(clearLabel, buttonStyle))
                {
                    lines.Clear();
                }
                configAutoScroll = GUILayout.Toggle(configAutoScroll, autoScrollLabel, toggleStyle, GUILayout.ExpandWidth(false));
                GUILayout.Space(guiWindow.HandleSize);
            }
            GUILayout.EndHorizontal();
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
    public float MaxWidth = Screen.width - margin * 2f;
    public float MaxHeight = Screen.height - margin * 2f;
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

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityInjector.Attributes;

namespace CM3D2.ConsistentWindowPosition.Plugin
{
    [PluginName("CM3D2 Consistent Window Position"), PluginVersion("0.1.3.0")]
    public class ConsistentWindowPositionPlugin : UnityInjector.PluginBase
    {
        static readonly string windowTitle = "CUSTOM MAID 3D 2";
        static readonly string windowClass = "UnityWndClass";
        static readonly string section = "Config";
        IntPtr hWnd = IntPtr.Zero;
        Config config = new Config();

        void Awake()
        {
            if (IsVr())
            {
                return;
            }
            GameObject.DontDestroyOnLoad(this);
            hWnd = Win32.FindCurrentApplicationWindow(windowTitle, windowClass);
            LoadWindowPosition(hWnd);
        }

        void OnApplicationQuit()
        {
            SaveWindowPosition(hWnd);
        }

        bool LoadWindowPosition(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return false;
            }
            Func<string, string> getValue = (key) =>
            {
                return Preferences[section][key].Value;
            };
            if (!config.GetPreferences(getValue))
            {
                return false;
            }

            AdjustWindowStyles(ref config.style, ref config.exStyle);
            Win32.SetWindowRect(hWnd, config.rect);
            Win32.SetWindowStyle(hWnd, (IntPtr)config.style);
            Win32.SetWindowExStyle(hWnd, (IntPtr)config.exStyle);
            return true;
        }

        bool SaveWindowPosition(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return false;
            }
            config.rect = Win32.GetWindowRect(hWnd);
            config.style = (long)Win32.GetWindowStyle(hWnd);
            config.exStyle = (long)Win32.GetWindowExStyle(hWnd);
            AdjustWindowStyles(ref config.style, ref config.exStyle);

            Action<string, string> setValue = (key, value) =>
            {
                Preferences[section][key].Value = value;
            };
            if (!config.SetPreferences(setValue))
            {
                return false;
            }
            SaveConfig();
            return true;
        }

        static void AdjustWindowStyles(ref long style, ref long exStyle)
        {
            // ウィンドウスタイルに WS_DISABLED が指定されないように補正 (その５>>69)
            style &= ~Win32.WS_DISABLED;
        }

        static bool IsVr()
        {
            string fullFilename = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string filename = Path.GetFileNameWithoutExtension(fullFilename);
            return filename.IndexOf("VRx64", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    internal class Config
    {
        public Win32.RECT rect;
        public long style;
        public long exStyle;

        public Config()
        {
            Clear();
        }

        public void Clear()
        {
            rect = new Win32.RECT();
            style = 0;
            exStyle = 0;
        }

        public bool GetPreferences(Func<string, string> getValue)
        {
            Clear();
            if (!int.TryParse(getValue("rectLeft"), out rect.Left)) { return false; }
            if (!int.TryParse(getValue("rectTop"), out rect.Top)) { return false; }
            if (!int.TryParse(getValue("rectRight"), out rect.Right)) { return false; }
            if (!int.TryParse(getValue("rectBottom"), out rect.Bottom)) { return false; }
            if (!long.TryParse(getValue("style"), out style)) { return false; }
            if (!long.TryParse(getValue("exStyle"), out exStyle)) { return false; }
            return true;
        }

        public bool SetPreferences(Action<string, string> setValue)
        {
            setValue("rectLeft", rect.Left.ToString());
            setValue("rectTop", rect.Top.ToString());
            setValue("rectRight", rect.Right.ToString());
            setValue("rectBottom", rect.Bottom.ToString());
            setValue("style", style.ToString());
            setValue("exStyle", exStyle.ToString());
            return true;
        }
    }

    // http://stackoverflow.com/a/20276701/2132223
    internal static class Win32
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int GetClassName(IntPtr HWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool MoveWindow(IntPtr hWnd, int x, int y, int cx, int cy, bool bRepaint);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr newLongPtr);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr newLongPtr);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentProcessId();

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        public const uint WS_DISABLED = 0x8000000;

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public static IntPtr FindCurrentApplicationWindow(string windowTitle, string windowClass)
        {
            return FindWindowByProcessId(GetCurrentProcessId(), windowTitle, windowClass);
        }

        public static IntPtr FindWindowByProcessId(uint pid, string windowTitle, string windowClass)
        {
            IntPtr result = IntPtr.Zero;
            Win32.EnumWindows((hWnd, param) =>
            {
                uint p;
                GetWindowThreadProcessId(hWnd, out p);
                if (p == pid)
                {
                    string title = Win32.GetWindowText(hWnd);
                    string className = Win32.GetClassName(hWnd);
                    if ((windowTitle == null || title.Contains(windowTitle)) &&
                       (windowClass == null || className == windowClass))
                    {
                        result = hWnd;
                    }
                }
                return true;
            });
            return result;
        }

        public static string GetWindowText(IntPtr hWnd)
        {
            var builder = new StringBuilder(4096);
            GetWindowText(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }

        public static string GetClassName(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return String.Empty;
            }
            var builder = new StringBuilder(1024);
            GetClassName(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }

        public static bool EnumWindows(EnumWindowsProc filter)
        {
            return EnumWindows(filter, IntPtr.Zero);
        }

        public static RECT GetWindowRect(IntPtr hWnd)
        {
            RECT rect;
            if (!GetWindowRect(hWnd, out rect))
            {
                rect = new RECT() { Left = 0, Top = 0, Right = 0, Bottom = 0 };
            }
            return rect;
        }

        public static bool SetWindowRect(IntPtr hWnd, RECT rect)
        {
            return MoveWindow(hWnd, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, true);
        }

        // http://stackoverflow.com/a/3344276/2132223
        public static IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
            {
                return GetWindowLongPtr32(hWnd, nIndex);
            }
            return GetWindowLongPtr64(hWnd, nIndex);
        }

        public static IntPtr GetWindowStyle(IntPtr hWnd)
        {
            return GetWindowLong(hWnd, -16);
        }

        public static IntPtr GetWindowExStyle(IntPtr hWnd)
        {
            return GetWindowLong(hWnd, -20);
        }

        // http://referencesource.microsoft.com/#System/compmod/microsoft/win32/UnsafeNativeMethods.cs,88
        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        public static IntPtr SetWindowStyle(IntPtr hWnd, IntPtr style)
        {
            return SetWindowLong(hWnd, -16, style);
        }

        public static IntPtr SetWindowExStyle(IntPtr hWnd, IntPtr exStyle)
        {
            return SetWindowLong(hWnd, -20, exStyle);
        }
    }
}

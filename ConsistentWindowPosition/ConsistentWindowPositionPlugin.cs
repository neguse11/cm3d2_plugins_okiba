using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace CM3D2.ConsistentWindowPosition.Plugin
{
    [PluginFilter("CM3D2x64"),
    PluginFilter("CM3D2x86"),
    //  PluginFilter("CM3D2VRx64"),
    PluginName("CM3D2 Consistent Window Position"),
    PluginVersion("0.1.0.0")]
    public class ConsistentWindowPositionPlugin : PluginBase
    {
        static readonly string windowTitle = "CUSTOM MAID 3D 2";
        static readonly string windowClass = "UnityWndClass";
        IntPtr hWnd = IntPtr.Zero;

        void Awake()
        {
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
            Win32.RECT rect;
            long style;
            long exStyle;
            if (!GetPreferences("Config", out rect, out style, out exStyle))
            {
                return false;
            }

            Win32.SetWindowRect(hWnd, rect);
            Win32.SetWindowStyle(hWnd, (IntPtr)style);
            Win32.SetWindowExStyle(hWnd, (IntPtr)exStyle);
            return true;
        }

        bool SaveWindowPosition(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return false;
            }
            Win32.RECT rect = Win32.GetWindowRect(hWnd);
            long style = (long)Win32.GetWindowStyle(hWnd);
            long exStyle = (long)Win32.GetWindowExStyle(hWnd);

            if (!SetPreferences("Config", rect, style, exStyle))
            {
                return false;
            }
            SaveConfig();
            return true;
        }

        bool SetPreferences(string section, Win32.RECT rect, long style, long exStyle)
        {
            Preferences[section]["rectLeft"].Value = rect.Left.ToString();
            Preferences[section]["rectTop"].Value = rect.Top.ToString();
            Preferences[section]["rectRight"].Value = rect.Right.ToString();
            Preferences[section]["rectBottom"].Value = rect.Bottom.ToString();
            Preferences[section]["style"].Value = style.ToString();
            Preferences[section]["exStyle"].Value = exStyle.ToString();
            return true;
        }

        bool GetPreferences(string section, out Win32.RECT rect, out long style, out long exStyle)
        {
            rect = new Win32.RECT();
            style = 0;
            exStyle = 0;

            if (!Preferences.HasSection(section))
            {
                return false;
            }
            string[] keys = { "rectLeft", "rectTop", "rectRight", "rectBottom", "style", "exStyle" };
            foreach (string key in keys)
            {
                if (!Preferences[section].HasKey(key))
                {
                    return false;
                }
            }
            if (!int.TryParse(Preferences[section]["rectLeft"].Value, out rect.Left)) { return false; }
            if (!int.TryParse(Preferences[section]["rectTop"].Value, out rect.Top)) { return false; }
            if (!int.TryParse(Preferences[section]["rectRight"].Value, out rect.Right)) { return false; }
            if (!int.TryParse(Preferences[section]["rectBottom"].Value, out rect.Bottom)) { return false; }
            if (!long.TryParse(Preferences[section]["style"].Value, out style)) { return false; }
            if (!long.TryParse(Preferences[section]["exStyle"].Value, out exStyle)) { return false; }
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

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr newLongPtr);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentProcessId();

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

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

        public static IntPtr GetWindowStyle(IntPtr hWnd)
        {
            return GetWindowLongPtr(hWnd, -16);
        }

        public static IntPtr GetWindowExStyle(IntPtr hWnd)
        {
            return GetWindowLongPtr(hWnd, -20);
        }

        public static IntPtr SetWindowStyle(IntPtr hWnd, IntPtr style)
        {
            return SetWindowLongPtr(hWnd, -16, style);
        }

        public static IntPtr SetWindowExStyle(IntPtr hWnd, IntPtr exStyle)
        {
            return SetWindowLongPtr(hWnd, -20, exStyle);
        }
    }
}

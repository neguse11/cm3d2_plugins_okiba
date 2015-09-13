using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

internal static class PluginHelper
{
    static string[] mpnStrings;

#if DEBUG
    public static bool bDebugEnable = true;
#else
    public static bool bDebugEnable = false;
#endif

    public static List<string> debugLines = new List<string>();
    public static int debugLinesMax = 100;
    public static Vector2 debugScrollPosition = new Vector2(0f, 0f);
    const int margin = 20;
    const int windowId = 0x123456;
    public static Rect debugWindowRect = new Rect(margin, margin, Screen.width / 2 - (margin * 2), Screen.height - (margin * 2));

    public static string[] MpnStrings
    {
        get
        {
            var mpnValues = Enum.GetValues(typeof(MPN));
            mpnStrings = new string[mpnValues.Length];
            for (int i = 0, n = mpnValues.Length; i < n; i++)
            {
                MPN mpn = (MPN)mpnValues.GetValue(i);
                mpnStrings[i] = mpn.ToString("G");
            }
            return mpnStrings;
        }
    }

    public static Maid GetMaid(TBody tbody)
    {
        return tbody.maid;
    }

    // AudioSourceMgrを手がかりに、Maidを得る
    public static Maid GetMaid(AudioSourceMgr audioSourceMgr)
    {
        if (audioSourceMgr == null)
        {
            return null;
        }
        CharacterMgr cm = GameMain.Instance.CharacterMgr;
        for (int i = 0, n = cm.GetStockMaidCount(); i < n; i++)
        {
            Maid maid = cm.GetStockMaid(i);
            if (maid.AudioMan == null)
            {
                continue;
            }
            if (object.ReferenceEquals(maid.AudioMan, audioSourceMgr))
            {
                return maid;
            }
        }
        return null;
    }

    // BoneMorph_を手がかりに、Maidを得る
    public static Maid GetMaid(BoneMorph_ boneMorph_)
    {
        if (boneMorph_ == null)
        {
            return null;
        }
        CharacterMgr cm = GameMain.Instance.CharacterMgr;
        for (int i = 0, n = cm.GetStockMaidCount(); i < n; i++)
        {
            Maid maid = cm.GetStockMaid(i);
            if (maid.body0 == null || maid.body0.bonemorph == null)
            {
                continue;
            }
            if (object.ReferenceEquals(maid.body0.bonemorph, boneMorph_))
            {
                return maid;
            }
        }
        return null;
    }

    // BoneMorph_.SetScaleを呼び出す
    public static void BoneMorphSetScale(string tag, string bname, float x, float y, float z, float x2, float y2, float z2)
    {
        // class BoneMorph { private static void SetScale(string, string, float, float, float, float, float, float); }
        MethodInfo methodInfo = typeof(BoneMorph).GetMethod(
            "SetScale",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            new Type[] { typeof(string), typeof(string), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float) },
            null
        );
        methodInfo.Invoke(null, new object[] { tag, bname, x, y, z, x2, y2, z2 });
    }

    public static void DebugGui()
    {
        if (bDebugEnable && debugLines != null)
        {
            debugWindowRect = GUILayout.Window(windowId, debugWindowRect, DebugGuiWindow, "Debug");
        }
    }

    public static void DebugGuiWindow(int windowId)
    {
        debugScrollPosition = GUILayout.BeginScrollView(debugScrollPosition);
        foreach (string line in debugLines)
        {
            GUILayout.Label(line);
        }
        GUILayout.EndScrollView();
    }

    public static void DebugClear()
    {
        debugLines = new List<string>();
    }

    public static void Debug(string s)
    {
        if (!bDebugEnable)
        {
            return;
        }
        if (debugLines.Count > debugLinesMax)
        {
            return;
        }
        debugLines.Add(s);
    }

    public static void Debug(string format, params object[] args)
    {
        if (!bDebugEnable)
        {
            return;
        }
        Debug(string.Format(format, args));
    }

    public static float NormalizeAngle(float angle)
    {
        if (angle >= 180.0f)
        {
            angle -= 360.0f;
        }
        else if (angle < -180.0f)
        {
            angle += 360.0f;
        }
        return angle;
    }

    public static Vector3 NormalizeEulerAngles(Vector3 eulerAngles)
    {
        return new Vector3(
            NormalizeAngle(eulerAngles.x),
            NormalizeAngle(eulerAngles.y),
            NormalizeAngle(eulerAngles.z));
    }

    public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
    {
        T t = null;
        if (gameObject != null)
        {
            t = gameObject.GetComponent<T>();
            if (t == null)
            {
                t = (T)gameObject.AddComponent<T>();
            }
        }
        return t;
    }
}

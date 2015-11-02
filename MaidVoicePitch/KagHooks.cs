using CM3D2.ExternalSaveData.Managed;
using script;
using System;
using System.Reflection;

internal static class KagHooks
{
    static bool bKagTagPropSetHooked = false;
    static string PluginName;
    static MethodInfo GetWaitEventListMethodInfo;
    delegate bool TagProcDelegate(BaseKagManager baseKagManager, KagTagSupport tag_data);

    public static void SetHook(string PluginName_, bool bForceSet)
    {
        PluginName = PluginName_;
        if (!bForceSet && bKagTagPropSetHooked)
        {
            return;
        }
        bKagTagPropSetHooked = true;
        HookTagCallback("propset", TagPropSet);
        HookTagCallback("faceblend", TagFaceBlend);
        HookTagCallback("face", TagFace);
        HookTagCallback("eyetocamera", TagEyeToCamera);
    }

    static void HookTagCallback(string tagName, TagProcDelegate tagProcDelegate)
    {
        foreach (var kv in GameMain.Instance.ScriptMgr.kag_mot_dic)
        {
            BaseKagManager mgr = kv.Value;
            KagScript kag = mgr.kag;
            kag.RemoveTagCallBack(tagName);
            kag.AddTagCallBack(tagName, new KagScript.KagTagCallBack(delegate (KagTagSupport tag_data)
            {
                return tagProcDelegate(mgr, tag_data);
            }));
        }

        {
            BaseKagManager mgr = GameMain.Instance.ScriptMgr.adv_kag;
            KagScript kag = mgr.kag;
            kag.RemoveTagCallBack(tagName);
            kag.AddTagCallBack(tagName, new KagScript.KagTagCallBack(delegate (KagTagSupport tag_data)
            {
                return tagProcDelegate(mgr, tag_data);
            }));
        }

        {
            BaseKagManager mgr = GameMain.Instance.ScriptMgr.yotogi_kag;
            KagScript kag = mgr.kag;
            kag.RemoveTagCallBack(tagName);
            kag.AddTagCallBack(tagName, new KagScript.KagTagCallBack(delegate (KagTagSupport tag_data)
            {
                return tagProcDelegate(mgr, tag_data);
            }));
        }
    }

    static bool TagPropSet(BaseKagManager baseKagManager, KagTagSupport tag_data)
    {
        Maid maidAndMan = baseKagManager.GetMaidAndMan(tag_data);
        if (maidAndMan != null && ExSaveData.GetBool(maidAndMan, PluginName, "PROPSET_OFF", false))
        {
            string str = tag_data.GetTagProperty("category").AsString();
            if (Array.IndexOf(PluginHelper.MpnStrings, str) >= 0)
            {
#if DEBUG
                Console.WriteLine("PROPSET_OFF(category={0}) -> match", str);
                Helper.Log("PROPSET_OFF(category={0}) -> match", str);
#endif
                return false;
            }
        }

        return baseKagManager.TagPropSet(tag_data);
    }

    static bool TagEyeToCamera(BaseKagManager baseKagManager, KagTagSupport tag_data)
    {
        Maid maidAndMan = baseKagManager.GetMaidAndMan(tag_data);
        if (maidAndMan != null && ExSaveData.GetBool(maidAndMan, PluginName, "EYETOCAMERA_OFF", false))
        {
            return false;
        }
        return baseKagManager.TagEyeToCamera(tag_data);
    }

    static bool TagFace(BaseKagManager baseKagManager, KagTagSupport tag_data)
    {
        Maid maidAndMan = baseKagManager.GetMaidAndMan(tag_data);
        if (maidAndMan == null)
        {
            return false;
        }
        if (maidAndMan != null && ExSaveData.GetBool(maidAndMan, PluginName, "FACE_OFF", false))
        {
            // Helper.Log("FACE_OFF() -> match");
            return false;
        }

        baseKagManager.CheckAbsolutelyNecessaryTag(tag_data, "face", new string[] { "name" });

        string oldName = tag_data.GetTagProperty("name").AsString();
        string newName = FaceScriptTemplates.ProcFaceName(maidAndMan, PluginName, oldName);
        // Helper.Log("TagFace({0})->({1})", oldName, newName);

        WaitEventList waitEventList = GetWaitEventList(baseKagManager, "face");
        int num = 0;
        if (tag_data.IsValid("wait"))
        {
            num = tag_data.GetTagProperty("wait").AsInteger();
        }
        if (num > 0)
        {
            waitEventList.Add(() =>
            {
                if (maidAndMan != null && maidAndMan.body0 != null && maidAndMan.body0.isLoadedBody)
                {
                    maidAndMan.FaceAnime(newName, 1f, 0);
                }
            }, num);
        }
        else
        {
            maidAndMan.FaceAnime(newName, 1f, 0);
            waitEventList.Clear();
        }
        return false;
    }

    static bool TagFaceBlend(BaseKagManager baseKagManager, KagTagSupport tag_data)
    {
        Maid maidAndMan = baseKagManager.GetMaidAndMan(tag_data);
        if (maidAndMan == null)
        {
            return false;
        }
        if (ExSaveData.GetBool(maidAndMan, PluginName, "FACEBLEND_OFF", false))
        {
            // Helper.Log("FACEBLEND_OFF() -> match");
            return false;
        }

        baseKagManager.CheckAbsolutelyNecessaryTag(tag_data, "faceblend", new string[] { "name" });

        string oldName = tag_data.GetTagProperty("name").AsString();
        if (oldName == "なし")
        {
            oldName = "無し";
        }
        string newName = FaceScriptTemplates.ProcFaceBlendName(maidAndMan, PluginName, oldName);
        // Helper.Log("TagFaceBlend({0})->({1})", oldName, newName);

        maidAndMan.FaceBlend(newName);
        return false;
    }

    static WaitEventList GetWaitEventList(BaseKagManager baseKagManager, string list_name)
    {
        // class BaseKagManager protected WaitEventList GetWaitEventList(string list_name)
        MethodInfo methodInfo = GetWaitEventListMethodInfo;
        if (methodInfo == null)
        {
            methodInfo = typeof(BaseKagManager).GetMethod(
                "GetWaitEventList",
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(string) },
                null
            );
            GetWaitEventListMethodInfo = methodInfo;
        }
        if (methodInfo == null)
        {
            return null;
        }
        object obj = methodInfo.Invoke(baseKagManager, new object[] { list_name });
        return obj as WaitEventList;
    }
}

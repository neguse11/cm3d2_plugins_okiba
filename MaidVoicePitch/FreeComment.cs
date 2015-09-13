using CM3D2.ExternalSaveData.Managed;
using System;
using System.Text.RegularExpressions;

internal static class FreeComment
{
    public static void FreeCommentToSetting(string PluginName, bool overwrite)
    {
        CharacterMgr cm = GameMain.Instance.CharacterMgr;
        for (int i = 0, n = cm.GetStockMaidCount(); i < n; i++)
        {
            Maid maid = cm.GetStockMaid(i);

            try
            {
                FreeCommentToSetting(maid, PluginName, overwrite);
            }
            catch (Exception e)
            {
                Helper.ShowException(e);
            }
        }
    }

    public static void FreeCommentToSetting(Maid maid, string PluginName, bool overwrite)
    {
        if (maid == null || maid.Param == null || maid.Param.status == null)
        {
            return;
        }
        string freeComment = maid.Param.status.free_comment;

        Func<string, bool> Contains = (str) =>
        {
            if (freeComment == null)
            {
                return false;
            }
            return freeComment.Contains(str);
        };

        Func<string, string> String = (pattern) =>
        {
            if (freeComment != null)
            {
                Match m = Regex.Match(freeComment, pattern);
                if (m.Groups.Count >= 2)
                {
                    return m.Groups[1].Value;
                }
            }
            return null;
        };

        Func<int, float, string, float> Float = (index, defaultValue, pattern) =>
        {
            if (freeComment != null)
            {
                Match m = Regex.Match(freeComment, pattern);
                if (m.Groups.Count >= 2 + index)
                {
                    return Helper.StringToFloat(m.Groups[1 + index].Value, defaultValue);
                }
            }
            return defaultValue;
        };

        string n = PluginName;

        //

        ExSaveData.SetBool(maid, n, "WIDESLIDER", Contains("#WIDESLIDER#") || Contains("#TEST_WIDE_SLIDER#"), overwrite);
        ExSaveData.SetBool(maid, n, "PROPSET_OFF", Contains("#PROPSET_OFF#") || Contains("#TEST_PROPSET_OFF#"), overwrite);
        ExSaveData.SetFloat(maid, n, "EYETOCAM", Contains("#EYETOCAM#") ? 1f : 0f, overwrite);
        ExSaveData.SetFloat(maid, n, "HEADTOCAM", Contains("#HEADTOCAM#") ? 1f : 0f, overwrite);
        ExSaveData.SetBool(maid, n, "LIPSYNC_OFF", Contains("#LIPSYNC_OFF#"), overwrite);
        ExSaveData.SetBool(maid, n, "HYOUJOU_OFF", Contains("#HYOUJOU_OFF#"), overwrite);
        ExSaveData.SetBool(maid, n, "EYETOCAMERA_OFF", Contains("#TEST_EYETOCAMERA_OFF#"), overwrite);
        ExSaveData.SetBool(maid, n, "MUHYOU", Contains("#MUHYOU#"), overwrite);
        ExSaveData.Set(maid, n, "FACE_SCRIPT_TEMPLATE", String(@"#TEST_FACE_SCRIPT_TEMPLATE=([^#]*)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "FACE_ANIME_SPEED", Float(0, 1f, @"#TEST_FACE_ANIME_SPEED=([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.Set(maid, n, "SLIDER_TEMPLATE", String(@"#TEST_SLIDER_TEMPLATE=([^#]*)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "PITCH", Float(0, 0f, @"#PITCH=([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "LIPSYNC", Float(0, 1f, @"#TEST_LIPSYNC=([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "EYEBALL.width", Float(0, 1f, @"#EYEBALL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "EYEBALL.height", Float(1, 1f, @"#EYEBALL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "EYE_RATIO", Float(0, 1f, @"#TEST_EYE_RATIO=([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "MABATAKI", Float(0, 1f, @"#MABATAKI=([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetBool(maid, n, "MABATAKI.pause", Contains("#MABATAKI_PAUSE#"), overwrite);
        //          ExSaveData.SetFloat(maid, n, "MABATAKI.min",            Float(0, 0f, @"#MABATAKI=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        //          ExSaveData.SetFloat(maid, n, "MABATAKI.max",            Float(1, 1f, @"#MABATAKI=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "EYE_ANG.angle", Float(0, 0f, @"#TEST_EYE_ANG=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "EYE_ANG.x", Float(1, 0f, @"#TEST_EYE_ANG=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "EYE_ANG.y", Float(2, 0f, @"#TEST_EYE_ANG=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "PELSCL.width", Float(0, 1f, @"#TEST_PELSCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "PELSCL.depth", Float(1, 1f, @"#TEST_PELSCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "PELSCL.height", Float(2, 1f, @"#TEST_PELSCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "THISCL.width", Float(0, 1f, @"#TEST_THISCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "THISCL.depth", Float(1, 1f, @"#TEST_THISCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "THIPOS.x", Float(0, 0f, @"#TEST_THIPOS=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "THIPOS.z", Float(1, 0f, @"#TEST_THIPOS=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetBool(maid, n, "PELVIS", Contains("#TEST_PELVIS_ENABLE#"), overwrite);
        ExSaveData.SetFloat(maid, n, "PELVIS.x", Float(0, 0f, @"#TEST_PELVIS=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "PELVIS.y", Float(1, 0f, @"#TEST_PELVIS=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "PELVIS.z", Float(2, 0f, @"#TEST_PELVIS=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);

        ExSaveData.SetBool(maid, n, "FARMFIX", Contains("#FARMFIX##"), overwrite);
        ExSaveData.SetBool(maid, n, "SPISCL", Contains("#SPISCL"), overwrite);
        ExSaveData.SetFloat(maid, n, "SPISCL.width", Float(0, 1f, @"#SPISCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "SPISCL.depth", Float(1, 1f, @"#SPISCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "SPISCL.height", Float(2, 1f, @"#SPISCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetBool(maid, n, "S0ASCL", Contains("#S0ASCL"), overwrite);
        ExSaveData.SetFloat(maid, n, "S0ASCL.width", Float(0, 1f, @"#S0ASCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "S0ASCL.depth", Float(1, 1f, @"#S0ASCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "S0ASCL.height", Float(2, 1f, @"#S0ASCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetBool(maid, n, "S1_SCL", Contains("#S1_SCL"), overwrite);
        ExSaveData.SetFloat(maid, n, "S1_SCL.width", Float(0, 1f, @"#S1_SCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "S1_SCL.depth", Float(1, 1f, @"#S1_SCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "S1_SCL.height", Float(2, 1f, @"#S1_SCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetBool(maid, n, "S1ASCL", Contains("#S1ASCL"), overwrite);
        ExSaveData.SetFloat(maid, n, "S1ASCL.width", Float(0, 1f, @"#S1ASCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "S1ASCL.depth", Float(1, 1f, @"#S1ASCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
        ExSaveData.SetFloat(maid, n, "S1ASCL.height", Float(2, 1f, @"#S1ASCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#"), overwrite);
    }
}

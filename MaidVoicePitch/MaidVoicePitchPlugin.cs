using CM3D2.ExternalSaveData.Managed;
using script;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace CM3D2.MaidVoicePitch.Plugin
{
    [PluginFilter("CM3D2x64"),
    PluginFilter("CM3D2x86"),
    PluginFilter("CM3D2VRx64"),
    PluginName("CM3D2 MaidVoicePitch"),
    PluginVersion("0.2.4.0")]
    public class MaidVoicePitch : PluginBase
    {
        public bool bEditScene = false;
        public bool bKagTagPropSetHooked = false;
        FaceScriptTemplateCache faceScriptTemplates = new FaceScriptTemplateCache();
        SliderTemplateCache sliderTemplates = new SliderTemplateCache();
        bool bDeserialized = false;

        string PluginName { get { return "CM3D2.MaidVoicePitch"; } }

        delegate bool TagCallbackDelegate(KagTagSupport tag_data);
        delegate bool TagProcDelegate(string kagManagerTypeName, Type kagManagerType, BaseKagManager baseKagManager, KagTagSupport tag_data);

        public void Awake()
        {
            UnityEngine.GameObject.DontDestroyOnLoad(this);
            FlushTemplatesCache();
            TestPropSet();
            LowerBodyScaleAwake();
            ForeArmFixAwake();
        }

        public void OnLevelWasLoaded(int level)
        {
            bKagTagPropSetHooked = false;
            if (level == 5)
            {
                bEditScene = true;
            }
            CM3D2.ExternalSaveData.Managed.GameMainCallbacks.Deserialize.Callbacks[PluginName] = deserializeCallback;
            if (bDeserialized)
            {
                Cleanup();
                FreeCommentToSetting(false);
                bDeserialized = false;
            }
            TestPropSet();
        }

        void deserializeCallback(GameMain that, int f_nSaveNo)
        {
            bDeserialized = true;
        }

        // 存在しないMaidや設定を削除する
        void Cleanup()
        {
            List<string> guids = new List<string>();
            CharacterMgr cm = GameMain.Instance.CharacterMgr;
            for (int i = 0, n = cm.GetStockMaidCount(); i < n; i++)
            {
                Maid maid = cm.GetStockMaid(i);
                guids.Add(maid.Param.status.guid);
            }
            ExSaveData.CleanupMaids(guids);

            CleanupSettings();
        }

        public void CleanupSettings()
        {
            {
                string[] obsoleteSettings = {
                    "WIDESLIDER.enable", "PROPSET_OFF.enable", "LIPSYNC_OFF.enable",
                    "HYOUJOU_OFF.enable", "EYETOCAMERA_OFF.enable", "MUHYOU.enable",
                    "FARMFIX.enable", "EYEBALL.enable", "EYE_ANG.enable",
                    "PELSCL.enable", "SKTSCL.enable", "THISCL.enable", "THIPOS.enable",
                    "PELVIS.enable", "FARMFIX.enable", "SPISCL.enable",
                    "S0ASCL.enable", "S1_SCL.enable", "S1ASCL.enable",
                    "FACE_OFF.enable", "FACEBLEND_OFF.enable"
                };
                CharacterMgr cm = GameMain.Instance.CharacterMgr;
                for (int i = 0, n = cm.GetStockMaidCount(); i < n; i++)
                {
                    Maid maid = cm.GetStockMaid(i);
                    foreach (string s in obsoleteSettings)
                    {
                        ExSaveData.Remove(maid, PluginName, s);
                    }
                }
            }

            {
                string[] obsoleteGlobalSettings = {
                    "TEST_GLOBAL_KEY"
                };
                foreach (string s in obsoleteGlobalSettings)
                {
                    ExSaveData.GlobalRemove(PluginName, s);
                }
            }
        }

        void FreeCommentToSetting(bool overwrite)
        {
            CharacterMgr cm = GameMain.Instance.CharacterMgr;
            for (int i = 0, n = cm.GetStockMaidCount(); i < n; i++)
            {
                Maid maid = cm.GetStockMaid(i);

                try
                {
                    FreeCommentToSetting(maid, overwrite);
                }
                catch (Exception e)
                {
                    Helper.ShowException(e);
                }
            }
        }

        void FreeCommentToSetting(Maid maid, bool overwrite)
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
                        return Helper.FloatTryParse(m.Groups[1 + index].Value, defaultValue);
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

        public void Update()
        {
            // テンプレートキャッシュを消去して、再読み込みを促す
            if (Input.GetKey(KeyCode.F12))
            {
                FlushTemplatesCache();
            }

            if (CharacterMgr.EditModeLookHaveItem)
            {
                MaidsUpdate(true);  // エディット画面にいる場合のみ、アップデートを行う
            }
        }

        public void LateUpdate()
        {
            MaidsUpdate(false);
        }

        void MaidsUpdate(bool bEditUpdate)
        {
            if (GameMain.Instance == null || GameMain.Instance.CharacterMgr == null)
            {
                return;
            }
            CharacterMgr cm = GameMain.Instance.CharacterMgr;
            for (int i = 0, n = cm.GetStockMaidCount(); i < n; i++)
            {
                MaidUpdate(cm.GetStockMaid(i), bEditUpdate);
            }
        }

        void MaidUpdate(Maid maid, bool bEditUpdate)
        {
            if (maid == null)
            {
                return;
            }

            if (!maid.Visible)
            {
                return;
            }

            // フリーコメントから設定を読み込む
            if (Input.GetKey(KeyCode.F4))
            {
                FreeCommentToSetting(maid, true);
            }

            bool bMuhyou = ExSaveData.GetBool(maid, PluginName, "MUHYOU", false);
            bool bLipSyncOff = ExSaveData.GetBool(maid, PluginName, "LIPSYNC_OFF", false);
            bool bHyoujouOff = ExSaveData.GetBool(maid, PluginName, "HYOUJOU_OFF", false);

            // リップシンク(口パク)しない
            if (bLipSyncOff || bMuhyou)
            {
                Helper.SetInstanceField(typeof(Maid), maid, "m_bFoceKuchipakuSelfUpdateTime", true);
            }
            else
            {
                if (bEditUpdate)
                {
                    Helper.SetInstanceField(typeof(Maid), maid, "m_bFoceKuchipakuSelfUpdateTime", false);
                }
            }

            if (bMuhyou)
            {
                Helper.SetInstanceField(typeof(Maid), maid, "MabatakiVal", 1f);
            }

            // 目と口の表情変化をやめる
            if (bHyoujouOff || bMuhyou)
            {
                maid.FaceAnime("", 0f, 0);
            }

            // 目を常時カメラに向ける
            //  todo    状態は３つ。true=常に向ける、false=常に向けない、default=何も操作せず、スクリプト等にまかせる
            if (maid.body0 != null)
            {
                float fEyeToCam = ExSaveData.GetFloat(maid, PluginName, "EYETOCAM", 0f);
                if (fEyeToCam < -0.5f)
                {
                    maid.body0.boEyeToCam = false;
                }
                else if (fEyeToCam > 0.5f)
                {
                    maid.body0.boEyeToCam = true;
                }
            }

            // 顔を常時カメラに向ける
            //  todo    状態は３つ。true=常に向ける、false=常に向けない、default=何も操作せず、スクリプト等にまかせる
            if (maid.body0 != null)
            {
                float fHeadToCam = ExSaveData.GetFloat(maid, PluginName, "HEADTOCAM", 0f);
                if (fHeadToCam < -0.5f)
                {
                    maid.body0.boHeadToCam = false;
                }
                else if (fHeadToCam > 0.5f)
                {
                    maid.body0.boHeadToCam = true;
                }
            }

            bool bSlider = ExSaveData.GetBool(maid, PluginName, "WIDESLIDER", false);

            TBody tbody = maid.body0;
            Pitch(maid);
            Mabataki(maid);
            WideSlider(maid);
            EyeBall(maid);
            if (bSlider)
            {
                LowerBodyScale(maid);
                EyeScaleRotate(maid);
                SpineScale(maid);
                ForeArmFix(maid);
            }
            TestMabatakiSpeed(maid, bEditUpdate);
            TestPelvis(maid);
            TestLipSync(maid);
            TestSliderTemplate(maid);
        }

        // キャッシュ状態更新
        void FlushTemplatesCache()
        {
            faceScriptTemplates.Clear();// = new Dictionary<string, FaceScriptTemplate>();
            sliderTemplates.Clear();// = new Dictionary<string, SliderTemplate>();
        }

        // ピッチ変更
        void Pitch(Maid maid)
        {
            if (maid.AudioMan == null || maid.AudioMan.audiosource == null || !maid.AudioMan.audiosource.isPlaying)
            {
                return;
            }
            float f = ExSaveData.GetFloat(maid, PluginName, "PITCH", 0f);
            maid.AudioMan.audiosource.pitch = 1f + f;
        }

        // まばたき制限
        void Mabataki(Maid maid)
        {
            float mabatakiVal = (float)Helper.GetInstanceField(typeof(Maid), maid, "MabatakiVal");
            float f = Mathf.Clamp01(1f - ExSaveData.GetFloat(maid, PluginName, "MABATAKI", 1f));
            float mMin = Mathf.Asin(f);
            float mMax = (float)Math.PI - mMin;
            mMin = Mathf.Pow(mMin / (float)Math.PI, 0.5f);
            mMax = Mathf.Pow(mMax / (float)Math.PI, 0.5f);
            mabatakiVal = Mathf.Clamp(mabatakiVal, mMin, mMax);

            /*
                        // 旧まばたき (そのうち廃止)
                        {
                            float mMin = GetExternalSaveDataFloat(maid, PluginName, "MABATAKI.min", 0f);
                            float mMax = GetExternalSaveDataFloat(maid, PluginName, "MABATAKI.max", 1f);
                            mabatakiVal = Mathf.Clamp(mabatakiVal, mMin, mMax);
                        }
            */
            Helper.SetInstanceField(typeof(Maid), maid, "MabatakiVal", mabatakiVal);
        }

        // まばたき速度変更
        void TestMabatakiSpeed(Maid maid, bool bEditUpdate)
        {
            float mabatakiVal = (float)Helper.GetInstanceField(typeof(Maid), maid, "MabatakiVal");
            if (mabatakiVal != 1f && mabatakiVal > 0.1f && !bEditUpdate)
            {
                float f = Mathf.Clamp(ExSaveData.GetFloat(maid, PluginName, "MABATAKI_SPEED", 1f), 0f, 10f);
                mabatakiVal += Time.deltaTime * 2f;
                mabatakiVal -= Time.deltaTime * 2f * f;
            }
            Helper.SetInstanceField(typeof(Maid), maid, "MabatakiVal", mabatakiVal);
        }

        // 瞳サイズ変更
        void EyeBall(Maid maid)
        {
            TBody tbody = maid.body0;
            if (tbody != null && tbody.trsEyeL != null && tbody.trsEyeR != null)
            {
                float w = ExSaveData.GetFloat(maid, PluginName, "EYEBALL.width", 1f);
                float h = ExSaveData.GetFloat(maid, PluginName, "EYEBALL.height", 1f);
                tbody.trsEyeL.localScale = new Vector3(1f, h, w);
                tbody.trsEyeR.localScale = new Vector3(1f, h, w);
            }
        }

        // 目のサイズ・角度変更
        // EyeScaleRotate : 目のサイズと角度変更する CM3D.MaidVoicePich.Plugin.cs の追加メソッド
        // http://pastebin.com/DBuN5Sws
        // その１>>923
        // http://jbbs.shitaraba.net/bbs/read.cgi/game/55179/1438196715/923
        void EyeScaleRotate(Maid maid)
        {
            Transform tL = null;
            Transform tR = null;

            for (int i = 0; i < maid.body0.bonemorph.bones.Count; i++)
            {
                if (maid.body0.bonemorph.bones[i].linkT == null) continue;
                if (maid.body0.bonemorph.bones[i].linkT.name == "Eyepos_L") tL = maid.body0.bonemorph.bones[i].linkT;
                if (maid.body0.bonemorph.bones[i].linkT.name == "Eyepos_R") tR = maid.body0.bonemorph.bones[i].linkT;
            }
            if (tL == null || tR == null) return;

            {
                float aspectRatioMin = 0.1f;
                float aspectRatioMax = 10f;
                float aspectRatio = Mathf.Clamp(ExSaveData.GetFloat(maid, PluginName, "EYE_RATIO", 1f), aspectRatioMin, aspectRatioMax);

                float aspectW = 1f;
                float aspectH = 1f;
                if (aspectRatio >= 1f)
                {
                    // 1以上の場合、横幅は固定で、高さを小さくする
                    aspectW = 1f;
                    aspectH = 1f / aspectRatio;
                }
                else
                {
                    // 1未満の場合、高さは固定で、横幅を小さくする
                    aspectW = 1f * aspectRatio;
                    aspectH = 1f;
                }
                Vector3 sclrvs = new Vector3(1.00f, aspectH, aspectW);
                tL.localScale = Vector3.Scale(tL.localScale, sclrvs);
                tR.localScale = Vector3.Scale(tR.localScale, sclrvs);
            }

            {
                float ra = ExSaveData.GetFloat(maid, PluginName, "EYE_ANG.angle", 0f);
                float rx = ExSaveData.GetFloat(maid, PluginName, "EYE_ANG.x", 0f) / 1000f;
                float ry = ExSaveData.GetFloat(maid, PluginName, "EYE_ANG.y", 0f) / 1000f;

                rx += -9f / 1000f;
                ry += -17f / 1000f;

                {
                    //  プリティフェイス左目tL  rx =  -9.0, ry = -17.0
                    //  初代左目tL              rx = -13.0, ry = -25.0
                    Transform tLparent = tL.parent;
                    Vector3 localCenter = tL.localPosition + (new Vector3(0f, ry, rx)); // ローカル座標系での回転中心位置
                    Vector3 worldCenter = tLparent.TransformPoint(localCenter);         // ワールド座標系での回転中心位置
                    Vector3 localAxis = new Vector3(-1f, 0f, 0f);                       // ローカル座標系での回転軸
                    Vector3 worldAxis = tL.TransformDirection(localAxis);               // ワールド座標系での回転軸

                    tL.localRotation = new Quaternion(-0.00560432f, -0.001345155f, 0.06805823f, 0.9976647f);    // 初期の回転量
                    tL.RotateAround(worldCenter, worldAxis, ra);
                }

                {
                    Transform tRparent = tR.parent;
                    Vector3 localCenter = tR.localPosition + (new Vector3(0f, ry, -rx));    // ローカル座標系での回転中心位置
                    Vector3 worldCenter = tRparent.TransformPoint(localCenter);             // ワールド座標系での回転中心位置
                    Vector3 localAxis = new Vector3(-1f, 0f, 0f);                           // ローカル座標系での回転軸
                    Vector3 worldAxis = tR.TransformDirection(localAxis);                   // ワールド座標系での回転軸

                    tR.localRotation = new Quaternion(0.9976647f, 0.06805764f, -0.001350592f, -0.005603582f);   // 初期の回転量
                    tR.RotateAround(worldCenter, worldAxis, -ra);
                }
            }
        }

        // 下半身の大きさを調整
        // LowerBodyScale : 下半身の大きさを調整する CM3D.MaidVoicePitch.Plugin.cs の追加メソッド
        // http://pastebin.com/VDZVUQED
        // http://pastebin.com/D25zzGSz
        // http://pastebin.com/TaNfrY8d
        // その１>>961, >>970
        // http://jbbs.shitaraba.net/bbs/read.cgi/game/55179/1438196715/961
        // http://jbbs.shitaraba.net/bbs/read.cgi/game/55179/1438196715/970
        //
        // CM3D.MaidVoicePitch.Plugin を適用し #WIDESLIDER# を有効にした状態で、メイドのフリーコメント欄に、
        // #PELSCL=1.0,1.0,1.0# の記述で骨盤周りのサイズ調整。表記順は、幅,奥行き,高さ。
        // #THISCL=1.0,1.0# の記述で足全体のサイズ調整。表記順は、幅,奥行き。
        // #THIPOS=0.0,0.0# の記述で足全体の位置調整。表記順は、左右,前後。
        void LowerBodyScaleAwake()
        {
            string tag = "sintyou";
            string bname = "Bip01 ? Thigh";
            string key = string.Concat("min+" + tag, "*", bname.Replace('?', 'L'));

            if (BoneMorph.dic.ContainsKey(key))
            {
                return;
            }

            BoneMorphSetScale(tag, bname, 1f, 1f, 1f, 1f, 1f, 1f);
        }

        static void BoneMorphSetScale(string tag, string bname, float x, float y, float z, float x2, float y2, float z2)
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

        void LowerBodyScale(Maid maid)
        {
            BoneMorph_ bm_ = maid.body0.bonemorph;

            {
                List<Transform> tListP = new List<Transform>();
                List<Transform> tListHL = new List<Transform>();
                List<Transform> tListHR = new List<Transform>();
                for (int i = 0; i < bm_.bones.Count; i++)
                {
                    if (bm_.bones[i].linkT == null) continue;
                    if (bm_.bones[i].linkT.name == "Bip01 Pelvis_SCL_") tListP.Add(bm_.bones[i].linkT);
                    if (bm_.bones[i].linkT.name == "Hip_L") tListHL.Add(bm_.bones[i].linkT);
                    if (bm_.bones[i].linkT.name == "Hip_R") tListHR.Add(bm_.bones[i].linkT);
                }
                if (tListP.Count < 1 || tListHL.Count < 1 || tListHR.Count < 1) return;

                float mw = ExSaveData.GetFloat(maid, PluginName, "PELSCL.width", 1f);
                float md = ExSaveData.GetFloat(maid, PluginName, "PELSCL.depth", 1f);
                float mh = ExSaveData.GetFloat(maid, PluginName, "PELSCL.height", 1f);

                Vector3 sclrvs = new Vector3(mh, md, mw);

                foreach (Transform t in tListP) t.localScale = Vector3.Scale(t.localScale, sclrvs);
                foreach (Transform t in tListHL) t.localScale = Vector3.Scale(t.localScale, sclrvs);
                foreach (Transform t in tListHR) t.localScale = Vector3.Scale(t.localScale, sclrvs);
            }

            {
                List<Transform> tListTL = new List<Transform>();
                List<Transform> tListTR = new List<Transform>();
                for (int i = 0; i < bm_.bones.Count; i++)
                {
                    if (bm_.bones[i].linkT.name == "Bip01 L Thigh") tListTL.Add(bm_.bones[i].linkT);
                    if (bm_.bones[i].linkT.name == "Bip01 R Thigh") tListTR.Add(bm_.bones[i].linkT);
                }
                if (tListTL.Count < 1 || tListTR.Count < 1) return;

                float mw = ExSaveData.GetFloat(maid, PluginName, "THISCL.width", 1f);
                float md = ExSaveData.GetFloat(maid, PluginName, "THISCL.depth", 1f);
                float mh = 1.0f;

                Vector3 sclrvs = new Vector3(mh, md, mw);

                foreach (Transform t in tListTL) t.localScale = Vector3.Scale(t.localScale, sclrvs);
                foreach (Transform t in tListTR) t.localScale = Vector3.Scale(t.localScale, sclrvs);
            }

            {
                List<Transform> tListTL = new List<Transform>();
                List<Transform> tListTR = new List<Transform>();
                List<Transform> tListHL = new List<Transform>();
                List<Transform> tListHR = new List<Transform>();
                for (int i = 0; i < bm_.bones.Count; i++)
                {
                    if (bm_.bones[i].linkT == null) continue;
                    if (bm_.bones[i].linkT.name == "Hip_L") tListHL.Add(bm_.bones[i].linkT);
                    if (bm_.bones[i].linkT.name == "Hip_R") tListHR.Add(bm_.bones[i].linkT);
                    if (bm_.bones[i].linkT.name == "Bip01 L Thigh") tListTL.Add(bm_.bones[i].linkT);
                    if (bm_.bones[i].linkT.name == "Bip01 R Thigh") tListTR.Add(bm_.bones[i].linkT);
                }
                if (tListHL.Count < 1 || tListHR.Count < 1 || tListTL.Count < 1 || tListTR.Count < 1) return;

                float dx = ExSaveData.GetFloat(maid, PluginName, "THIPOS.x", 0f);
                float dz = ExSaveData.GetFloat(maid, PluginName, "THIPOS.z", 0f);
                float dy = 0.0f;

                Vector3 posrvsL = new Vector3(dy, dz / 1000f, -dx / 1000f);
                Vector3 posrvsR = new Vector3(dy, dz / 1000f, dx / 1000f);

                foreach (Transform t in tListTL) t.localPosition += posrvsL;
                foreach (Transform t in tListTR) t.localPosition += posrvsR;
                foreach (Transform t in tListHL) t.localPosition += posrvsL;
                foreach (Transform t in tListHR) t.localPosition += posrvsR;
            }

            if (ExSaveData.GetBool(maid, PluginName, "SKTSCL", false))
            {
                float mw = ExSaveData.GetFloat(maid, PluginName, "SKTSCL.width", 1f);
                float md = ExSaveData.GetFloat(maid, PluginName, "SKTSCL.depth", 1f);
                float mh = ExSaveData.GetFloat(maid, PluginName, "SKTSCL.height", 1f);
                List<Transform> tListS = new List<Transform>();
                for (int i = 0; i < bm_.bones.Count; i++)
                {
                    if (bm_.bones[i].linkT.name == "Skirt") tListS.Add(bm_.bones[i].linkT);
                }
                if (tListS.Count < 1) return;
                Vector3 sclrvs = new Vector3(mh, md, mw);
                foreach (Transform t in tListS) t.localScale = Vector3.Scale(t.localScale, sclrvs);
            }
        }

        // ForeArmFix : 前腕の歪みを修正する CM3D.MaidVoicePitch.Plugin.cs の追加メソッド
        // CM3D.MaidVoicePitch.Plugin を適用しメイドのフリーコメント欄に #FARMFIX# の記述で前腕の歪みを修正する。
        // 前腕歪みバグを修正
        void ForeArmFixAwake()
        {
            string tag = "sintyou";
            string bname = "Bip01 ? Forearm";
            string key = string.Concat("min+" + tag, "*", bname.Replace('?', 'L'));

            if (BoneMorph.dic.ContainsKey(key))
            {
                return;
            }

            BoneMorphSetScale(tag, bname, 1f, 1f, 1f, 1f, 1f, 1f);
        }

        void ForeArmFix(Maid maid)//, string freeComment)
        {
            if (!ExSaveData.GetBool(maid, PluginName, "FARMFIX", false))
            {
                return;
            }

            BoneMorph_ bm_ = maid.body0.bonemorph;
            List<Transform> tListFAL = new List<Transform>();
            List<Transform> tListFAR = new List<Transform>();
            float sclUAx = -1f;

            for (int i = 0; i < bm_.bones.Count; i++)
            {
                if (bm_.bones[i].linkT == null) continue;
                if (bm_.bones[i].linkT.name == "Bip01 L Forearm") tListFAL.Add(bm_.bones[i].linkT);
                if (bm_.bones[i].linkT.name == "Bip01 R Forearm") tListFAR.Add(bm_.bones[i].linkT);
                if (sclUAx < 0f && bm_.bones[i].linkT.name == "Bip01 L UpperArm") sclUAx = bm_.bones[i].linkT.localScale.x;
            }
            if (sclUAx < 0f || tListFAL.Count < 1 || tListFAR.Count < 1) return;

            Vector3 sclUA = new Vector3(sclUAx, 1f, 1f);

            Vector3 antisclUA_d = new Vector3(1f / sclUAx - 1f, 0f, 0f);

            Vector3 eaFAL = tListFAL[0].localRotation.eulerAngles;
            Vector3 eaFAR = tListFAR[0].localRotation.eulerAngles;

            Quaternion antirotFAL = Quaternion.Euler(eaFAL - new Vector3(180f, 180f, 180f));
            Quaternion antirotFAR = Quaternion.Euler(eaFAR - new Vector3(180f, 180f, 180f));
            Vector3 sclFAL_d = antirotFAL * antisclUA_d;
            Vector3 sclFAR_d = antirotFAR * antisclUA_d;

            Vector3 antisclFAL = new Vector3(1f, 1f, 1f) + new Vector3(Mathf.Abs(sclFAL_d.x), Mathf.Abs(sclFAL_d.y), Mathf.Abs(sclFAL_d.z));
            Vector3 antisclFAR = new Vector3(1f, 1f, 1f) + new Vector3(Mathf.Abs(sclFAR_d.x), Mathf.Abs(sclFAR_d.y), Mathf.Abs(sclFAR_d.z));

            foreach (Transform t in tListFAL) t.localScale = Vector3.Scale(antisclFAL, sclUA);
            foreach (Transform t in tListFAR) t.localScale = Vector3.Scale(antisclFAR, sclUA);
        }

        // SpineScale : 胴のサイズを調整する CM3D.MaidVoicePitch.Plugin.cs の追加メソッド
        void SpineScale(Maid maid)
        {
            //            Match sclspi = Regex.Match(freeComment, @"#SPISCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#");
            //            Match scls0a = Regex.Match(freeComment, @"#S0ASCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#");
            //            Match scls1_ = Regex.Match(freeComment, @"#S1_SCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#");
            //            Match scls1a = Regex.Match(freeComment, @"#S1ASCL=([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)#");
            //            if (sclspi.Groups.Count < 4 && scls0a.Groups.Count < 4 && scls1_.Groups.Count < 4 && scls1a.Groups.Count < 4) return;

            BoneMorph_ bm_ = maid.body0.bonemorph;

            // 胴(下腹部周辺)
            if (ExSaveData.GetBool(maid, PluginName, "SPISCL", false))
            {
                float mw = ExSaveData.GetFloat(maid, PluginName, "SPISCL.width", 1f);
                float md = ExSaveData.GetFloat(maid, PluginName, "SPISCL.depth", 1f);
                float mh = ExSaveData.GetFloat(maid, PluginName, "SPISCL.height", 1f);

                List<Transform> tListS = new List<Transform>();

                for (int i = 0; i < bm_.bones.Count; i++)
                {
                    if (bm_.bones[i].linkT == null) continue;
                    if (bm_.bones[i].linkT.name == "Bip01 Spine_SCL_") tListS.Add(bm_.bones[i].linkT);
                }

                if (tListS.Count >= 1)
                {
                    Vector3 scl = new Vector3(mh, md, mw);
                    foreach (Transform t in tListS) t.localScale = Vector3.Scale(t.localScale, scl);
                }
            }
            // 胴0a(腹部周辺)
            //            if (scls0a.Groups.Count >= 4)
            if (ExSaveData.GetBool(maid, PluginName, "S0ASCL", false))
            {
                float mw = ExSaveData.GetFloat(maid, PluginName, "S0ASCL.width", 1f);
                float md = ExSaveData.GetFloat(maid, PluginName, "S0ASCL.depth", 1f);
                float mh = ExSaveData.GetFloat(maid, PluginName, "S0ASCL.height", 1f);
                List<Transform> tListS = new List<Transform>();

                for (int i = 0; i < bm_.bones.Count; i++)
                {
                    if (bm_.bones[i].linkT == null) continue;
                    if (bm_.bones[i].linkT.name == "Bip01 Spine0a_SCL_") tListS.Add(bm_.bones[i].linkT);
                }

                if (tListS.Count >= 1)
                {
                    Vector3 scl = new Vector3(mh, md, mw);
                    foreach (Transform t in tListS) t.localScale = Vector3.Scale(t.localScale, scl);
                }
            }
            // 胴1_(みぞおち周辺)
            if (ExSaveData.GetBool(maid, PluginName, "S1_SCL", false))
            {
                float mw = ExSaveData.GetFloat(maid, PluginName, "S1_SCL.width", 1f);
                float md = ExSaveData.GetFloat(maid, PluginName, "S1_SCL.depth", 1f);
                float mh = ExSaveData.GetFloat(maid, PluginName, "S1_SCL.height", 1f);
                //            if (scls1_.Groups.Count >= 4)
                //            {
                List<Transform> tListS = new List<Transform>();

                for (int i = 0; i < bm_.bones.Count; i++)
                {
                    if (bm_.bones[i].linkT == null) continue;
                    if (bm_.bones[i].linkT.name == "Bip01 Spine1_SCL_") tListS.Add(bm_.bones[i].linkT);
                }

                if (tListS.Count >= 1)
                {
                    Vector3 scl = new Vector3(mh, md, mw);
                    foreach (Transform t in tListS) t.localScale = Vector3.Scale(t.localScale, scl);
                }
            }
            // 胴1a(首・肋骨周辺)
            if (ExSaveData.GetBool(maid, PluginName, "S1ASCL", false))
            {
                float mw = ExSaveData.GetFloat(maid, PluginName, "S1ASCL.width", 1f);
                float md = ExSaveData.GetFloat(maid, PluginName, "S1ASCL.depth", 1f);
                float mh = ExSaveData.GetFloat(maid, PluginName, "S1ASCL.height", 1f);
                List<Transform> tListS = new List<Transform>();

                for (int i = 0; i < bm_.bones.Count; i++)
                {
                    if (bm_.bones[i].linkT == null) continue;
                    if (bm_.bones[i].linkT.name == "Bip01 Spine1a_SCL_") tListS.Add(bm_.bones[i].linkT);
                }

                if (tListS.Count >= 1)
                {
                    Vector3 scl = new Vector3(mh, md, mw);
                    foreach (Transform t in tListS) t.localScale = Vector3.Scale(t.localScale, scl);
                }
            }
        }

        // スライダー範囲を拡大
        void WideSlider(Maid maid)
        {
            bool bSlider = ExSaveData.GetBool(maid, PluginName, "WIDESLIDER", false);

            if (!bSlider)
            {
                return;
            }

            float sliderScale = 20f;

            TBody tbody = maid.body0;
            if (tbody == null || tbody.bonemorph == null || tbody.bonemorph.bones == null)
            {
                return;
            }

            BoneMorph_ boneMorph_ = tbody.bonemorph;

            string[] PropNames = Helper.GetInstanceField(typeof(BoneMorph_), boneMorph_, "PropNames") as string[];
            if (PropNames == null)
            {
                return;
            }

            for (int i = boneMorph_.bones.Count - 1; i >= 0; i--)
            {
                BoneMorphLocal boneMorphLocal = boneMorph_.bones[i];
                Vector3 scl = new Vector3(1f, 1f, 1f);
                Vector3 pos = boneMorphLocal.pos;
                for (int j = 0; j < (int)PropNames.Length; j++)
                {
                    float s = 1f;
                    switch (j)
                    {
                        case 0:
                            s = boneMorph_.SCALE_Kubi;
                            break;
                        case 1:
                            s = boneMorph_.SCALE_Ude;
                            break;
                        case 2:
                            s = boneMorph_.SCALE_Eye;
                            break;
                        case 3:
                            s = boneMorph_.Postion_EyeX * (0.5f + boneMorph_.Postion_EyeY * 0.5f);
                            break;
                        case 4:
                            s = boneMorph_.Postion_EyeY;
                            break;
                        case 5:
                            s = boneMorph_.SCALE_HeadX;
                            break;
                        case 6:
                            s = boneMorph_.SCALE_HeadY;
                            break;
                        case 7:
                            s = boneMorph_.SCALE_DouPer;
                            if (boneMorphLocal.Kahanshin == 0f)
                            {
                                s = 1f - s;
                            }
                            break;
                        case 8:
                            s = boneMorph_.SCALE_Sintyou;
                            break;
                        case 9:
                            s = boneMorph_.SCALE_Koshi;
                            break;
                        case 10:
                            s = boneMorph_.SCALE_Kata;
                            break;
                        case 11:
                            s = boneMorph_.SCALE_West;
                            break;
                        default:
                            s = 1f;
                            break;
                    }

                    if ((boneMorphLocal.atr & 1 << (j & 31)) != 0)
                    {
                        Vector3 v0 = boneMorphLocal.vecs_min[j];
                        Vector3 v1 = boneMorphLocal.vecs_max[j];

                        Vector3 n0 = v0 * sliderScale - v1 * (sliderScale - 1f);
                        Vector3 n1 = v1 * sliderScale - v0 * (sliderScale - 1f);
                        float f = (s + sliderScale - 1f) * (1f / (sliderScale * 2.0f - 1f));
                        scl = Vector3.Scale(scl, Vector3.Lerp(n0, n1, f));
                        //  scl = Vector3.Scale(scl, Vector3.Lerp(v0, v1, s));
                    }

                    if ((boneMorphLocal.atr & 1 << (j + 16 & 31)) != 0)
                    {
                        Vector3 v0 = boneMorphLocal.vecs_min[j + 16];
                        Vector3 v1 = boneMorphLocal.vecs_max[j + 16];

                        Vector3 n0 = v0 * sliderScale - v1 * (sliderScale - 1f);
                        Vector3 n1 = v1 * sliderScale - v0 * (sliderScale - 1f);
                        float f = (s + sliderScale - 1f) * (1f / (sliderScale * 2.0f - 1f));
                        pos = Vector3.Scale(pos, Vector3.Lerp(n0, n1, f));

                        //  pos = Vector3.Scale(pos, Vector3.Lerp(v0, v1, s));
                    }
                }

                if (boneMorphLocal.linkT == null)
                {
                    continue;
                }

                if (boneMorphLocal.linkT.name != null && boneMorphLocal.linkT.name.Contains("Thigh_SCL_"))
                {
                    boneMorph_.SnityouOutScale = Mathf.Pow(scl.x, 0.9f);
                }
                boneMorphLocal.linkT.localPosition = pos;
                boneMorphLocal.linkT.localScale = scl;
            }
        }

        // スライダー範囲拡大を指定するテンプレートファイルの読み込み (テスト中)
        void TestSliderTemplate(Maid maid)
        {
            // エディット画面以外では無視
            if (Application.loadedLevel != 5)
            {
                return;
            }

            SliderTemplate sliderTemplate = sliderTemplates.Get(ExSaveData.Get(maid, PluginName, "SLIDER_TEMPLATE", null));
            if (sliderTemplate == null)
            {
                return;
            }

            foreach (var kv in sliderTemplate.Sliders)
            {
                string name = kv.Key;
                SliderTemplate.Slider slider = kv.Value;
                MPN mpn = Helper.ToEnum<MPN>(name, MPN.null_mpn);
                if (mpn != MPN.null_mpn)
                {
                    MaidProp maidProp = maid.GetProp(mpn);
                    maidProp.min = (int)slider.min;
                    maidProp.max = (int)slider.max;
                }
            }
        }

        // リップシンク強度指定
        void TestLipSync(Maid maid)
        {
            TBody tbody = maid.body0;
            if (tbody != null)
            {
                float f1 = Mathf.Clamp01(ExSaveData.GetFloat(maid, PluginName, "LIPSYNC", 1f));
                maid.VoicePara_1 = f1 * 0.5f;
                maid.VoicePara_2 = f1 * 0.074f;
                maid.VoicePara_3 = f1 * 0.5f;
                maid.VoicePara_4 = f1 * 0.05f;
                if (f1 < 0.01f)
                {
                    maid.voice_ao_f2 = 0;
                }
            }
        }

        // 骨盤部コリジョン指定 (空中に浮くので注意)
        void TestPelvis(Maid maid)
        {
            TBody tbody = maid.body0;
            if (tbody != null && tbody.Pelvis != null && ExSaveData.GetBool(maid, PluginName, "PELVIS", false))
            {
                float x = ExSaveData.GetFloat(maid, PluginName, "PELVIS.x", 0f);
                float y = ExSaveData.GetFloat(maid, PluginName, "PELVIS.y", 0f);
                float z = ExSaveData.GetFloat(maid, PluginName, "PELVIS.z", 0f);
                tbody.Pelvis.localScale = new Vector3(x, y, z);
            }
        }

        //
        void TestPropSet()
        {
            if (!bKagTagPropSetHooked)
            {
                OverWriteTagCallback("propset", delegate (string typeName, Type type, BaseKagManager baseKagManager, KagTagSupport tag_data)
                {
                    return TagPropSet(typeName, type, baseKagManager, tag_data);
                });

                OverWriteTagCallback("faceblend", delegate (string typeName, Type type, BaseKagManager baseKagManager, KagTagSupport tag_data)
                {
                    return TagFaceBlend(typeName, type, baseKagManager, tag_data);
                });

                OverWriteTagCallback("face", delegate (string typeName, Type type, BaseKagManager baseKagManager, KagTagSupport tag_data)
                {
                    return TagFace(typeName, type, baseKagManager, tag_data);
                });

                OverWriteTagCallback("eyetocamera", delegate (string typeName, Type type, BaseKagManager baseKagManager, KagTagSupport tag_data)
                {
                    return TagEyeToCamera(typeName, type, baseKagManager, tag_data);
                });

                bKagTagPropSetHooked = true;
            }
        }

        static void OverWriteTagCallback(string tagName, TagProcDelegate tagProcDelegate)
        {
            foreach (var kv in GameMain.Instance.ScriptMgr.kag_mot_dic)
            {
                BaseKagManager mgr = kv.Value;
                KagScript kag = mgr.kag;
                kag.RemoveTagCallBack(tagName);
                kag.AddTagCallBack(tagName, new KagScript.KagTagCallBack(delegate (KagTagSupport tag_data)
                {
                    return tagProcDelegate("MotionKagManager", typeof(MotionKagManager), mgr, tag_data);
                }));
            }

            {
                BaseKagManager mgr = GameMain.Instance.ScriptMgr.adv_kag;
                KagScript kag = mgr.kag;
                kag.RemoveTagCallBack(tagName);
                kag.AddTagCallBack(tagName, new KagScript.KagTagCallBack(delegate (KagTagSupport tag_data)
                {
                    return tagProcDelegate("ADVKagManager", typeof(ADVKagManager), mgr, tag_data);
                }));
            }

            {
                BaseKagManager mgr = GameMain.Instance.ScriptMgr.yotogi_kag;
                KagScript kag = mgr.kag;
                kag.RemoveTagCallBack(tagName);
                kag.AddTagCallBack(tagName, new KagScript.KagTagCallBack(delegate (KagTagSupport tag_data)
                {
                    return tagProcDelegate("YotogiKagManager", typeof(YotogiKagManager), mgr, tag_data);
                }));
            }
        }

        public bool TagPropSet(string dbg, Type type, BaseKagManager baseKagManager, KagTagSupport tag_data)
        {
            baseKagManager.CheckAbsolutelyNecessaryTag(tag_data, "propset", new string[] { "category", "val" });

            bool flag = tag_data.IsValid("temp");
            string str = tag_data.GetTagProperty("category").AsString();
            int num = tag_data.GetTagProperty("val").AsInteger();

            Maid maidAndMan = GetMaidAndMan(tag_data);
            if (maidAndMan == null)
            {
                return false;
            }

            if (ExSaveData.GetBool(maidAndMan, PluginName, "PROPSET_OFF", false))
            {
                foreach (MPN mpn in Enum.GetValues(typeof(MPN)))
                {
                    if (mpn.ToString("G") == str)
                    {
                        return false;
                    }
                }
            }
            maidAndMan.SetProp(str, num, flag);
            return false;
        }

        public bool TagFaceBlend(string dbg, Type type, BaseKagManager baseKagManager, KagTagSupport tag_data)
        {
            baseKagManager.CheckAbsolutelyNecessaryTag(tag_data, "faceblend", new string[] { "name" });
            string str = tag_data.GetTagProperty("name").AsString();
            Maid maidAndMan = GetMaidAndMan(tag_data);
            if (maidAndMan == null)
            {
                return false;
            }
            if (ExSaveData.GetBool(maidAndMan, PluginName, "FACEBLEND_OFF", false))
            {
                return false;
            }
            if (str == "なし")
            {
                str = "無し";
            }
            string str1 = ProcFaceBlendName(maidAndMan, str);
            maidAndMan.FaceBlend(str1);
            return false;
        }

        public bool TagFace(string dbg, Type type, BaseKagManager baseKagManager, KagTagSupport tag_data)
        {
            baseKagManager.CheckAbsolutelyNecessaryTag(tag_data, "face", new string[] { "name" });
            string str0 = tag_data.GetTagProperty("name").AsString();
            Maid maidAndMan = GetMaidAndMan(tag_data);
            if (maidAndMan == null)
            {
                return false;
            }
            if (ExSaveData.GetBool(maidAndMan, PluginName, "FACE_OFF", false))
            {
                return false;
            }
            string str = ProcFaceName(maidAndMan, str0);
            int num = 0;
            if (tag_data.IsValid("wait"))
            {
                num = tag_data.GetTagProperty("wait").AsInteger();
            }
            WaitEventList waitEventList = GetWaitEventList(baseKagManager, "face");
            float faceAnimeSpeed = ExSaveData.GetFloat(maidAndMan, PluginName, "FACE_ANIME_SPEED", 1f);
            if (num > 0)
            {
                waitEventList.Add(() =>
                {
                    if (maidAndMan != null && maidAndMan.body0 != null && maidAndMan.body0.isLoadedBody)
                    {
                        maidAndMan.FaceAnime(str, 1f / faceAnimeSpeed, 0);
                    }
                }, num);
            }
            else
            {
                maidAndMan.FaceAnime(str, 1f / faceAnimeSpeed, 0);
                waitEventList.Clear();
            }
            return false;
        }

        public bool TagEyeToCamera(string dbg, Type type, BaseKagManager baseKagManager, KagTagSupport tag_data)
        {
            Maid maidAndMan = GetMaidAndMan(tag_data);
            if (maidAndMan == null)
            {
                return false;
            }
            if (ExSaveData.GetBool(maidAndMan, PluginName, "EYETOCAMERA_OFF", false))
            {
                return false;
            }
            baseKagManager.CheckAbsolutelyNecessaryTag(tag_data, "eyetocamera", new string[] { "move" });
            string str = tag_data.GetTagProperty("move").AsString();
            Maid.EyeMoveType eyeMoveType = Maid.EyeMoveType.無し;
            try
            {
                eyeMoveType = (Maid.EyeMoveType)((int)Enum.Parse(typeof(Maid.EyeMoveType), str));
            }
            catch (ArgumentException)
            {
                NDebug.Assert(string.Concat("Maid.EyeMoveType\nenum parse error.[", str, "]"));
            }
            int num = 500;
            if (tag_data.IsValid("blend"))
            {
                num = tag_data.GetTagProperty("blend").AsInteger();
            }
            maidAndMan.EyeToCamera(eyeMoveType, GameUty.MillisecondToSecond(num));
            return false;
        }

        public string ProcFaceName(Maid maid, string faceName)
        {
            FaceScriptTemplate t = faceScriptTemplates.Get(ExSaveData.Get(maid, PluginName, "FACE_SCRIPT_TEMPLATE", null));
            if (t == null)
            {
                return faceName;
            }
            return t.ProcFaceName(faceName);
        }

        public string ProcFaceBlendName(Maid maid, string faceBlendName)
        {
            FaceScriptTemplate t = faceScriptTemplates.Get(ExSaveData.Get(maid, PluginName, "FACE_SCRIPT_TEMPLATE", null));
            if (t == null)
            {
                return faceBlendName;
            }
            return t.ProcFaceBlendName(faceBlendName);
        }

        static Maid GetMaidAndMan(KagTagSupport tag_data)
        {
            // class BaseKagManager protected static Maid MaidAndMan(KagTagSupport);
            MethodInfo methodInfo = typeof(BaseKagManager).GetMethod(
                "GetMaidAndMan",
                BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new Type[] { typeof(KagTagSupport) },
                null
            );
            object obj = methodInfo.Invoke(null, new object[] { tag_data });
            return obj as Maid;
        }

        static WaitEventList GetWaitEventList(BaseKagManager baseKagManager, string list_name)
        {
            // class BaseKagManager protected WaitEventList GetWaitEventList(string list_name)
            MethodInfo methodInfo = typeof(BaseKagManager).GetMethod(
                "GetWaitEventList",
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(string) },
                null
            );
            object obj = methodInfo.Invoke(baseKagManager, new object[] { list_name });
            return obj as WaitEventList;
        }
    }

    public interface ITemplateFile
    {
        bool Load(string fname);
    }

    public class TemplateFiles<T> : Dictionary<string, T> where T : ITemplateFile, new()
    {
        public T Get(string fname)
        {
            if (fname != null)
            {
                T t0;
                if (TryGetValue(fname, out t0))
                {
                    return t0;
                }

                T t1 = new T();
                if (t1.Load(fname))
                {
                    Add(fname, t1);
                    return t1;
                }
            }
            return default(T);
        }
    }

    public class FaceScriptTemplateCache : TemplateFiles<FaceScriptTemplate> { }
    public class SliderTemplateCache : TemplateFiles<SliderTemplate> { }

    public class FaceScriptTemplate : ITemplateFile
    {
        public Dictionary<string, string> FaceBlends { get; set; }
        public Dictionary<string, string> Faces { get; set; }

        public FaceScriptTemplate()
        {
            Clear();
        }

        public void Clear()
        {
            FaceBlends = new Dictionary<string, string>();
            Faces = new Dictionary<string, string>();
        }

        public bool Load(string fname)
        {
            bool result = false;
            Clear();
            var xd = new XmlDocument();
            try
            {
                if (File.Exists(fname))
                {
                    xd.Load(fname);
                    foreach (XmlNode e in xd.SelectNodes("/facescripttemplate/faceblends/faceblend"))
                    {
                        FaceBlends[e.Attributes["key"].Value] = e.Attributes["value"].Value;
                    }
                    foreach (XmlNode e in xd.SelectNodes("/facescripttemplate/faces/face"))
                    {
                        Faces[e.Attributes["key"].Value] = e.Attributes["value"].Value;
                    }
                    result = true;
                }
            }
            catch (Exception e)
            {
                Helper.ShowException(e);
            }
            return result;
        }

        public string ProcFaceName(string faceName)
        {
            string s;
            if (Faces.TryGetValue(faceName, out s))
            {
                return s;
            }
            return faceName;
        }

        public string ProcFaceBlendName(string faceBlendName)
        {
            string s;
            if (FaceBlends.TryGetValue(faceBlendName, out s))
            {
                return s;
            }
            return faceBlendName;
        }
    }

    public class SliderTemplate : ITemplateFile
    {
        public class Slider
        {
            public float min;
            public float max;
        }

        public Dictionary<string, Slider> Sliders { get; set; }

        public SliderTemplate()
        {
            Clear();
        }

        public void Clear()
        {
            Sliders = new Dictionary<string, Slider>();
        }

        public bool Load(string fname)
        {
            bool result = false;
            Clear();
            var xd = new XmlDocument();
            try
            {
                if (File.Exists(fname))
                {
                    xd.Load(fname);
                    foreach (XmlNode e in xd.SelectNodes("/slidertemplate/sliders/slider"))
                    {
                        Sliders[e.Attributes["name"].Value] = new Slider
                        {
                            min = Helper.FloatTryParse(e.Attributes["min"].Value, 0f),
                            max = Helper.FloatTryParse(e.Attributes["max"].Value, 0f)
                        };
                    }
                    result = true;
                }
            }
            catch (Exception e)
            {
                Helper.ShowException(e);
            }
            return result;
        }
    }

    public static class Helper
    {
        static StreamWriter logStreamWriter = null;
        public static DateTime now = DateTime.Now;

        public static void Log(string s)
        {
            if (logStreamWriter == null)
            {
                string fname = (@".\MaidVoicePitch_" + now.ToString("yyyyMMdd_HHmmss") + ".log");
                logStreamWriter = new StreamWriter(fname, true);
            }
            logStreamWriter.Write(s);
            logStreamWriter.Flush();
        }

        public static float FloatTryParse(string s, float defaultValue)
        {
            float f = defaultValue;
            float.TryParse(s, out f);
            return f;
        }

        // http://stackoverflow.com/a/1082587/2132223
        public static TEnum ToEnum<TEnum>(this string strEnumValue, TEnum defaultValue)
        {
            if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
                return defaultValue;
            return (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
        }

        // http://stackoverflow.com/a/3303182/2132223
        public static FieldInfo GetFieldInfo(Type type, string fieldName)
        {
            return type.GetField(fieldName,
                                 BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        }

        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            FieldInfo field = GetFieldInfo(type, fieldName);
            return field == null ? null : field.GetValue(instance);
        }

        public static void SetInstanceField(Type type, object instance, string fieldName, object val)
        {
            FieldInfo field = GetFieldInfo(type, fieldName);
            if (field != null)
            {
                field.SetValue(instance, val);
            }
        }

        static public void ShowException(Exception ex)
        {
            Console.WriteLine("{0}", ex.Message);
            StackTrace st = new StackTrace(ex, true);
            foreach (StackFrame f in st.GetFrames())
            {
                Console.WriteLine(
                    "{0}({1}.{2}) : {3}.{4}",
                    f.GetFileName(), f.GetFileLineNumber(), f.GetFileColumnNumber(),
                    f.GetMethod().DeclaringType, f.GetMethod()
                );
            }
        }
    }
}

using CM3D2.ExternalSaveData.Managed;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

#if DYNAMIC_PLUGIN
using System.IO;
using CM3D2.DynamicLoader.Plugin;
using PluginBase = CM3D2.DynamicLoader.Plugin.DynamicPluginBase;
[assembly: AssemblyVersion("1.0.*")]  // AssemblyVersion は削除しないこと
#endif

namespace CM3D2.PersonalizedEditSceneSettings.Plugin
{
    [PluginName("CM3D2 PersonalizedEditSceneSettings"), PluginVersion("0.1.5.0")]
    public class PersonalizedEditSceneSettings : PluginBase
    {
        const string PluginName = "CM3D2.PersonalizedEditSceneSettings";
        int previousLevel = -1;
        bool bLevelWasLoaded = false;
        float levelTimer;

        Maid lastMaid;
        bool bLastAutoCam = true;
        bool bLastEyeToCam = true;
        TBody.MaskMode lastClothMaskMode = TBody.MaskMode.None;
        string lastBgName;
        string lastPoseScript = string.Empty;
        string lastPoseLabel = string.Empty;
        Vector3 lastCameraPos;
        Vector3 lastCameraTargetPos;
        float lastCameraDistance;
        Quaternion lastCameraRotation;
        float lastCameraFov;

        CameraMain mainCamera;
        FieldInfo fieldInfo_TBody_m_eMaskMode;
        EditViewReset editViewReset;
        SceneEdit sceneEdit;
        EventDelegate.Callback poseButtonCallback;

#if DYNAMIC_PLUGIN
        static StringWriter Log = new StringWriter();
        static StringWriter Win = new StringWriter();

        public override void OnPluginLoad()
        {
            Awake();
            OnLevelWasLoaded(Application.loadedLevel);
        }

        public override void OnPluginUnload()
        {
            OnDestroy();
        }

        public override void OnGUI()
        {
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), Win.ToString() + Log.ToString());
        }
#endif

        void Awake()
        {
            GameObject.DontDestroyOnLoad(this);
            fieldInfo_TBody_m_eMaskMode = typeof(TBody).GetField("m_eMaskMode", BindingFlags.Instance | BindingFlags.NonPublic);
        }

#if DYNAMIC_PLUGIN
        public override
#endif
        void OnLevelWasLoaded(int level)
        {
            if (previousLevel == 5)
            {
                StoreSettings();
            }

            mainCamera = GameMain.Instance.MainCamera;
            previousLevel = level;
            bLevelWasLoaded = true;
            levelTimer = 0f;
            InitPoseCallback();
        }

#if DYNAMIC_PLUGIN
        public override
#endif
        void Update()
        {
            if (Application.loadedLevel != 5)
            {
                return;
            }
            if (editViewReset == null)
            {
                editViewReset = UnityEngine.Object.FindObjectOfType<EditViewReset>();
            }
            if (sceneEdit == null)
            {
                sceneEdit = UnityEngine.Object.FindObjectOfType<SceneEdit>();
            }

            {
                string gridName = "/UI Root/PresetButtonPanel/ItemPresetsViewer/Scroll View/Grid";
                GameObject goGrid = GameObject.Find(gridName);
                if (goGrid != null)
                {
                    if (poseButtonCallback == null)
                    {
                        poseButtonCallback = new EventDelegate.Callback(ClickPoseCallback);
                    }
                    foreach (Transform t in goGrid.transform)
                    {
                        if (!t.name.StartsWith("Pose_"))
                        {
                            continue;
                        }
                        GameObject go = t.gameObject;
                        UIButton uiButton = go.GetComponent<UIButton>();
                        EventDelegate.Add(uiButton.onClick, poseButtonCallback);
                    }
                }
            }

            if (editViewReset != null && sceneEdit != null)
            {
                Maid maid = GameMain.Instance.CharacterMgr.GetMaid(0);

                if (bLevelWasLoaded)
                {
                    bLevelWasLoaded = false;
                    LoadSettings(maid);
                }

                // UICamera.InputEnable が False になるとサムネール撮影用のために着衣状態になる
                TBody tbody = maid == null ? null : maid.body0;
                if (tbody != null && fieldInfo_TBody_m_eMaskMode != null && UICamera.InputEnable)
                {
                    lastMaid = maid;
                    bLastAutoCam = editViewReset.GetVisibleAutoCam();
                    bLastEyeToCam = editViewReset.GetVisibleEyeToCam();
                    lastBgName = GameMain.Instance.BgMgr.GetBGName();
                    lastClothMaskMode = (TBody.MaskMode)fieldInfo_TBody_m_eMaskMode.GetValue(tbody);
                    lastCameraPos = mainCamera.GetPos();
                    lastCameraTargetPos = mainCamera.GetTargetPos();
                    lastCameraDistance = mainCamera.GetDistance();
                    lastCameraRotation = Camera.main.gameObject.transform.rotation;
                    lastCameraFov = Camera.main.fieldOfView;
                }
            }
        }

#if DYNAMIC_PLUGIN
        public override
#endif
        void LateUpdate()
        {
            if (Application.loadedLevel != 5)
            {
                return;
            }

            levelTimer += Time.deltaTime;
            if (levelTimer < 1f)
            {
                Maid maid = GameMain.Instance.CharacterMgr.GetMaid(0);
                LoadCameraSettings(maid);
            }
        }

        void InitPoseCallback()
        {
            string gridName = "/UI Root/PresetButtonPanel/ItemPresetsViewer/Scroll View/Grid";
            GameObject goGrid = GameObject.Find(gridName);
            if (goGrid != null)
            {
                if (poseButtonCallback == null)
                {
                    poseButtonCallback = new EventDelegate.Callback(ClickPoseCallback);
                }
                foreach (Transform t in goGrid.transform)
                {
                    GameObject go = t.gameObject;
                    UIButton uiButton = go.GetComponent<UIButton>();
                    EventDelegate.Add(uiButton.onClick, poseButtonCallback);
                }
            }
        }

        void ClickPoseCallback()
        {
            string name = UIButton.current.name;

            PresetButtonMgr presetButtonMgr = BaseMgr<PresetButtonMgr>.Instance;
            if (presetButtonMgr == null)
            {
                return;
            }

            var presetButtonCtrl = (PresetButtonCtrl)GetInstanceField(
                presetButtonMgr.GetType(), presetButtonMgr, "m_presetButtonCtrl");
            if (presetButtonCtrl == null)
            {
                return;
            }

            var dicItem = (Dictionary<string, PresetButtonCtrl.ItemPreset>)
                GetInstanceField(presetButtonCtrl.GetType(), presetButtonCtrl, "m_dicItem");
            if (dicItem == null)
            {
                return;
            }

            PresetButtonCtrl.ItemPreset itemPreset;
            if (!dicItem.TryGetValue(name, out itemPreset))
            {
                return;
            }

            var poseInfo = (SceneEdit.PoseInfo)itemPreset.m_pvbInfo;
            if (poseInfo == null)
            {
                return;
            }

            lastPoseScript = poseInfo.strScriptFileName;
            lastPoseLabel = poseInfo.strScriptLabelName;
        }

        void LoadSettings(Maid maid)
        {
            if (maid == null)
            {
                return;
            }

            bool autoCam = ExSaveData.GetBool(maid, PluginName, "AutoCameraInitialValue", true);
            bool eyeToCam = ExSaveData.GetBool(maid, PluginName, "EyeToCameraInitialValue", true);
            string clothMaskMode = ExSaveData.Get(maid, PluginName, "ClothMaskMode", "");
            string bgName = ExSaveData.Get(maid, PluginName, "BgName", "");
            string poseScript = ExSaveData.Get(maid, PluginName, "Pose.Script", "");
            string poseLabel = ExSaveData.Get(maid, PluginName, "Pose.Label", "");

            DebugWriteLine(
                "PersonalizedEditSceneSettings.LoadSettings : name={0}{1}, autoCam={2}, eyeToCam={3}, cloth={4}, bg={5}, pose={6}.{7}",
                maid.Param.status.last_name, maid.Param.status.first_name,
                autoCam, eyeToCam, clothMaskMode, bgName, poseScript, poseLabel);

            editViewReset.SetVisibleAutoCam(autoCam);
            editViewReset.SetVisibleEyeToCam(eyeToCam);
            sceneEdit.ClothesState(Helper.ToEnum<SceneEditInfo.ClothesState>(clothMaskMode, SceneEditInfo.ClothesState.Wear));

            SceneEdit.PVBInfo bgPvbInfo = sceneEdit.m_listBg.FirstOrDefault((i) =>
            {
                var bgInfo = i as SceneEdit.BGInfo;
                if (bgInfo == null)
                {
                    return false;
                }
                return bgInfo.strBGFileName == bgName;
            });
            if (bgPvbInfo != null)
            {
                sceneEdit.Bg(bgPvbInfo);
            }

            SceneEdit.PVBInfo posePvbInfo = sceneEdit.m_listPose.FirstOrDefault((i) =>
            {
                var poseInfo = i as SceneEdit.PoseInfo;
                if (poseInfo == null)
                {
                    return false;
                }
                return poseInfo.strScriptFileName == poseScript
                    && poseInfo.strScriptLabelName == poseLabel;
            });

            if (posePvbInfo != null)
            {
                sceneEdit.Pose(posePvbInfo);
                lastPoseScript = poseScript;
                lastPoseLabel = poseLabel;
            }

            LoadCameraSettings(maid);
        }

        void LoadCameraSettings(Maid maid)
        {
            if (maid == null || !maid.Visible)
            {
                return;
            }

            bool autoCam = ExSaveData.GetBool(maid, PluginName, "AutoCameraInitialValue", true);
            if (autoCam)
            {
                return;
            }

            float cameraPosX = ExSaveData.GetFloat(maid, PluginName, "CameraPos.x", float.NaN);
            float cameraPosY = ExSaveData.GetFloat(maid, PluginName, "CameraPos.y", float.NaN);
            float cameraPosZ = ExSaveData.GetFloat(maid, PluginName, "CameraPos.z", float.NaN);
            float cameraTargetPosX = ExSaveData.GetFloat(maid, PluginName, "CameraTargetPos.x", float.NaN);
            float cameraTargetPosY = ExSaveData.GetFloat(maid, PluginName, "CameraTargetPos.y", float.NaN);
            float cameraTargetPosZ = ExSaveData.GetFloat(maid, PluginName, "CameraTargetPos.z", float.NaN);
            float cameraDistance = ExSaveData.GetFloat(maid, PluginName, "CameraDistance", float.NaN);
            float cameraRotationX = ExSaveData.GetFloat(maid, PluginName, "CameraRotation.x", float.NaN);
            float cameraRotationY = ExSaveData.GetFloat(maid, PluginName, "CameraRotation.y", float.NaN);
            float cameraRotationZ = ExSaveData.GetFloat(maid, PluginName, "CameraRotation.z", float.NaN);
            float cameraRotationW = ExSaveData.GetFloat(maid, PluginName, "CameraRotation.w", float.NaN);
            float cameraFov = ExSaveData.GetFloat(maid, PluginName, "CameraFov", float.NaN);

            if (!float.IsNaN(cameraRotationX) && !float.IsNaN(cameraRotationY) && !float.IsNaN(cameraRotationZ) && !float.IsNaN(cameraRotationW))
            {
                Camera.main.gameObject.transform.rotation = new Quaternion(cameraRotationX, cameraRotationY, cameraRotationZ, cameraRotationW);
            }

            if (!float.IsNaN(cameraPosX) && !float.IsNaN(cameraPosY) && !float.IsNaN(cameraPosZ))
            {
                mainCamera.SetPos(new Vector3(cameraPosX, cameraPosY, cameraPosZ));
            }

            if (!float.IsNaN(cameraTargetPosX) && !float.IsNaN(cameraTargetPosY) && !float.IsNaN(cameraTargetPosZ))
            {
                mainCamera.SetTargetPos(new Vector3(cameraTargetPosX, cameraTargetPosY, cameraTargetPosZ), true);
            }

            if (!float.IsNaN(cameraDistance))
            {
                mainCamera.SetDistance(cameraDistance, true);
            }
            if (!float.IsNaN(cameraFov))
            {
                Camera.main.fieldOfView = cameraFov;
            }
        }

        void StoreSettings()
        {
            Maid maid = lastMaid;
            ExSaveData.SetBool(maid, PluginName, "AutoCameraInitialValue", bLastAutoCam);
            ExSaveData.SetBool(maid, PluginName, "EyeToCameraInitialValue", bLastEyeToCam);
            ExSaveData.Set(maid, PluginName, "ClothMaskMode", lastClothMaskMode.ToString());
            ExSaveData.Set(maid, PluginName, "BgName", lastBgName);
            ExSaveData.Set(maid, PluginName, "Pose.Script", lastPoseScript);
            ExSaveData.Set(maid, PluginName, "Pose.Label", lastPoseLabel);
            ExSaveData.SetFloat(maid, PluginName, "CameraPos.x", lastCameraPos.x);
            ExSaveData.SetFloat(maid, PluginName, "CameraPos.y", lastCameraPos.y);
            ExSaveData.SetFloat(maid, PluginName, "CameraPos.z", lastCameraPos.z);
            ExSaveData.SetFloat(maid, PluginName, "CameraTargetPos.x", lastCameraTargetPos.x);
            ExSaveData.SetFloat(maid, PluginName, "CameraTargetPos.y", lastCameraTargetPos.y);
            ExSaveData.SetFloat(maid, PluginName, "CameraTargetPos.z", lastCameraTargetPos.z);
            ExSaveData.SetFloat(maid, PluginName, "CameraDistance", lastCameraDistance);
            ExSaveData.SetFloat(maid, PluginName, "CameraRotation.x", lastCameraRotation.x);
            ExSaveData.SetFloat(maid, PluginName, "CameraRotation.y", lastCameraRotation.y);
            ExSaveData.SetFloat(maid, PluginName, "CameraRotation.z", lastCameraRotation.z);
            ExSaveData.SetFloat(maid, PluginName, "CameraRotation.w", lastCameraRotation.w);
            ExSaveData.SetFloat(maid, PluginName, "CameraFov", lastCameraFov);

            DebugWriteLine(
                "PersonalizedEditSceneSettings.StoreSettings : name={0}{1}, autoCam={2}, eyeToCam={3}, cloth={4}, bg={5}, pose={6}.{7}",
                maid.Param.status.last_name, maid.Param.status.first_name,
                bLastAutoCam, bLastEyeToCam, lastClothMaskMode, lastBgName, lastPoseScript, lastPoseLabel);
            DebugWriteLine(
                "PersonalizedEditSceneSettings.Camera : pos={0}, tpos={1}, dist={2}, rot={3}, fov={4}"
                , lastCameraPos, lastCameraTargetPos, lastCameraDistance, lastCameraRotation, lastCameraFov);
        }

        // http://stackoverflow.com/a/3303182/2132223
        public static FieldInfo GetFieldInfo(Type type, string fieldName)
        {
            return type.GetField(fieldName,
                                 BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            FieldInfo field = GetFieldInfo(type, fieldName);
            return field == null ? null : field.GetValue(instance);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugWriteLine(string str)
        {
            Console.WriteLine(str);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugWriteLine(string format, params object[] args)
        {
            DebugWriteLine(string.Format(format, args));
        }
    }
}

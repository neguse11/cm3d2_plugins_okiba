using CM3D2.ExternalSaveData.Managed;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;

namespace CM3D2.PersonalizedEditSceneSettings.Plugin
{
    [PluginFilter("CM3D2x64"),
    PluginFilter("CM3D2x86"),
    PluginFilter("CM3D2VRx64"),
    PluginName("CM3D2 EyeToCamera Settings"),
    PluginVersion("0.1.1.0")]
    public class PersonalizedEditSceneSettings : PluginBase
    {
        const string PluginName = "CM3D2.PersonalizedEditSceneSettings";
        static int previousLevel = -1;
        bool bLevelWasLoaded = false;
        float levelTimer;

        Maid lastMaid;
        bool bLastAutoCam = true;
        bool bLastEyeToCam = true;
        TBody.MaskMode lastClothMaskMode = TBody.MaskMode.None;
        string lastBgName;
        string lastPoseName;
        Vector3 lastCameraPos;
        Vector3 lastCameraTargetPos;
        float lastCameraDistance;
        Quaternion lastCameraRotation;
        float lastCameraFov;

        CameraMain mainCamera;
        FieldInfo fieldInfo_TBody_m_eMaskMode;
        EditViewReset editViewReset;
        SceneEdit sceneEdit;

        void Awake()
        {
            GameObject.DontDestroyOnLoad(this);
            fieldInfo_TBody_m_eMaskMode = typeof(TBody).GetField("m_eMaskMode", BindingFlags.Instance | BindingFlags.NonPublic);
        }

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
        }

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
                    lastPoseName = tbody.LastAnimeFN;
                    lastCameraPos = mainCamera.GetPos();
                    lastCameraTargetPos = mainCamera.GetTargetPos();
                    lastCameraDistance = mainCamera.GetDistance();
                    lastCameraRotation = Camera.main.gameObject.transform.rotation;
                    lastCameraFov = Camera.main.fieldOfView;
                }
            }
        }

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
            string poseName = ExSaveData.Get(maid, PluginName, "PoseName", "");

            Console.WriteLine(
                "PersonalizedEditSceneSettings.LoadSettings : name={0}{1}, autoCam={2}, eyeToCam={3}, cloth={4}, bg={5}, pose={6}",
                maid.Param.status.last_name, maid.Param.status.first_name,
                autoCam, eyeToCam, clothMaskMode, bgName, poseName);

            editViewReset.SetVisibleAutoCam(autoCam);
            editViewReset.SetVisibleEyeToCam(eyeToCam);
            sceneEdit.ClothesState(Helper.ToEnum<SceneEditInfo.ClothesState>(clothMaskMode, SceneEditInfo.ClothesState.Wear));

            SceneEdit.PVBInfo bgPvbInfo = sceneEdit.m_listBg.FirstOrDefault(i => i.info.strFileName == bgName);
            if (bgPvbInfo != null)
            {
                sceneEdit.Bg(bgPvbInfo);
            }

            SceneEdit.PVBInfo[] poses = sceneEdit.m_dicPose[maid.Param.status.personal];
            SceneEdit.PVBInfo posePvbInfo = poses.FirstOrDefault(i => i.info.strFileName + ".anm" == poseName);
            if (posePvbInfo != null)
            {
                sceneEdit.Pose(posePvbInfo);
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
            float cameraRotatationX = ExSaveData.GetFloat(maid, PluginName, "CameraRotation.x", float.NaN);
            float cameraRotatationY = ExSaveData.GetFloat(maid, PluginName, "CameraRotation.y", float.NaN);
            float cameraRotatationZ = ExSaveData.GetFloat(maid, PluginName, "CameraRotation.z", float.NaN);
            float cameraRotatationW = ExSaveData.GetFloat(maid, PluginName, "CameraRotation.w", float.NaN);
            float cameraFov = ExSaveData.GetFloat(maid, PluginName, "CameraFov", float.NaN);

            if (!float.IsNaN(cameraRotatationX) && !float.IsNaN(cameraRotatationY) && !float.IsNaN(cameraRotatationZ) && !float.IsNaN(cameraRotatationW))
            {
                Camera.main.gameObject.transform.rotation = new Quaternion(cameraRotatationX, cameraRotatationY, cameraRotatationZ, cameraRotatationW);
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
            ExSaveData.Set(maid, PluginName, "PoseName", lastPoseName);
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

            Console.WriteLine(
                "PersonalizedEditSceneSettings.StoreSettings : name={0}{1}, autoCam={2}, eyeToCam={3}, cloth={4}, bg={5}, pose={6}",
                maid.Param.status.last_name, maid.Param.status.first_name,
                bLastAutoCam, bLastEyeToCam, lastClothMaskMode, lastBgName, lastPoseName);
        }
    }

    internal static class Helper
    {
        // http://stackoverflow.com/a/1082587/2132223
        public static TEnum ToEnum<TEnum>(this string strEnumValue, TEnum defaultValue)
        {
            if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
                return defaultValue;
            return (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
        }
    }
}

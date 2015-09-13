using Mono.Cecil;
using System;
using System.Linq;

namespace CM3D2.MaidVoicePitch.Patcher
{
    public class MaidVoicePitchPatcher : ReiPatcher.Patch.PatchBase
    {
        public override string Version { get { return "0.1.0.0"; } }
        public override string Name { get { return "CM3D2.MaidVoicePitch.Patcer"; } }
        string patchTag { get { return Name + "_PATCHED"; } }

        public override bool CanPatch(ReiPatcher.Patch.PatcherArguments args)
        {
            return args.Assembly.Name.Name == "Assembly-CSharp" && !base.GetPatchedAttributes(args.Assembly).Any(a => a.Info == patchTag);
        }

        public override void PrePatch()
        {
            ReiPatcher.RPConfig.RequestAssembly("Assembly-CSharp.dll");
        }

        public override void Patch(ReiPatcher.Patch.PatcherArguments args)
        {
            try
            {
                AssemblyDefinition ta = args.Assembly;
                AssemblyDefinition da = PatcherHelper.GetAssemblyDefinition(args, "CM3D2.MaidVoicePitch.Managed.dll");
                string m = "CM3D2.MaidVoicePitch.Managed.";

                // TBody.LateUpdateの処理終了後にCM3D2.MaidVoicePitch.Managed.Callbacks.TBody.LateUpdate.Invokeを呼び出す
                PatcherHelper.SetHook(PatcherHelper.HookType.PostCall, ta, "TBody", "LateUpdate", da, m + "Callbacks.TBody.LateUpdate", "Invoke");

                // TBody.MoveHeadAndEyeの処理を完全に乗っ取り、CM3D2.MaidVoicePitch.Managed.Callbacks.TBody.MoveHeadAndEye.Invokeの呼び出しに置き換える
                PatcherHelper.SetHook(PatcherHelper.HookType.PreJump, ta, "TBody", "MoveHeadAndEye", da, m + "Callbacks.TBody.MoveHeadAndEye", "Invoke");

                // BoneMorph_.Blendの処理終了後にCM3D2.MaidVoicePitch.Managed.Callbacks.BoneMorph_.Blend.Invokeを呼び出す
                PatcherHelper.SetHook(PatcherHelper.HookType.PostCall, ta, "BoneMorph_", "Blend", da, m + "Callbacks.BoneMorph_.Blend", "Invoke");

                // AudioSourceMgr.Playの処理終了後にCM3D2.MaidVoicePitch.Managed.Callbacks.AudioSourceMgr.Play.Invokeを呼び出す
                PatcherHelper.SetHook(PatcherHelper.HookType.PostCall, ta, "AudioSourceMgr", "Play", da, m + "Callbacks.AudioSourceMgr.Play", "Invoke");

                // AudioSourceMgr.PlayOneShotの処理終了後にCM3D2.MaidVoicePitch.Managed.Callbacks.AudioSourceMgr.PlayOneShot.Invokeを呼び出す
                PatcherHelper.SetHook(PatcherHelper.HookType.PostCall, ta, "AudioSourceMgr", "PlayOneShot", da, m + "Callbacks.AudioSourceMgr.PlayOneShot", "Invoke");
                SetPatchedAttribute(args.Assembly, patchTag);
            }
            catch (Exception e)
            {
                Helper.ShowException(e);
                throw;
            }
        }
    }
}

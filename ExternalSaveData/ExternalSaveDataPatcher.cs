using Mono.Cecil;
using System.Linq;
using System.Reflection;

[assembly: AssemblyTitle("CM3D2.ExternalSaveData.Patcher")]
[assembly: AssemblyVersion("0.1.3.0")]

namespace CM3D2.ExternalSaveData.Patcher
{
    public class Patcher : ReiPatcher.Patch.PatchBase
    {
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
            AssemblyDefinition ta = args.Assembly;
            AssemblyDefinition da = PatcherHelper.GetAssemblyDefinition(args, "CM3D2.ExternalSaveData.Managed.dll");

            // GameMain.OnInitializeの処理後、CM3D2.略.ExSaveData.DummyInitializeを呼ぶ
            PatcherHelper.SetHook(
                PatcherHelper.HookType.PostCall,
                ta, "GameMain.OnInitialize",
                da, "CM3D2.ExternalSaveData.Managed.ExSaveData.DummyInitialize");

            // GameMain.Deserializeの処理後、CM3D2.略.GameMainCallbacks.Deserialize.Invokeを呼ぶ
            PatcherHelper.SetHook(
                PatcherHelper.HookType.PostCall,
                ta, "GameMain.Deserialize",
                da, "CM3D2.ExternalSaveData.Managed.GameMainCallbacks.Deserialize.Invoke");

            // GameMain.Deserializeの処理後、CM3D2.略.GameMainCallbacks.Serialize.Invokeを呼ぶ
            PatcherHelper.SetHook(
                PatcherHelper.HookType.PostCall,
                ta, "GameMain.Serialize",
                da, "CM3D2.ExternalSaveData.Managed.GameMainCallbacks.Serialize.Invoke");

            // GameMain.DeleteSerializeDataの処理後、CM3D2.略.GameMainCallbacks.DeleteSerializeData.Invokeを呼ぶ
            PatcherHelper.SetHook(
                PatcherHelper.HookType.PostCall,
                ta, "GameMain.DeleteSerializeData",
                da, "CM3D2.ExternalSaveData.Managed.GameMainCallbacks.DeleteSerializeData.Invoke");

            SetPatchedAttribute(args.Assembly, patchTag);
        }
    }
}

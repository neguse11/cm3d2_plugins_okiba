using Mono.Cecil;
using System;
using System.Linq;
using System.Reflection;

[assembly: AssemblyTitle("CM3D2.VoiceNormalizer.Patcher")]
[assembly: AssemblyVersion("0.1.0.0")]

namespace CM3D2.VoiceNormalizer.Patcher
{
    public class VoiceNormalizerPatcher : ReiPatcher.Patch.PatchBase
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
            try
            {
                AssemblyDefinition ta = args.Assembly;
                AssemblyDefinition da = PatcherHelper.GetAssemblyDefinition(args, "CM3D2.VoiceNormalizer.Managed.dll");
                string m = "CM3D2.VoiceNormalizer.Managed.";

                PatcherHelper.SetHook(
                    PatcherHelper.HookType.PreCall,
                    ta, "AudioSourceMgr.LoadFromWf",
                    da, m + "Callbacks.AudioSourceMgr.LoadFromWf.Invoke");
            }
            catch (Exception e)
            {
                Helper.ShowException(e);
                throw;
            }
        }
    }
}

/*
# 1.15 で動作しなかった理由のメモ

## フックする関数はちゃんとシグネチャを見たほうが良い

1.15 で落ちていたのは、メソッドのシグネチャが以下のように変更されたため

```C#
  // .. ver 1.14
  class AudioSourceMgr { public bool LoadFromWf(string f_strFilename); }

  // ver 1.15 ..
  class AudioSourceMgr { public bool LoadFromWf(string f_strFilename, bool stream); }
```

落ちるよりはフックが動作しないほうがはるかに良いので、必ずシグネチャを見て
マッチしない場合はフックせずにあきらめるようにすること。
*/
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

                // 引数の型
                string[] targetArgTypes = {
                    "System.String",
                    "System.Boolean"
                };

                string[] calleeArgTypes = {
                    "AudioSourceMgr",
                    "System.String",
                    "System.Boolean"
                };

                PatcherHelper.SetHook(
                    PatcherHelper.HookType.PreCall,
                    ta, "AudioSourceMgr.LoadFromWf", targetArgTypes,
                    da, m + "Callbacks.AudioSourceMgr.LoadFromWf.Invoke", calleeArgTypes);
            }
            catch (Exception e)
            {
                Helper.ShowException(e);
                throw;
            }
        }
    }
}

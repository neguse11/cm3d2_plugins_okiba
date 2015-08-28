using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Reflection;

namespace CM3D2.FastFade.Patcher
{
    public class FastFadePatch : ReiPatcher.Patch.PatchBase
    {
        const string patchTag = "CM3D2.FastFade_PATCHED";
        const float DefaultCameraFadeInTime = 0.002f;
        const float DefaultUiFadeInTime = 0.002f;
        delegate void InsertInstDelegate(Instruction newInst);

        public override string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }
        public override string Name { get { return "CM3D2.FastFade"; } }

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
            PatchGetAnyMouseAndKey(GetMethod(
                args.Assembly.MainModule.GetType("SceneLogo"),
                "GetAnyMouseAndKey"));

            PatchGetAnyMouseAndKey(GetMethod(
                args.Assembly.MainModule.GetType("SceneWarning"),
                "GetAnyMouseAndKey"));

            TypeDefinition tCameraMain = args.Assembly.MainModule.GetType("CameraMain");
            PatchIsForceSkip(GetMethod(tCameraMain, "FadeIn"));
            PatchIsForceSkip(GetMethod(tCameraMain, "FadeInNoUI"));
            PatchIsForceSkip(GetMethod(tCameraMain, "FadeOut"));
            PatchIsForceSkip(GetMethod(tCameraMain, "FadeOutNoUI"));

            {
                // セーブロード画面のフェードアウト
                MethodDefinition targetMethod = GetMethod(
                    args.Assembly.MainModule.GetType("TweenAlpha"), "Begin");
                Mono.Cecil.Rocks.MethodBodyRocks.SimplifyMacros(targetMethod.Body);
                foreach (Instruction inst in targetMethod.Body.Instructions)
                {
                    // 第二引数 (duration) の値の代わりに DefaultUiFadeInTime を積む
                    if (inst.OpCode == OpCodes.Ldarg && ((ParameterDefinition)inst.Operand).Index == 1)
                    {
                        inst.OpCode = OpCodes.Ldc_R4;
                        inst.Operand = DefaultUiFadeInTime;
                    }
                }
                Mono.Cecil.Rocks.MethodBodyRocks.OptimizeMacros(targetMethod.Body);
            }

            {
                // セーブロード画面のフェードイン
                //      ldc.r4 0.0
                //      stfld float32 TweenAlpha::from
                // を
                //      ldc.r4 1.0
                //      stfld float32 TweenAlpha::from
                // に書き換える
                MethodDefinition targetMethod = GetMethod(
                    args.Assembly.MainModule.GetType("BasePanelMgr"), "FadeInPanel");
                Mono.Cecil.Rocks.MethodBodyRocks.SimplifyMacros(targetMethod.Body);
                foreach (Instruction inst in targetMethod.Body.Instructions)
                {
                    if (inst.OpCode == OpCodes.Stfld && inst.Operand.ToString().Contains("TweenAlpha::from"))
                    {
                        inst.Previous.Operand = 1.0f;
                    }
                }
                Mono.Cecil.Rocks.MethodBodyRocks.OptimizeMacros(targetMethod.Body);
            }

            {
                // 日付アイキャッチ画面スキップ
                //  Input.anyKeyの戻り値を破棄して、常にtrueにする
                MethodDefinition targetMethod = GetMethod(
                    args.Assembly.MainModule.GetType("StartDailyMgr"), "Update");
                Mono.Cecil.Rocks.MethodBodyRocks.SimplifyMacros(targetMethod.Body);
                Instruction inst = targetMethod.Body.Instructions.First(
                    i => i.OpCode == OpCodes.Call && i.Operand.ToString().Contains("Input::get_anyKey"));
                ILProcessor l = targetMethod.Body.GetILProcessor();
                Instruction instInsertPoint = inst.Next;
                InsertInstDelegate o = newInst =>
                {
                    l.InsertBefore(instInsertPoint, newInst);
                };
                o(l.Create(OpCodes.Pop));
                o(l.Create(OpCodes.Ldc_I4, 1));
                Mono.Cecil.Rocks.MethodBodyRocks.OptimizeMacros(targetMethod.Body);
            }

            SetPatchedAttribute(args.Assembly, patchTag);
        }

        void PatchGetAnyMouseAndKey(MethodDefinition targetMethod)
        {
            ILProcessor l = targetMethod.Body.GetILProcessor();
            Instruction instInsertPoint = targetMethod.Body.Instructions.First();
            InsertInstDelegate o = newInst =>
            {
                l.InsertBefore(instInsertPoint, newInst);
            };
            o(l.Create(OpCodes.Ldc_I4, 1));
            o(l.Create(OpCodes.Ret));
        }

        void PatchIsForceSkip(MethodDefinition targetMethod)
        {
            Mono.Cecil.Rocks.MethodBodyRocks.SimplifyMacros(targetMethod.Body);

            // GameMain.Instance.IsForceSkip() の戻り値を捨てて、true を積む
            Instruction inst = targetMethod.Body.Instructions.FirstOrDefault(
                i => i.OpCode == OpCodes.Callvirt && i.Operand.ToString().Contains("GameMain::IsForceSkip"));
            if (inst == null)
            {
                return;
            }
            ILProcessor l = targetMethod.Body.GetILProcessor();
            Instruction instInsertPoint = inst.Next;
            InsertInstDelegate o = newInst =>
            {
                l.InsertBefore(instInsertPoint, newInst);
            };
            o(l.Create(OpCodes.Pop));
            o(l.Create(OpCodes.Ldc_I4, 1));
            Mono.Cecil.Rocks.MethodBodyRocks.OptimizeMacros(targetMethod.Body);
        }

        static MethodDefinition GetMethod(TypeDefinition type, string name)
        {
            return type.Methods.FirstOrDefault(m => m.Name == name);
        }
    }
}

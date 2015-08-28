using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEngine;

namespace CM3D2.ExternalSaveData.Patcher
{
    public class Patcher : ReiPatcher.Patch.PatchBase
    {
        public override string Version { get { return "0.1.2.0"; } }
        public override string Name { get { return "CM3D2.ExternalSaveData"; } }
        string patchTag { get { return Name + "_PATCHED"; } }

        delegate void InsertInstDelegate(Instruction newInst);
        ReiPatcher.Patch.PatcherArguments args;

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
            this.args = args;
            AssemblyDefinition ta = args.Assembly;
            AssemblyDefinition da = GetAssemblyDefinition(args, "CM3D2.ExternalSaveData.Managed.dll");
            string m = "CM3D2.ExternalSaveData.Managed.";
            SetHook(HookType.PostCall, ta, "GameMain", "OnInitialize", da, m + "ExSaveData", "DummyInitialize");
            SetHook(HookType.PostCall, ta, "GameMain", "Deserialize", da, m + "GameMainCallbacks.Deserialize", "Invoke");
            SetHook(HookType.PostCall, ta, "GameMain", "Serialize", da, m + "GameMainCallbacks.Serialize", "Invoke");
            SetHook(HookType.PostCall, ta, "GameMain", "DeleteSerializeData", da, m + "GameMainCallbacks.DeleteSerializeData", "Invoke");
            SetPatchedAttribute(args.Assembly, patchTag);
        }

        static AssemblyDefinition GetAssemblyDefinition(ReiPatcher.Patch.PatcherArguments args, string assemblyName)
        {
            var directoryName = Path.GetDirectoryName(args.Location);
            var filename = Path.Combine(directoryName, assemblyName);
            return AssemblyDefinition.ReadAssembly(filename);
        }

        static MethodDefinition GetMethod(TypeDefinition type, string name)
        {
            return type.Methods.FirstOrDefault(m => m.Name == name);
        }

        static void SetHook(
            HookType hookType,
            AssemblyDefinition targetAssembly, string targetType, string targetMethod,
            AssemblyDefinition dstAssembly, string dstType, string dstMethod)
        {
            MethodDefinition newMethod = GetMethod(dstAssembly.MainModule.GetType(dstType), dstMethod);
            if (newMethod == null)
            {
                Console.WriteLine("Error ({0}) : {1}.{2} is not found", dstAssembly.Name, dstType, dstMethod);
            }

            MethodDefinition srcMethod = GetMethod(targetAssembly.MainModule.GetType(targetType), targetMethod);
            if (srcMethod == null)
            {
                Console.WriteLine("Error ({0}) : {1}.{2} is not found", targetAssembly.Name, targetType, targetMethod);
            }
            HookMethod(hookType, targetAssembly.MainModule, srcMethod, newMethod);
        }

        static void HookMethod(
            HookType hookType,
            ModuleDefinition targetModule, MethodDefinition targetMethod,
            MethodDefinition calleeMethod)
        {
            ILProcessor l = targetMethod.Body.GetILProcessor();
            Instruction instInsertPoint = targetMethod.Body.Instructions.First();

            if (hookType == HookType.PostCall)
            {
                instInsertPoint = targetMethod.Body.Instructions.Last();
            }

            InsertInstDelegate o = newInst =>
            {
                l.InsertBefore(instInsertPoint, newInst);
            };

            int n = targetMethod.Parameters.Count + (targetMethod.IsStatic ? 0 : 1);
            for (int i = 0; i < n; i++)
            {
                if (i == 0)
                {
                    o(l.Create(OpCodes.Ldarg_0));
                }
                else
                {
                    // ref 参照にしたい場合は OpCodes.Ldarga にすること
                    o(l.Create(OpCodes.Ldarg, i));
                }
            }
            o(l.Create(OpCodes.Call, targetModule.Import(calleeMethod)));

            // PreJumpの場合は元の処理を行わないように、そのままRetする
            if (hookType == HookType.PreJump)
            {
                o(l.Create(OpCodes.Ret));
            }
        }

        enum HookType
        {
            PreJump,        // 元メソッドの先頭で、置き換え先メソッドへジャンプし、元のメソッドの処理は一切行わずに終了する
            PreCall,        // 元メソッドの先頭で、置き換え先メソッドをコールし、その後通常通り元のメソッドの処理を行う
            PostCall,       // 元メソッドの処理が完了したあと、リターンする直前で置き換え先メソッドを呼び出す
        }
    }
}

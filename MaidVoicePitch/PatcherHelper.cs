using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Linq;

internal static class PatcherHelper
{
    public delegate void InsertInstDelegate(Instruction newInst);

    public static AssemblyDefinition GetAssemblyDefinition(ReiPatcher.Patch.PatcherArguments args, string assemblyName)
    {
        var directoryName = Path.GetDirectoryName(args.Location);
        var filename = Path.Combine(directoryName, assemblyName);
        AssemblyDefinition ad = AssemblyDefinition.ReadAssembly(filename);
        if (ad == null)
        {
            Console.WriteLine("{0} not found", assemblyName);
            throw new Exception();
        }
        return ad;
    }

    public static MethodDefinition GetMethod(TypeDefinition type, string methodName)
    {
        return type.Methods.FirstOrDefault(m => m.Name == methodName);
    }

    public static MethodDefinition GetMethod(TypeDefinition type, string methodName, params string[] args)
    {
        for (int i = 0; i < type.Methods.Count; i++)
        {
            MethodDefinition m = type.Methods[i];
            if (m.Name == methodName && m.Parameters.Count == args.Length)
            {
                bool b = true;
                for (int j = 0; j < args.Length; j++)
                {
                    if (m.Parameters[j].ParameterType.FullName != args[j])
                    {
                        b = false;
                        break;
                    }
                }
                if (b)
                {
                    return m;
                }
            }
        }
        return null;
    }

    public static void SetHook(
        HookType hookType,
        AssemblyDefinition targetAssembly, string targetTypeName, string targetMethodName,
        AssemblyDefinition calleeAssembly, string calleeTypeName, string calleeMethodName)
    {
        TypeDefinition calleeTypeDefinition = calleeAssembly.MainModule.GetType(calleeTypeName);
        if (calleeTypeDefinition == null)
        {
            Console.WriteLine("Error ({0}) : {1} is not found", calleeAssembly.Name, calleeTypeName);
            throw new Exception();
        }

        MethodDefinition calleeMethod = GetMethod(calleeTypeDefinition, calleeMethodName);
        if (calleeMethod == null)
        {
            Console.WriteLine("Error ({0}) : {1}.{2} is not found", calleeAssembly.Name, calleeTypeName, calleeMethodName);
            throw new Exception();
        }

        TypeDefinition targetTypeDefinition = targetAssembly.MainModule.GetType(targetTypeName);
        if (targetTypeDefinition == null)
        {
            Console.WriteLine("Error ({0}) : {1} is not found", targetAssembly.Name, targetTypeName);
            throw new Exception();
        }

        MethodDefinition targetMethod = GetMethod(targetTypeDefinition, targetMethodName);
        if (targetMethod == null)
        {
            Console.WriteLine("Error ({0}) : {1}.{2} is not found", targetAssembly.Name, targetTypeName, targetMethodName);
            throw new Exception();
        }
        HookMethod(hookType, targetAssembly.MainModule, targetMethod, calleeMethod);
    }

    public static void HookMethod(
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

    public enum HookType
    {
        PreJump,        // 元メソッドの先頭で、置き換え先メソッドへジャンプし、元のメソッドの処理は一切行わずに終了する
        PreCall,        // 元メソッドの先頭で、置き換え先メソッドをコールし、その後通常通り元のメソッドの処理を行う
        PostCall,       // 元メソッドの処理が完了したあと、リターンする直前で置き換え先メソッドを呼び出す
    }
}

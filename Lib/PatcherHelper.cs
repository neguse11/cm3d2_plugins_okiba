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
            throw new Exception(string.Format("{0} not found", assemblyName));
        }
        return ad;
    }

    public static MethodDefinition GetMethod(TypeDefinition type, string methodName)
    {
        return type.Methods.FirstOrDefault(m => m.Name == methodName);
    }

    public static void DumpMethods(TextWriter tw, TypeDefinition type)
    {
        foreach (MethodDefinition m in type.Methods)
        {
            tw.Write("{0} . {1}(", type.Name, m.Name);
            bool b = true;
            foreach (var p in m.Parameters)
            {
                if (!b)
                {
                    tw.Write(",");
                }
                b = false;
                tw.Write("{0}", p.ParameterType.FullName);
            }
            tw.WriteLine(")");
        }
    }

    public static MethodDefinition GetMethod(TypeDefinition type, string methodName, params string[] args)
    {
        if (args == null)
        {
            return GetMethod(type, methodName);
        }
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
        AssemblyDefinition targetAssembly, string targetName,
        AssemblyDefinition calleeAssembly, string calleeName)
    {
        int i0 = targetName.LastIndexOf('.');
        if (i0 < 0)
        {
            throw new Exception(string.Format("SetHook - Error : Bad Name ({0})", targetName));
        }
        string targetTypeName = targetName.Substring(0, i0);
        string targetMethodName = targetName.Substring(i0 + 1);

        int i1 = calleeName.LastIndexOf('.');
        if (i1 < 0)
        {
            throw new Exception(string.Format("SetHook - Error : Bad Name ({0})", calleeName));
        }
        string calleeTypeName = calleeName.Substring(0, i1);
        string calleeMethodName = calleeName.Substring(i1 + 1);

        SetHook(hookType,
                targetAssembly, targetTypeName, targetMethodName,
                calleeAssembly, calleeTypeName, calleeMethodName);
    }

    public static void SetHook(
        HookType hookType,
        AssemblyDefinition targetAssembly, string targetName, string[] targetArgTypes,
        AssemblyDefinition calleeAssembly, string calleeName, string[] calleeArgTypes)
    {
        int i0 = targetName.LastIndexOf('.');
        if (i0 < 0)
        {
            throw new Exception(string.Format("SetHook - Error : Bad Name ({0})", targetName));
        }
        string targetTypeName = targetName.Substring(0, i0);
        string targetMethodName = targetName.Substring(i0 + 1);

        int i1 = calleeName.LastIndexOf('.');
        if (i1 < 0)
        {
            throw new Exception(string.Format("SetHook - Error : Bad Name ({0})", calleeName));
        }
        string calleeTypeName = calleeName.Substring(0, i1);
        string calleeMethodName = calleeName.Substring(i1 + 1);

        SetHook(hookType,
                targetAssembly, targetTypeName, targetMethodName, targetArgTypes,
                calleeAssembly, calleeTypeName, calleeMethodName, calleeArgTypes);
    }

    public static void SetHook(
        HookType hookType,
        AssemblyDefinition targetAssembly, string targetTypeName, string targetMethodName,
        AssemblyDefinition calleeAssembly, string calleeTypeName, string calleeMethodName)
    {
#if DEBUG
        Console.WriteLine("SetHook - {0}/{1}|{2} -> {3}/{4}|{5}", targetAssembly.Name.Name, targetTypeName, targetMethodName, calleeAssembly.Name.Name, calleeTypeName, calleeMethodName);
#endif
        TypeDefinition calleeTypeDefinition = calleeAssembly.MainModule.GetType(calleeTypeName);
        if (calleeTypeDefinition == null)
        {
            throw new Exception(string.Format("Error ({0}) : {1} is not found", calleeAssembly.Name, calleeTypeName));
        }

        MethodDefinition calleeMethod = GetMethod(calleeTypeDefinition, calleeMethodName);
        if (calleeMethod == null)
        {
            throw new Exception(string.Format("Error ({0}) : {1}.{2} is not found", calleeAssembly.Name, calleeTypeName, calleeMethodName));
        }

        TypeDefinition targetTypeDefinition = targetAssembly.MainModule.GetType(targetTypeName);
        if (targetTypeDefinition == null)
        {
            throw new Exception(string.Format("Error ({0}) : {1} is not found", targetAssembly.Name, targetTypeName));
        }

        MethodDefinition targetMethod = GetMethod(targetTypeDefinition, targetMethodName);
        if (targetMethod == null)
        {
            throw new Exception(string.Format("Error ({0}) : {1}.{2} is not found", targetAssembly.Name, targetTypeName, targetMethodName));
        }
        HookMethod(hookType, targetAssembly.MainModule, targetMethod, calleeMethod);
    }

    public static void SetHook(
        HookType hookType,
        AssemblyDefinition targetAssembly, string targetTypeName, string targetMethodName, string[] targetArgTypes,
        AssemblyDefinition calleeAssembly, string calleeTypeName, string calleeMethodName, string[] calleeArgTypes)
    {
#if DEBUG
        Console.WriteLine("SetHook - {0}/{1}|{2}({3}) -> {4}/{5}|{6}({7})",
                          targetAssembly.Name.Name, targetTypeName, targetMethodName, string.Join(",", targetArgTypes),
                          calleeAssembly.Name.Name, calleeTypeName, calleeMethodName, string.Join(",", calleeArgTypes));
#endif
        TypeDefinition calleeTypeDefinition = calleeAssembly.MainModule.GetType(calleeTypeName);
        if (calleeTypeDefinition == null)
        {
            throw new Exception(string.Format("Error ({0}) : {1} is not found", calleeAssembly.Name, calleeTypeName));
        }

        MethodDefinition calleeMethod = GetMethod(calleeTypeDefinition, calleeMethodName, calleeArgTypes);
        if (calleeMethod == null)
        {
            throw new Exception(string.Format("Error ({0}) : {1}.{2} is not found", calleeAssembly.Name, calleeTypeName, calleeMethodName));
        }

        TypeDefinition targetTypeDefinition = targetAssembly.MainModule.GetType(targetTypeName);
        if (targetTypeDefinition == null)
        {
            throw new Exception(string.Format("Error ({0}) : {1} is not found", targetAssembly.Name, targetTypeName));
        }

        MethodDefinition targetMethod = GetMethod(targetTypeDefinition, targetMethodName, targetArgTypes);
        if (targetMethod == null)
        {
            throw new Exception(string.Format("Error ({0}) : {1}.{2} is not found", targetAssembly.Name, targetTypeName, targetMethodName));
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

        if (hookType == HookType.PostCall || hookType == HookType.PostCallRet)
        {
            instInsertPoint = targetMethod.Body.Instructions.Last();
        }

        InsertInstDelegate o = newInst =>
        {
            l.InsertBefore(instInsertPoint, newInst);
        };

        // todo 2があるとは限らないので、良い方法を探す
        int tmpLoc = 2;
        if (hookType == HookType.PostCallRet)
        {
            // 戻り値をテンポラリにコピー
            o(l.Create(OpCodes.Dup));           // 最後の ret 用にコピーを作る
            o(l.Create(OpCodes.Stloc, tmpLoc));
        }

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
        if (hookType == HookType.PostCallRet)
        {
            // 戻り値をテンポラリからスタックへコピー
            o(l.Create(OpCodes.Ldloc, tmpLoc));
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
        PostCallRet,    // 元メソッドの処理が完了したあと、リターンする直前で置き換え先メソッドを呼び出す。置き換えメソッドの最後の引数には戻り値を入れる
    }
}

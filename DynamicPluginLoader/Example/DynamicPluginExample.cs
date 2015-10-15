using CM3D2.DynamicLoader.Plugin;
using UnityEngine;
using System;
using System.Reflection;

// 以下の AssemblyVersion は削除しないこと
[assembly: AssemblyVersion("1.0.*")]

class DynamicPluginExample : DynamicPluginBase
{
    int counter = 0;

    public DynamicPluginExample()
    {
        Console.WriteLine("{0} : ctor()", GetType().Name);
    }

    public override void OnPluginLoad()
    {
        Console.WriteLine("{0} : OnPluginLoad()", GetType().Name);
    }

    public override void OnPluginUnload()
    {
        Console.WriteLine("{0} : OnPluginUnload()", GetType().Name);
    }

    public override void Update()
    {
        counter++;
    }

    public override void OnGUI()
    {
        var rect = new Rect(0, 128, 256, 64);
        var str = string.Format("{0} : counter={1}", GetType().Name, counter);
        GUI.Label(rect, str);
    }
}

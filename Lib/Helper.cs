using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;

internal static class Helper
{
    static StreamWriter logStreamWriter = null;
    public static readonly DateTime now = DateTime.Now;

#if DEBUG
    public static bool bLogEnable = true;
#else
    public static bool bLogEnable = false;
#endif

    public static void Log(string s)
    {
        if (!bLogEnable)
        {
            return;
        }
        if (logStreamWriter == null)
        {
            string fname = (@".\Log_" + now.ToString("yyyyMMdd_HHmmss") + ".log");
            logStreamWriter = new StreamWriter(fname, true);
        }
        logStreamWriter.Write(s);
        logStreamWriter.Write("\n");
        logStreamWriter.Flush();
    }

    public static void Log(string format, params object[] args)
    {
        if (!bLogEnable)
        {
            return;
        }
        Log(string.Format(format, args));
    }

    public static bool StringToBool(string s, bool defaultValue)
    {
        bool v;
        if (s == null)
        {
            return defaultValue;
        }
        if (bool.TryParse(s, out v))
        {
            return v;
        }
        float f;
        if (float.TryParse(s, out f))
        {
            return f > 0.5f;
        }
        int i;
        if (int.TryParse(s, out i))
        {
            return i > 0;
        }
        return defaultValue;
    }

    public static int StringToInt(string s, int defaultValue)
    {
        int v;
        if (s == null || !int.TryParse(s, out v))
        {
            v = defaultValue;
        }
        return v;
    }

    //  public static float FloatTryParse(string s, float defaultValue)
    public static float StringToFloat(string s, float defaultValue)
    {
        float v;
        if (s == null || !float.TryParse(s, out v))
        {
            v = defaultValue;
        }
        return v;
    }

    public static XmlDocument LoadXmlDocument(string xmlFilePath)
    {
        XmlDocument xml = new XmlDocument();
        try
        {
            if (File.Exists(xmlFilePath))
            {
                xml.Load(xmlFilePath);
            }
        }
        catch (Exception e)
        {
            Helper.ShowException(e);
        }
        return xml;
    }

    // http://stackoverflow.com/a/1082587/2132223
    public static TEnum ToEnum<TEnum>(this string strEnumValue, TEnum defaultValue)
    {
        if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
            return defaultValue;
        return (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
    }

    // http://stackoverflow.com/a/3303182/2132223
    public static FieldInfo GetFieldInfo(Type type, string fieldName)
    {
        return type.GetField(fieldName,
                             BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
    }

    public static object GetInstanceField(Type type, object instance, string fieldName)
    {
        FieldInfo field = GetFieldInfo(type, fieldName);
        return field == null ? null : field.GetValue(instance);
    }

    public static void SetInstanceField(Type type, object instance, string fieldName, object val)
    {
        FieldInfo field = GetFieldInfo(type, fieldName);
        if (field != null)
        {
            field.SetValue(instance, val);
        }
    }

    public static void ShowStackFrames(StackFrame[] stackFrames)
    {
        foreach (StackFrame f in stackFrames)
        {
            Console.WriteLine(
                "{0}({1}.{2}) : {3}.{4}",
                f.GetFileName(), f.GetFileLineNumber(), f.GetFileColumnNumber(),
                f.GetMethod().DeclaringType, f.GetMethod()
            );
        }
    }

    public static void ShowException(Exception ex)
    {
        Console.WriteLine("{0}", ex.Message);
        StackTrace st = new StackTrace(ex, true);
        ShowStackFrames(st.GetFrames());
    }

    // http://stackoverflow.com/a/909583/2132223
    public static System.Reflection.Assembly GetCurrentAssembly()
    {
        return System.Reflection.Assembly.GetExecutingAssembly();
    }

    public static FileVersionInfo GetCurrentAssemblyFileVersionInfo()
    {
        return FileVersionInfo.GetVersionInfo(GetCurrentAssembly().Location);
    }
}

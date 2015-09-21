using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace VoiceTextList
{
    class Program
    {
        class Entry
        {
            public string ScriptFileName = "";  // arc内ファイル名
            public int ScriptLineNumber = 0;    // 行番号
            public string VoiceName = "";       // ボイスファイル名
            public string VoiceText = "";       // ボイステキスト
            public string Label = "";           // ラベル
            public string SubLabel = "";        // 「Lxx」形式のラベル
            public string MotionFile = "";      // @MotionScript file="XXX"
            public string MotionLabel = "";     // @MotionScript label=XXX
            public string Face = "";            // @face name=XXX
            public string FaceBlend = "";       // @faceblend name=XXX
            public bool Repeat = false;         // @talkRepeatなら true
            public string ArcName = "";         // arcファイル名

            static Dictionary<string, string> PrefixToPersonalTable = new Dictionary<string, string>()
            {
                { "N0", "秘書" },
                { "S0", "ツンデレ" },
                { "S1", "クーデレ" },
                { "S2", "純真" },
            };

            public int CompareTo(Entry rhs)
            {
                return this.VoiceName.CompareTo(rhs.VoiceName);
            }

            public static string GetCsvHeader()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"Name\"");
                sb.Append(",\"Text\"");
                sb.Append(",\"Script\"");
                sb.Append(",\"Line Number\"");
                sb.Append(",\"Label\"");
                sb.Append(",\"SubLabel\"");
                sb.Append(",\"Motion File\"");
                sb.Append(",\"Motion Label\"");
                sb.Append(",\"Face\"");
                sb.Append(",\"FaceBlend\"");
                sb.Append(",\"Repeat\"");
                sb.Append(",\"Personality\"");
                sb.Append(",\"Arc\"");
                return sb.ToString();
            }

            public string ToCsvLine()
            {
                string Personality = "";
                PrefixToPersonalTable.TryGetValue(VoiceName.Substring(0, 2), out Personality);

                StringBuilder sb = new StringBuilder();
                sb.Append(string.Format("\"{0}\"", VoiceName));
                sb.Append(string.Format(",\"{0}\"", VoiceText));
                sb.Append(string.Format(",\"{0}\"", ScriptFileName));
                sb.Append(string.Format(",\"{0}\"", ScriptLineNumber));
                sb.Append(string.Format(",\"{0}\"", Label));
                sb.Append(string.Format(",\"{0}\"", SubLabel));
                sb.Append(string.Format(",\"{0}\"", MotionFile));
                sb.Append(string.Format(",\"{0}\"", MotionLabel));
                sb.Append(string.Format(",\"{0}\"", Face));
                sb.Append(string.Format(",\"{0}\"", FaceBlend));
                sb.Append(string.Format(",\"{0}\"", Repeat));
                sb.Append(string.Format(",\"{0}\"", Personality));
                sb.Append(string.Format(",\"{0}\"", ArcName));
                return sb.ToString();
            }
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("VoiceTextList <output csv file>");
                return;
            }
            proc(args[0]);
        }

        static void proc(string dstFileName)
        {
            // 結果の格納先
            List<Entry> entries = new List<Entry>();

            // 全arcファイルを処理
            procAllArcs(
                // 処理したいファイル名なら true を返して、↓のファイル処理を行う
                //
                // arcPath : 処理対象のarcアーカイブ名 (例:"C:\KISS\CM3D2\GameData\script.arc")
                // filename : 処理対象のarc内ファイル名 (例:"daily\a_tun\com\a_com0001.ks")
                //
                (string arcPath, string filename) =>
                {
                    return filename.EndsWith(".ks");
                },

                // .arc内のファイルの内容を処理
                //
                // arcPath : 処理対象のarcアーカイブ名 (例:"C:\KISS\CM3D2\GameData\script.arc")
                // filename : 処理対象のarc内ファイル名 (例:"daily\a_tun\com\a_com0001.ks")
                // file : 処理対象のarc内ファイル (バイナリ)
                //
                (string arcPath, string filename, byte[] file) =>
                {
                    string arcName = Path.GetFileName(arcPath);
                    string text = System.Text.Encoding.GetEncoding(932).GetString(file);
                    string[] lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                    Entry entry = new Entry();

                    Action reset = () =>
                    {
                        entry.Label = "";
                        entry.SubLabel = "";
                        entry.MotionFile = "";
                        entry.MotionLabel = "";
                        entry.Face = "";
                        entry.FaceBlend = "";
                        entry.Repeat = false;
                    };
                    reset();

                    Regex motionScriptPattern = new Regex(@"^\s*@MotionScript file=""?([^"" ]*)""? label=""?([^"" ]*)""?", RegexOptions.IgnoreCase);
                    Regex facePattern = new Regex(@"^\s*@face maid=([^ ]*) name=""?([^"" ]*)""?", RegexOptions.IgnoreCase);
                    Regex faceBlendPattern = new Regex(@"^\s@faceblend maid=([^ ]*) name=""?([^"" ]*)""?", RegexOptions.IgnoreCase);
                    Regex subLabelPattern = new Regex(@"^\s*\*(L[^ |]*)", RegexOptions.IgnoreCase);
                    Regex labelPattern = new Regex(@"^\s*\*([^L][^ \*|]*)", RegexOptions.IgnoreCase);
                    Regex talkPattern = new Regex(@"^\s*@talk voice=([^ ]*)", RegexOptions.IgnoreCase);
                    Regex talkRepeatPattern = new Regex(@"^\s*@talkRepeat voice=([^ ]*)", RegexOptions.IgnoreCase);
                    Regex rReturnPattern = new Regex(@"^\s*@R_return", RegexOptions.IgnoreCase);
                    Regex returnPattern = new Regex(@"\s*@return", RegexOptions.IgnoreCase);

                    for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
                    {
                        string line = lines[lineNumber];
                        if (string.IsNullOrEmpty(entry.VoiceName))
                        {
                            Match m;
                            if ((m = motionScriptPattern.Match(line)).Success)
                            {
                                if (m.Groups.Count >= 2)
                                {
                                    entry.MotionFile = m.Groups[1].Value;
                                }
                                if (m.Groups.Count >= 3)
                                {
                                    entry.MotionLabel = m.Groups[2].Value;
                                }
                            }
                            else if ((m = facePattern.Match(line)).Groups.Count >= 3)
                            {
                                entry.Face = m.Groups[2].Value;
                            }
                            else if ((m = faceBlendPattern.Match(line)).Groups.Count >= 3)
                            {
                                entry.FaceBlend = m.Groups[2].Value;
                            }
                            else if ((m = subLabelPattern.Match(line)).Groups.Count >= 2)
                            {
                                entry.SubLabel = m.Groups[1].Value;
                            }
                            else if ((m = labelPattern.Match(line)).Groups.Count >= 2)
                            {
                                entry.Label = m.Groups[1].Value;
                            }
                            else if ((m = talkPattern.Match(line)).Groups.Count >= 2)
                            {
                                entry.VoiceName = m.Groups[1].Value;
                                entry.Repeat = false;
                            }
                            else if ((m = talkRepeatPattern.Match(line)).Groups.Count >= 2)
                            {
                                entry.VoiceName = m.Groups[1].Value;
                                entry.Repeat = true;
                            }
                            else if ((m = rReturnPattern.Match(line)).Groups.Count >= 2)
                            {
                                reset();
                            }
                            else if ((m = returnPattern.Match(line)).Groups.Count >= 2)
                            {
                                reset();
                            }
                        }
                        else
                        {
                            entry.ScriptFileName = filename;
                            entry.ScriptLineNumber = lineNumber + 1;
                            entry.VoiceText = line;
                            entry.ArcName = arcName;

                            entries.Add(entry);
                            Entry oldEntry = entry;

                            entry = new Entry()
                            {
                                Label = oldEntry.Label,
                                SubLabel = oldEntry.SubLabel,
                                MotionFile = oldEntry.MotionFile,
                                MotionLabel = oldEntry.MotionLabel,
                                Face = oldEntry.Face,
                                FaceBlend = oldEntry.FaceBlend
                            };
                        }
                    }
                }
            );

            // てきとうにソートしてCSVを出す
            entries.Sort((lhs, rhs) => lhs.CompareTo(rhs));
            using (StreamWriter sw = new StreamWriter(File.Open(dstFileName, FileMode.Create), Encoding.Unicode))
            {
                sw.WriteLine(Entry.GetCsvHeader());
                foreach (Entry e in entries)
                {
                    sw.WriteLine(e.ToCsvLine());
                }
            }
        }

        // GameData下の全arcファイルを取得して処理する
        static void procAllArcs(Func<string, string, bool> predicate, Action<string, string, byte[]> filter)
        {
            string gameDataPath = Path.Combine(Cm3d2Dll.GetInstallPath(), "GameData");
            string[] arcPaths = Directory.GetFiles(gameDataPath, "*.arc");
            foreach (string arcPath in arcPaths)
            {
                using (Cm3d2Dll dll = new Cm3d2Dll())
                {
                    dll.ProcArchive(arcPath, (ref Cm3d2Dll.FSDATA fsdata, string[] filenames) =>
                    {
                        foreach (string filename in filenames)
                        {
                            if (predicate(arcPath, filename))
                            {
                                byte[] file = dll.GetFile(ref fsdata, filename);
                                filter(arcPath, filename, file);
                            }
                        }
                    });
                }
            }
        }
    }
}


// http://blogs.msdn.com/b/jonathanswift/archive/2006/10/03/dynamically-calling-an-unmanaged-dll-from-.net-_2800_c_23002900_.aspx
// http://qiita.com/exliko/items/e458c26a2e2389580872
// http://stackoverflow.com/questions/538060/
public class UnmanagedDll : System.IDisposable
{
    static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string lpszLibFile);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpszProc);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
    }

    protected string DllPath { get; set; }
    protected System.IntPtr ModuleHandle { get; set; }
    protected bool Disposed { get; set; }

    public bool IsModuleReady
    {
        get
        {
            return ModuleHandle != IntPtr.Zero;
        }
    }

    protected UnmanagedDll()
    {
        DllPath = "";
        Disposed = false;
        ModuleHandle = System.IntPtr.Zero;
    }

    protected UnmanagedDll(string dllPath) : this()
    {
        Load(dllPath);
    }

    ~UnmanagedDll()
    {
        Dispose(false);
    }

    public void Dispose()
    {
#if DEBUG
        Console.WriteLine("UnmanagedDll.Dispose() : {0}", DllPath);
#endif
        Dispose(true);
        System.GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
#if DEBUG
        Console.WriteLine("UnmanagedDll.Dispose({0}) : {1}", disposing, DllPath);
#endif
        if (!Disposed)
        {
            Disposed = true;
            Unload();
        }
    }

    protected bool Load(string dllPath)
    {
#if DEBUG
        Console.WriteLine("UnmanagedDll.Load({0})", dllPath);
#endif
        if (IsModuleReady)
        {
            throw new System.InvalidOperationException("DLL is already loaded : " + DllPath);
        }

        DllPath = dllPath;
        if (!System.IO.File.Exists(DllPath))
        {
            throw new System.IO.FileNotFoundException("DLL not found : " + DllPath);
        }

        ModuleHandle = NativeMethods.LoadLibrary(dllPath);
        if (ModuleHandle == System.IntPtr.Zero)
        {
            throw new System.IO.FileNotFoundException("DLL loading failed : " + DllPath);
        }

        return IsModuleReady;
    }

    protected bool Unload()
    {
        if (!IsModuleReady)
        {
            return false;
        }
        bool result = NativeMethods.FreeLibrary(ModuleHandle);
        ModuleHandle = IntPtr.Zero;
#if DEBUG
        Console.WriteLine("UnmanagedDll.Unload({0})", DllPath);
#endif
        return result;
    }

    public T GetProcAddress<T>() where T : class
    {
        string funcName = typeof(T).Name;

        if (!IsModuleReady)
        {
            throw new System.InvalidOperationException("DLL not ready : " + DllPath + "/" + funcName);
        }

        System.IntPtr procAddress = NativeMethods.GetProcAddress(ModuleHandle, funcName);
        if (procAddress == System.IntPtr.Zero || procAddress == null)
        {
            throw new System.NotImplementedException("DLL function not found : " + DllPath + "/" + funcName);
        }

        System.Delegate func = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer(procAddress, typeof(T));

        return func as T;
    }
}



class Cm3d2Dll : UnmanagedDll
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FSDATA
    {
        public IntPtr object_pointer;
        public Int32 type;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FILEDATA
    {
        public IntPtr object_pointer;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LISTDATA
    {
        public Int32 size;
        public IntPtr data;
    }

    public delegate void ProcArchiveFunc(ref FSDATA fsdata, string[] filenames);

    public Cm3d2Dll()
    {
        Load(GetDllPath());
    }

    public static string GetDllPath()
    {
        {
            string x64 = GetDllPath("x64");
            if (System.IO.File.Exists(x64))
            {
                return x64;
            }
        }
        {
            string x86 = GetDllPath("x86");
            if (System.IO.File.Exists(x86))
            {
                return x86;
            }
        }
        return null;
    }

    public static string GetDllPath(bool isX64)
    {
        return GetDllPath(isX64 ? "x64" : "x86");
    }

    public static string GetDllPath(string platform)
    {
        string installPath = GetInstallPath();
        string dllPath = Path.Combine(installPath, "CM3D2" + platform + @"_Data\Plugins\cm3d2_" + platform + ".dll");
        return dllPath;
    }

    public static string GetInstallPath()
    {
        string result = null;
        try
        {
            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\KISS\カスタムメイド3D2"))
            {
                if (key != null)
                {
                    Object o = key.GetValue("InstallPath");
                    if (o != null)
                    {
                        result = o as string;
                    }
                }
            }
        }
        catch (Exception)
        {
            // ?
        }
        return result;
    }

    public void ProcArchive(string arcPath, ProcArchiveFunc func)
    {
        FSDATA fsdata = new FSDATA();
        DLL_FileSystem_CreateFileSystemArchive(ref fsdata);
        DLL_FileSystem_AddArchive(ref fsdata, arcPath);

        string[] files = GetFiles(ref fsdata);
        func(ref fsdata, files);
        DLL_FileSystem_DeleteFileSystem(ref fsdata);
    }

    public string[] GetFiles(ref Cm3d2Dll.FSDATA fsdata)
    {
        var listdata = new LISTDATA();
        DLL_FileSystem_CreateList(ref fsdata, "", 3, ref listdata);

        var filenames = new string[listdata.size];
        for (int i = 0; i < listdata.size; i++)
        {
            string str = "";
            DLL_FileSystem_AtList(ref listdata, i, ref str);
            filenames[i] = str;
        }
        DLL_FileSystem_DeleteList(ref listdata);
        return filenames;
    }

    public byte[] GetFile(ref Cm3d2Dll.FSDATA fsdata, string filename)
    {
        var filedata = new Cm3d2Dll.FILEDATA();
        DLL_FileSystem_GetFile(ref fsdata, filename, ref filedata);

        var b0 = DLL_File_IsValid(ref filedata);
        var size = DLL_File_GetSize(ref filedata);

        var a = new byte[size];
        GCHandle pinnedArray = GCHandle.Alloc(a, GCHandleType.Pinned);
        IntPtr pointer = pinnedArray.AddrOfPinnedObject();
        var b1 = DLL_File_Read(ref filedata, pointer, size);
        pinnedArray.Free();

        DLL_File_CloseFile(ref filedata);
        return a;
    }

    public string[,] GetCsv(ref Cm3d2Dll.FSDATA fsdata, string filename)
    {
        var csvParser = DLL_CSV_CreateCsvParser();
        var filedata = new Cm3d2Dll.FILEDATA();
        DLL_FileSystem_GetFile(ref fsdata, filename, ref filedata);
        DLL_CSV_Open(csvParser, filedata.object_pointer);

        var w = DLL_CSV_GetMaxCellX(csvParser);
        var h = DLL_CSV_GetMaxCellY(csvParser);
        var a = new string[w, h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var buf = new byte[DLL_CSV_GetDataSizeBinary(csvParser, x, y)];
                DLL_CSV_GetCellAsBinary(csvParser, x, y, ref buf, buf.Length);
                a[x, y] = Encoding.GetEncoding(932).GetString(buf);
            }
        }

        DLL_File_CloseFile(ref filedata);
        DLL_CSV_DeleteCsvParser(csvParser);
        return a;
    }

    private static class Functions
    {
        public delegate void DLL_FileSystem_CreateFileSystemWindows(ref FSDATA fs);
        public delegate void DLL_FileSystem_CreateFileSystemArchive(ref FSDATA fs);
        public delegate void DLL_FileSystem_DeleteFileSystem(ref FSDATA fs);
        public delegate void DLL_FileSystem_AddArchive(ref FSDATA fs, [MarshalAsAttribute(UnmanagedType.LPStr)] string path);
        public delegate Int32 DLL_FileSystem_IsValid(ref FSDATA fs);
        public delegate Int32 DLL_FileSystem_IsExistentFile(ref FSDATA fs, byte[] utf8Path);

        public delegate Int32 DLL_FileSystem_GetFile(ref FSDATA fs, byte[] utf8Path, ref FILEDATA dest);
        public delegate bool DLL_File_IsValid(ref FILEDATA fd);
        public delegate bool DLL_File_SetSeek(ref FILEDATA fd, long seek, bool absolute_move);
        public delegate long DLL_File_Read(ref FILEDATA fd, IntPtr dest, long read_size);
        public delegate long DLL_File_GetTell(ref FILEDATA fd);
        public delegate long DLL_File_GetSize(ref FILEDATA fd);
        public delegate void DLL_File_CloseFile(ref FILEDATA fd);

        public delegate void DLL_FileSystem_CreateList(ref FSDATA fs, byte[] utf8Path, Int32 listType, ref LISTDATA ld);
        public delegate void DLL_FileSystem_DeleteList(ref LISTDATA ld);
        public delegate Int32 DLL_FileSystem_AtList(ref LISTDATA ld, Int32 atNum, [Out] byte[] dest, Int32 size);

        public delegate IntPtr DLL_CSV_CreateCsvParser();
        public delegate void DLL_CSV_DeleteCsvParser(IntPtr csvParser);
        public delegate Int32 DLL_CSV_Open(IntPtr csvParser, IntPtr fileObjPty);
        public delegate Int32 DLL_CSV_GetCellAsString(IntPtr csvParser, Int32 cellX, Int32 cellY, StringBuilder dest, Int32 destSize);
        public delegate Int32 DLL_CSV_GetCellAsBinary(IntPtr csvParser, Int32 cellX, Int32 cellY, [Out] byte[] dest, Int32 destSize);
        public delegate Int32 DLL_CSV_GetDataSizeBinary(IntPtr csvParser, Int32 cellX, Int32 cellY);
        public delegate Int32 DLL_CSV_GetDataSizeString(IntPtr csvParser, Int32 cellX, Int32 cellY);
        public delegate Int32 DLL_CSV_IsCellToExistData(IntPtr csvParser, Int32 cellX, Int32 cellY);
        public delegate Int32 DLL_CSV_GetMaxCellX(IntPtr csvParser);
        public delegate Int32 DLL_CSV_GetMaxCellY(IntPtr csvParser);
    }

    public void DLL_FileSystem_CreateFileSystemArchive(ref FSDATA fsdata)
    {
        GetProcAddress<Functions.DLL_FileSystem_CreateFileSystemArchive>()(ref fsdata);
    }

    public void DLL_FileSystem_DeleteFileSystem(ref FSDATA fsdata)
    {
        GetProcAddress<Functions.DLL_FileSystem_DeleteFileSystem>()(ref fsdata);
    }

    public bool DLL_File_IsValid(ref FILEDATA fd)
    {
        return GetProcAddress<Functions.DLL_File_IsValid>()(ref fd);
    }

    public bool DLL_File_SetSeek(ref FILEDATA fd, long seek, bool absolute_move)
    {
        return GetProcAddress<Functions.DLL_File_SetSeek>()(ref fd, seek, absolute_move);
    }

    public long DLL_File_Read(ref FILEDATA fd, IntPtr dest, long read_size)
    {
        return GetProcAddress<Functions.DLL_File_Read>()(ref fd, dest, read_size);
    }

    public long DLL_File_GetTell(ref FILEDATA fd)
    {
        return GetProcAddress<Functions.DLL_File_GetTell>()(ref fd);
    }

    public long DLL_File_GetSize(ref FILEDATA fd)
    {
        return GetProcAddress<Functions.DLL_File_GetSize>()(ref fd);
    }

    public void DLL_File_CloseFile(ref FILEDATA filedata)
    {
        GetProcAddress<Functions.DLL_File_CloseFile>()(ref filedata);
    }

    public Int32 DLL_FileSystem_IsValid(ref FSDATA fsdata)
    {
        return GetProcAddress<Functions.DLL_FileSystem_IsValid>()(ref fsdata);
    }

    public void DLL_FileSystem_AddArchive(ref FSDATA fsdata, string path)
    {
        GetProcAddress<Functions.DLL_FileSystem_AddArchive>()(ref fsdata, path);
    }

    public Int32 DLL_FileSystem_IsExistentFile(ref FSDATA fsdata, string path)
    {
        return GetProcAddress<Functions.DLL_FileSystem_IsExistentFile>()(ref fsdata, stringToNullTerminatedUtf8(path));
    }

    public Int32 DLL_FileSystem_GetFile(ref FSDATA fsdata, string path, ref FILEDATA dest)
    {
        return GetProcAddress<Functions.DLL_FileSystem_GetFile>()(ref fsdata, stringToNullTerminatedUtf8(path), ref dest);
    }

    public void DLL_FileSystem_CreateList(ref FSDATA fsdata, string path, Int32 listType, ref LISTDATA listData)
    {
        GetProcAddress<Functions.DLL_FileSystem_CreateList>()(ref fsdata, stringToNullTerminatedUtf8(path), listType, ref listData);
    }

    public void DLL_FileSystem_DeleteList(ref LISTDATA listData)
    {
        GetProcAddress<Functions.DLL_FileSystem_DeleteList>()(ref listData);
    }

    public Int32 DLL_FileSystem_AtList(ref LISTDATA listData, Int32 atNum, ref string dest)
    {
        var s = new byte[8192];
        var result = GetProcAddress<Functions.DLL_FileSystem_AtList>()(ref listData, atNum, s, 8192);
        if (result > 0)
        {
            dest = Encoding.UTF8.GetString(s, 0, result);
        }
        else
        {
            dest = "";
        }
        return result;
    }

    public IntPtr DLL_CSV_CreateCsvParser()
    {
        return GetProcAddress<Functions.DLL_CSV_CreateCsvParser>()();
    }

    public void DLL_CSV_DeleteCsvParser(IntPtr csvParser)
    {
        GetProcAddress<Functions.DLL_CSV_DeleteCsvParser>()(csvParser);
    }

    public Int32 DLL_CSV_Open(IntPtr csvParser, IntPtr fileObjPty)
    {
        return GetProcAddress<Functions.DLL_CSV_Open>()(csvParser, fileObjPty);
    }

    public Int32 DLL_CSV_GetCellAsString(IntPtr csvParser, Int32 cellX, Int32 cellY, ref string dest)
    {
        var s = new StringBuilder(8192);
        var result = GetProcAddress<Functions.DLL_CSV_GetCellAsString>()(csvParser, cellX, cellY, s, s.Capacity);
        if (result > 0)
        {
            dest = s.ToString(0, result);
        }
        return result;
    }

    public Int32 DLL_CSV_GetCellAsBinary(IntPtr csvParser, Int32 cellX, Int32 cellY, ref byte[] dest, Int32 size)
    {
        return GetProcAddress<Functions.DLL_CSV_GetCellAsBinary>()(csvParser, cellX, cellY, dest, size);
    }

    public Int32 DLL_CSV_GetDataSizeBinary(IntPtr csvParser, Int32 cellX, Int32 cellY)
    {
        return GetProcAddress<Functions.DLL_CSV_GetDataSizeBinary>()(csvParser, cellX, cellY);
    }

    public Int32 DLL_CSV_GetDataSizeString(IntPtr csvParser, Int32 cellX, Int32 cellY)
    {
        return GetProcAddress<Functions.DLL_CSV_GetDataSizeString>()(csvParser, cellX, cellY);
    }

    public Int32 DLL_CSV_IsCellToExistData(IntPtr csvParser, Int32 cellX, Int32 cellY)
    {
        return GetProcAddress<Functions.DLL_CSV_IsCellToExistData>()(csvParser, cellX, cellY);
    }

    public Int32 DLL_CSV_GetMaxCellX(IntPtr csvParser)
    {
        return GetProcAddress<Functions.DLL_CSV_GetMaxCellX>()(csvParser);
    }

    public Int32 DLL_CSV_GetMaxCellY(IntPtr csvParser)
    {
        return GetProcAddress<Functions.DLL_CSV_GetMaxCellY>()(csvParser);
    }

    static byte[] stringToNullTerminatedUtf8(string str)
    {
        byte[] utf8Path = Encoding.UTF8.GetBytes(str);
        byte[] nullTerminatedUtf8Path = new byte[utf8Path.Length + 1];
        utf8Path.CopyTo(nullTerminatedUtf8Path, 0);
        nullTerminatedUtf8Path[nullTerminatedUtf8Path.Length - 1] = 0;
        return nullTerminatedUtf8Path;
    }
}

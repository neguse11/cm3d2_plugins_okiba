// スライダー範囲拡大を指定するテンプレートファイル (テスト中)
using CM3D2.ExternalSaveData.Managed;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

internal static class FaceScriptTemplates
{
    class Cache : TemplateFiles<TemplateFile> { }
    static Cache faceScriptTemplates = new Cache();

    public static void Clear()
    {
        faceScriptTemplates.Clear();
    }

    public static string ProcFaceName(Maid maid, string PluginName, string faceName)
    {
        TemplateFile t = Get(maid, PluginName);
        if (t == null)
        {
            // Helper.Log("FaceScriptTemplates.ProcFaceName({0},{1},{2}) -> null", maid, PluginName, faceName);
            return faceName;
        }
        return t.ProcFaceName(faceName);
    }

    public static string ProcFaceBlendName(Maid maid, string PluginName, string faceBlendName)
    {
        TemplateFile t = Get(maid, PluginName);
        if (t == null)
        {
            // Helper.Log("FaceScriptTemplates.ProcFaceBlendName({0},{1},{2}) -> null", maid, PluginName, faceBlendName);
            return faceBlendName;
        }
        return t.ProcFaceBlendName(faceBlendName);
    }

    static TemplateFile Get(Maid maid, string PluginName)
    {
        return Get(ExSaveData.Get(maid, PluginName, "FACE_SCRIPT_TEMPLATE", null));
    }

    static TemplateFile Get(string fname)
    {
        TemplateFile t = faceScriptTemplates.Get(fname);
        // Helper.Log("FaceScriptTemplates.Get({0}) -> {1}", fname, t);
        return t;
    }

    class TemplateFile : ITemplateFile
    {
        public Dictionary<string, string> FaceBlends { get; set; }
        public Dictionary<string, string> Faces { get; set; }

        public TemplateFile()
        {
            Clear();
        }

        public void Clear()
        {
            FaceBlends = new Dictionary<string, string>();
            Faces = new Dictionary<string, string>();
        }

        public bool Load(string fname)
        {
            bool result = false;
            Clear();
            var xd = new XmlDocument();
            try
            {
                if (File.Exists(fname))
                {
                    xd.Load(fname);
                    foreach (XmlNode e in xd.SelectNodes("/facescripttemplate/faceblends/faceblend"))
                    {
                        FaceBlends[e.Attributes["key"].Value] = e.Attributes["value"].Value;
                    }
                    foreach (XmlNode e in xd.SelectNodes("/facescripttemplate/faces/face"))
                    {
                        Faces[e.Attributes["key"].Value] = e.Attributes["value"].Value;
                    }
                    // Helper.Log("FaceScriptTemplates.TemplateFile({0}) -> ok", fname);
                    result = true;
                }
            }
            catch (Exception e)
            {
                Helper.ShowException(e);
            }
            return result;
        }

        public string ProcFaceName(string faceName)
        {
            string s;
            if (Faces.TryGetValue(faceName, out s))
            {
                // Helper.Log("FaceScriptTemplates.TemplateFile.ProcFaceName({0}) -> {1}", faceName, s);
                return s;
            }
            // Helper.Log("FaceScriptTemplates.TemplateFile.ProcFaceName({0}) -> fail", faceName);
            return faceName;
        }

        public string ProcFaceBlendName(string faceBlendName)
        {
            string s;
            if (FaceBlends.TryGetValue(faceBlendName, out s))
            {
                // Helper.Log("FaceScriptTemplates.TemplateFile.ProcFaceBlendName({0}) -> {1}", faceBlendName, s);
                return s;
            }
            // Helper.Log("FaceScriptTemplates.TemplateFile.ProcFaceBlendName({0}) -> fail", faceBlendName);
            return faceBlendName;
        }
    }
}

// スライダー範囲拡大を指定するテンプレートファイル
using CM3D2.ExternalSaveData.Managed;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

internal static class SliderTemplates
{
    class Cache : TemplateFiles<SliderTemplate> { }
    static Cache sliderTemplates = new Cache();

    public static void Clear()
    {
        sliderTemplates.Clear();
    }

    public static void Update(string PluginName)
    {
        // エディット画面以外では何もせず終了
        if (UnityEngine.Application.loadedLevel != 5)
        {
            return;
        }
        CharacterMgr cm = GameMain.Instance.CharacterMgr;
        for (int i = 0, n = cm.GetStockMaidCount(); i < n; i++)
        {
            Maid maid = cm.GetStockMaid(i);
            Update(maid, PluginName);
        }
    }

    static void Update(Maid maid, string PluginName)
    {
        if (!maid.Visible)
        {
            return;
        }
        string fname = ExSaveData.Get(maid, PluginName, "SLIDER_TEMPLATE", null);
        SliderTemplate sliderTemplate = sliderTemplates.Get(fname);
        if (sliderTemplate != null && !sliderTemplate.IsLoaded)
        {
            sliderTemplate.WriteProps(maid);
            sliderTemplate.IsLoaded = true;
        }
    }

    class SliderTemplate : ITemplateFile
    {
        public class Slider
        {
            public float min;
            public float max;
        }

        public Dictionary<string, Slider> Sliders { get; set; }
        public bool IsLoaded = false;

        public SliderTemplate()
        {
            Clear();
        }

        void Clear()
        {
            Sliders = new Dictionary<string, Slider>();
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
                    foreach (XmlNode e in xd.SelectNodes("/slidertemplate/sliders/slider"))
                    {
                        Sliders[e.Attributes["name"].Value] = new Slider
                        {
                            min = Helper.StringToFloat(e.Attributes["min"].Value, 0f),
                            max = Helper.StringToFloat(e.Attributes["max"].Value, 100f)
                        };
                    }
                    // Helper.Log("SliderTemplates.SliderTemplate.Load({0}) -> ok", fname);
                    result = true;
                }
            }
            catch (Exception e)
            {
                Helper.ShowException(e);
            }
            return result;
        }

        public void WriteProps(Maid maid)
        {
            foreach (var kv in Sliders)
            {
                string name = kv.Key;
                Slider slider = kv.Value;
                MPN mpn = Helper.ToEnum<MPN>(name, MPN.null_mpn);
                if (mpn != MPN.null_mpn)
                {
                    MaidProp maidProp = maid.GetProp(mpn);
                    maidProp.min = (int)slider.min;
                    maidProp.max = (int)slider.max;
                }
            }
        }
    }
}

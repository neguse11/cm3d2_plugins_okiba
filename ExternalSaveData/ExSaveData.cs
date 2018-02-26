using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace CM3D2.ExternalSaveData.Managed
{
    public static class ExSaveData
    {
        static SaveDataPluginSettings saveDataPluginSettings = new SaveDataPluginSettings();

        // 拡張セーブデータの実体
        static SaveDataPluginSettings PluginSettings { get { return saveDataPluginSettings; } }

        // 通常のプラグイン名よりも前に処理するため、先頭に '.' をつけている。
        // 通常のプラグインは "CM3D2～" のように英数字から始まる名前をつけること
        const string CallbackName = ".CM3D2 ExternalSaveData";

        /// <summary>
        /// 拡張セーブデータ内の設定を得る(文字列)
        /// <para>指定した設定が存在しない場合はdefaultValueを返す</para>
        /// <seealso cref="GetBool"/>
        /// <seealso cref="GetInt"/>
        /// <seealso cref="GetFloat"/>
        /// </summary>
        /// <param name="maid">メイドインスタンス</param>
        /// <param name="pluginName">プラグイン名("CM3D2.Test.Plugin"など)</param>
        /// <param name="propName">プロパティ名</param>
        /// <param name="defaultValue">プロパティが存在しない場合に返すデフォルト値</param>
        /// <returns>設定文字列</returns>
        public static string Get(Maid maid, string pluginName, string propName, string defaultValue)
        {
            if (maid == null || pluginName == null || propName == null)
            {
                return defaultValue;
            }
            if (!Contains(maid))
            {
                SetMaid(maid);
            }
            return PluginSettings.Get((string)Helper.GetMaidStatusValue(maid, "guid"), pluginName, propName, defaultValue);
        }

        public static bool GetBool(Maid maid, string pluginName, string propName, bool defaultValue)
        {
            return Helper.StringToBool(Get(maid, pluginName, propName, null), defaultValue);
        }

        public static int GetInt(Maid maid, string pluginName, string propName, int defaultValue)
        {
            return Helper.StringToInt(Get(maid, pluginName, propName, null), defaultValue);
        }

        public static float GetFloat(Maid maid, string pluginName, string propName, float defaultValue)
        {
            return Helper.StringToFloat(Get(maid, pluginName, propName, null), defaultValue);
        }

        /// <summary>
        /// 拡張セーブデータへ設定を書き込む
        /// <seealso cref="SetBool"/>
        /// <seealso cref="SetInt"/>
        /// <seealso cref="SetFloat"/>
        /// </summary>
        /// <param name="maid">メイドインスタンス</param>
        /// <param name="pluginName">プラグイン名("CM3D2.Test.Plugin"など)</param>
        /// <param name="propName">プロパティ名</param>
        /// <param name="value">書き込む値</param>
        /// <param name="overwrite">trueなら常に上書き。falseなら設定が存在する場合は書き込みを行わない</param>
        /// <returns>true:書き込み成功。false:失敗</returns>
        public static bool Set(Maid maid, string pluginName, string propName, string value, bool overwrite)
        {
            if (maid == null || pluginName == null || propName == null)
            {
                return false;
            }
            if (!Contains(maid))
            {
                SetMaid(maid);
            }
            if (!overwrite)
            {
                if (Contains(maid, pluginName, propName))
                {
                    return false;
                }
            }
            return PluginSettings.Set((string)Helper.GetMaidStatusValue(maid, "guid"), pluginName, propName, value);
        }

        public static bool SetBool(Maid maid, string pluginName, string propName, bool value, bool overwrite)
        {
            return Set(maid, pluginName, propName, value.ToString(), overwrite);
        }

        public static bool SetInt(Maid maid, string pluginName, string propName, int value, bool overwrite)
        {
            return Set(maid, pluginName, propName, value.ToString(), overwrite);
        }

        public static bool SetFloat(Maid maid, string pluginName, string propName, float value, bool overwrite)
        {
            return Set(maid, pluginName, propName, value.ToString(), overwrite);
        }

        /// <summary>
        /// 拡張セーブデータへ設定を書き込む(常に上書き)
        /// </summary>
        /// <param name="maid">メイドインスタンス</param>
        /// <param name="pluginName">プラグイン名("CM3D2.Test.Plugin"など)</param>
        /// <param name="propName">プロパティ名</param>
        /// <param name="value">書き込む値</param>
        /// <returns>true:書き込み成功。false:失敗</returns>
        public static bool Set(Maid maid, string pluginName, string propName, string value)
        {
            return Set(maid, pluginName, propName, value, true);
        }

        public static bool SetBool(Maid maid, string pluginName, string propName, bool value)
        {
            return SetBool(maid, pluginName, propName, value, true);
        }

        public static bool SetInt(Maid maid, string pluginName, string propName, int value)
        {
            return SetInt(maid, pluginName, propName, value, true);
        }

        public static bool SetFloat(Maid maid, string pluginName, string propName, float value)
        {
            return SetFloat(maid, pluginName, propName, value, true);
        }

        /// <summary>
        /// 拡張セーブデータ内の設定を削除
        /// </summary>
        /// <param name="maid">メイドインスタンス</param>
        /// <param name="pluginName">プラグイン名("CM3D2.Test.Plugin"など)</param>
        /// <param name="propName">プロパティ名</param>
        /// <returns>true:削除に成功(設定が存在し、それを削除した)。false:失敗(設定が存在しないか、何らかのエラー)</returns>
        public static bool Remove(Maid maid, string pluginName, string propName)
        {
            if (maid == null || pluginName == null || propName == null)
            {
                return false;
            }
            return PluginSettings.Remove((string)Helper.GetMaidStatusValue(maid, "guid"), pluginName, propName);
        }

        /// <summary>
        /// 拡張セーブデータ内の設定の存在を調査
        /// </summary>
        /// <param name="maid">メイドインスタンス</param>
        /// <param name="pluginName">プラグイン名("CM3D2.Test.Plugin"など)</param>
        /// <param name="propName">プロパティ名</param>
        /// <returns>true:設定が存在。false:存在しない</returns>
        public static bool Contains(Maid maid, string pluginName, string propName)
        {
            if (maid == null || pluginName == null || propName == null)
            {
                return false;
            }
            if (!Contains(maid))
            {
                SetMaid(maid);
            }
            return PluginSettings.Contains((string)Helper.GetMaidStatusValue(maid, "guid"), pluginName, propName);
        }

        /// <summary>
        /// 拡張セーブデータ内のメイドの存在を確認
        /// </summary>
        /// <param name="maid">メイドインスタンス</param>
        /// <returns>true:指定したメイドが存在する。false:存在しない</returns>
        public static bool Contains(Maid maid)
        {
            if (maid == null)
            {
                return false;
            }
            return PluginSettings.ContainsMaid((string)Helper.GetMaidStatusValue(maid, "guid"));
        }

        /// <summary>
        /// 拡張セーブデータへメイドを追加
        /// </summary>
        /// <param name="maid">メイドインスタンス</param>
        /// <returns></returns>
        public static void SetMaid(Maid maid)
        {
            if (maid == null)
            {
                return;
            }
            var s = Helper.GetMaidStatus(maid);
            var t = s.GetType();
            string[] cm3d2keys = { "last_name", "first_name", "create_time" },  com3d2keys = { "lastName", "firstName", "creationTime" };
            string[] keys = t.GetProperty("lastName") != null ? com3d2keys : cm3d2keys;
            PluginSettings.SetMaid((string)t.GetProperty("guid").GetValue(s, null),
                                   (string)t.GetProperty(keys[0]).GetValue(s, null),
                                   (string)t.GetProperty(keys[1]).GetValue(s, null),
                                   (string)t.GetProperty(keys[2]).GetValue(s, null));
        }

        /// <summary>
        /// 与えたGUIDを持たないメイドを拡張セーブデータから削除
        /// </summary>
        /// <param name="guids">メイドGUIDリスト</param>
        /// <returns></returns>
        public static void CleanupMaids(List<string> guids)
        {
            PluginSettings.Cleanup(guids);
        }

        public static void CleanupMaids()
        {
            List<string> guids = new List<string>();
            CharacterMgr cm = GameMain.Instance.CharacterMgr;
            for (int i = 0, n = cm.GetStockMaidCount(); i < n; i++)
            {
                Maid maid = cm.GetStockMaid(i);
                guids.Add((string)Helper.GetMaidStatusValue(maid, "guid"));
            }
            CleanupMaids(guids);
        }

        /// <summary>
        /// 拡張セーブデータ内のグローバル設定を得る(文字列)
        /// <para>指定した設定が存在しない場合はdefaultValueを返す</para>
        /// <seealso cref="GlobalGetBool"/>
        /// <seealso cref="GlobalGetInt"/>
        /// <seealso cref="GlobalGetFloat"/>
        /// </summary>
        /// <param name="pluginName">プラグイン名("CM3D2.Test.Plugin"など)</param>
        /// <param name="propName">プロパティ名</param>
        /// <param name="defaultValue">プロパティが存在しない場合に返すデフォルト値</param>
        /// <returns>設定文字列</returns>
        public static string GlobalGet(string pluginName, string propName, string defaultValue)
        {
            if (pluginName == null || propName == null)
            {
                return defaultValue;
            }
            return PluginSettings.Get(SaveDataPluginSettings.GlobalMaidGuid, pluginName, propName, defaultValue);
        }

        public static bool GlobalGetBool(string pluginName, string propName, bool defaultValue)
        {
            return Helper.StringToBool(GlobalGet(pluginName, propName, null), defaultValue);
        }

        public static int GlobalGetInt(string pluginName, string propName, int defaultValue)
        {
            return Helper.StringToInt(GlobalGet(pluginName, propName, null), defaultValue);
        }

        public static float GlobalGetFloat(string pluginName, string propName, float defaultValue)
        {
            return Helper.StringToFloat(GlobalGet(pluginName, propName, null), defaultValue);
        }

        /// <summary>
        /// 拡張セーブデータへグローバル設定を書き込む
        /// <seealso cref="GlobalSetBool"/>
        /// <seealso cref="GlobalSetInt"/>
        /// <seealso cref="GlobalSetFloat"/>
        /// </summary>
        /// <param name="pluginName">プラグイン名("CM3D2.Test.Plugin"など)</param>
        /// <param name="propName">プロパティ名</param>
        /// <param name="value">書き込む値</param>
        /// <param name="overwrite">trueなら常に上書き。falseなら設定が存在する場合は書き込みを行わない</param>
        /// <returns>true:書き込み成功。false:失敗</returns>
        public static bool GlobalSet(string pluginName, string propName, string value, bool overwrite)
        {
            if (pluginName == null || propName == null)
            {
                return false;
            }
            if (!overwrite)
            {
                if (GlobalContains(pluginName, propName))
                {
                    return false;
                }
            }
            return PluginSettings.Set(SaveDataPluginSettings.GlobalMaidGuid, pluginName, propName, value);
        }

        public static bool GlobalSetBool(string pluginName, string propName, bool value, bool overwrite)
        {
            return GlobalSet(pluginName, propName, value.ToString(), overwrite);
        }

        public static bool GlobalSetInt(string pluginName, string propName, int value, bool overwrite)
        {
            return GlobalSet(pluginName, propName, value.ToString(), overwrite);
        }

        public static bool GlobalSetFloat(string pluginName, string propName, float value, bool overwrite)
        {
            return GlobalSet(pluginName, propName, value.ToString(), overwrite);
        }

        /// <summary>
        /// 拡張セーブデータへグローバル設定を書き込む(常に上書き)
        /// <seealso cref="GlobalSetBool"/>
        /// <seealso cref="GlobalSetInt"/>
        /// <seealso cref="GlobalSetFloat"/>
        /// </summary>
        /// <param name="pluginName">プラグイン名("CM3D2.Test.Plugin"など)</param>
        /// <param name="propName">プロパティ名</param>
        /// <param name="value">書き込む値</param>
        /// <returns>true:書き込み成功。false:失敗</returns>
        public static bool GlobalSet(string pluginName, string propName, string value)
        {
            return GlobalSet(pluginName, propName, value, true);
        }

        public static bool GlobalSetBool(string pluginName, string propName, bool value)
        {
            return GlobalSetBool(pluginName, propName, value, true);
        }

        public static bool GlobalSetInt(string pluginName, string propName, int value)
        {
            return GlobalSetInt(pluginName, propName, value, true);
        }

        public static bool GlobalSetFloat(string pluginName, string propName, float value)
        {
            return GlobalSetFloat(pluginName, propName, value, true);
        }

        /// <summary>
        /// 拡張セーブデータ内のグローバル設定を削除
        /// </summary>
        /// <param name="pluginName">プラグイン名("CM3D2.Test.Plugin"など)</param>
        /// <param name="propName">プロパティ名</param>
        /// <returns>true:削除に成功(設定が存在し、それを削除した)。false:失敗(設定が存在しないか、何らかのエラー)</returns>
        public static bool GlobalRemove(string pluginName, string propName)
        {
            if (pluginName == null || propName == null)
            {
                return false;
            }
            return PluginSettings.Remove(SaveDataPluginSettings.GlobalMaidGuid, pluginName, propName);
        }

        /// <summary>
        /// 拡張セーブデータ内のグローバル設定の存在を調査
        /// </summary>
        /// <param name="pluginName">プラグイン名("CM3D2.Test.Plugin"など)</param>
        /// <param name="propName">プロパティ名</param>
        /// <returns>true:設定が存在する。false:存在しない</returns>
        public static bool GlobalContains(string pluginName, string propName)
        {
            if (pluginName == null || propName == null)
            {
                return false;
            }
            return PluginSettings.Contains(SaveDataPluginSettings.GlobalMaidGuid, pluginName, propName);
        }

        /// <summary>
        /// セーブファイル番号から拡張セーブデータのファイル名を生成
        /// </summary>
        /// <param name="that">GameMainインスタンス</param>
        /// <param name="f_nSaveNo">セーブファイル番号</param>
        /// <returns>拡張セーブデータのファイル名</returns>
        public static string makeXmlFilename(GameMain that, int f_nSaveNo)
        {
            return GameMainMakeSavePathFileName(that, f_nSaveNo) + ".exsave.xml";
        }

        /// <summary>
        /// セーブファイル番号からセーブデータのファイル名を生成 (GameMain.MakeSavePathFileNameを呼び出す)
        /// </summary>
        /// <param name="that">GameMainインスタンス</param>
        /// <param name="f_nSaveNo">セーブファイル番号</param>
        /// <returns>セーブデータのファイル名</returns>
        public static string GameMainMakeSavePathFileName(GameMain that, int f_nSaveNo)
        {
            // class GameMain { private string MakeSavePathFileName(int f_nSaveNo); }
            MethodInfo methodInfo = typeof(GameMain).GetMethod(
                "MakeSavePathFileName",
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(int) },
                null
            );
            return (string)(methodInfo.Invoke(that, new object[] { f_nSaveNo }));
        }

        static bool SetMaidName(Maid maid)
        {
            if (maid == null)
            {
                return false;
            }
            var s = Helper.GetMaidStatus(maid);
            var t = s.GetType();
            string[] cm3d2keys = { "last_name", "first_name", "create_time" },  com3d2keys = { "lastName", "firstName", "creationTime" };
            string[] keys = t.GetProperty("lastName") != null ? com3d2keys : cm3d2keys;
            return PluginSettings.SetMaidName((string)t.GetProperty("guid").GetValue(s, null),
                                              (string)t.GetProperty(keys[0]).GetValue(s, null),
                                              (string)t.GetProperty(keys[1]).GetValue(s, null),
                                              (string)t.GetProperty(keys[2]).GetValue(s, null));
        }

        //
        static ExSaveData()
        {
            GameMainCallbacks.Deserialize.Callbacks.Add(CallbackName, deserializeCallback);
            GameMainCallbacks.Serialize.Callbacks.Add(CallbackName, serializeCallback);
            GameMainCallbacks.DeleteSerializeData.Callbacks.Add(CallbackName, deleteSerializeDataCallback);
        }

        public static void DummyInitialize(GameMain that)
        {
            // static class の生成を強要するためのダミーメソッド
            //
            // C# の static class の内容はクラスへの初アクセス時に遅延生成される。
            // このため、ExSaveData対応プラグインが１つも無い場合、
            // ExSaveDataのコンストラクタが呼ばれず、コールバックの設定ができない。
            //
            // このメソッドはこれを回避するためのダミーメソッドで、
            // GameMain.OnInitializeの末尾から呼び出される。
            // 
            // 本当はアトリビュート等でもっと良いやり方があるはずなんだろうけど、
            // 分かっていないのでとりあえずこのまま。
        }

        static void deserializeCallback(GameMain that, int f_nSaveNo)
        {
            try
            {
                string xmlFilePath = makeXmlFilename(that, f_nSaveNo);
                saveDataPluginSettings = new SaveDataPluginSettings();
                if (File.Exists(xmlFilePath))
                {
                    PluginSettings.Load(xmlFilePath);
                }
            }
            catch (Exception e)
            {
                Helper.ShowException(e);
            }
        }

        static void serializeCallback(GameMain that, int f_nSaveNo, string f_strComment)
        {
            try
            {
                CharacterMgr cm = GameMain.Instance.CharacterMgr;
                for (int i = 0, n = cm.GetStockMaidCount(); i < n; i++)
                {
                    Maid maid = cm.GetStockMaid(i);
                    SetMaidName(maid);
                }
                CleanupMaids();
                string path = GameMainMakeSavePathFileName(that, f_nSaveNo);
                string xmlFilePath = makeXmlFilename(that, f_nSaveNo);
                PluginSettings.Save(xmlFilePath, path);
            }
            catch (Exception e)
            {
                Helper.ShowException(e);
            }
        }

        static void deleteSerializeDataCallback(GameMain that, int f_nSaveNo)
        {
            try
            {
                string xmlFilePath = makeXmlFilename(that, f_nSaveNo);
                if (File.Exists(xmlFilePath))
                {
                    File.Delete(xmlFilePath);
                }
            }
            catch (Exception e)
            {
                Helper.ShowException(e);
            }
        }
    }

    internal class SaveDataPluginSettings
    {
        SaveData saveData = new SaveData();
        public const string GlobalMaidGuid = "global";

        public SaveDataPluginSettings Load(string xmlFilePath)
        {
            XmlDocument xml = Helper.LoadXmlDocument(xmlFilePath);
            saveData = (new SaveData()).Load(xml.SelectSingleNode("/savedata"));
            return this;
        }

        public void Save(string xmlFilePath, string targetSaveDataFileName)
        {
            XmlDocument xml = Helper.LoadXmlDocument(xmlFilePath);
            saveData.target = targetSaveDataFileName;

            XmlNode xmlSaveData = SelectOrAppendNode(xml, "savedata", "savedata");
            saveData.Save(xmlSaveData);
            xml.Save(xmlFilePath);
        }

        public bool Contains(string guid, string pluginName, string propName)
        {
            return saveData.Contains(guid, pluginName, propName);
        }

        public string Get(string guid, string pluginName, string propName, string defaultValue)
        {
            return saveData.Get(guid, pluginName, propName, defaultValue);
        }

        public bool Set(string guid, string pluginName, string propName, string value)
        {
            return saveData.Set(guid, pluginName, propName, value);
        }

        public bool Remove(string guid, string pluginName, string propName)
        {
            return saveData.Remove(guid, pluginName, propName);
        }

        public bool ContainsMaid(string guid)
        {
            return saveData.ContainsMaid(guid);
        }

        public void SetMaid(string guid, string lastName, string firstName, string createTime)
        {
            saveData.SetMaid(guid, lastName, firstName, createTime);
        }

        public bool SetMaidName(string guid, string lastName, string firstName, string createTime)
        {
            return saveData.SetMaidName(guid, lastName, firstName, createTime);
        }

        public void Cleanup(List<string> guids)
        {
            saveData.Cleanup(guids);
        }

        public class SaveData
        {
            public string target;       // 寄生先のセーブデータ名
            Dictionary<string, Maid> maids;

            public SaveData()
            {
                Clear();
            }

            public void Clear()
            {
                maids = new Dictionary<string, Maid>();
                SetMaid(GlobalMaidGuid, "", "", "");
            }

            public SaveData Load(XmlNode xmlNode)
            {
                target = GetAttribute(xmlNode, "target");
                Clear();
                foreach (XmlNode n in xmlNode.SelectNodes("maids/maid"))
                {
                    string a = GetAttribute(n, "guid");
                    if (a != null)
                    {
                        maids[a] = (new Maid()).Load(n);
                    }
                }
                return this;
            }

            public void Save(XmlNode xmlNode)
            {
                SetAttribute(xmlNode, "target", target);
                XmlNode xmlMaids = SelectOrAppendNode(xmlNode, "maids", "maids");

                // 存在しない<maid>を削除
                foreach (XmlNode n in xmlMaids.SelectNodes("maid"))
                {
                    bool bRemove = true;
                    string guid = GetAttribute(n, "guid");
                    if (guid != null)
                    {
                        if (maids.ContainsKey(guid))
                        {
                            bRemove = false;
                        }
                    }
                    if (bRemove)
                    {
                        xmlMaids.RemoveChild(n);
                    }
                }

                foreach (var kv in maids.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value))
                {
                    XmlNode n = SelectOrAppendNode(xmlMaids, string.Format("maid[@guid='{0}']", kv.Key), "maid");
                    kv.Value.Save(n);
                }
            }

            public void Cleanup(List<string> guids)
            {
                maids = maids.Where(kv => guids.Contains(kv.Key) || kv.Key == GlobalMaidGuid).ToDictionary(kv => kv.Key, kv => kv.Value);
            }

            Maid TryGetValue(string guid)
            {
                Maid maid;
                if (maids.TryGetValue(guid, out maid))
                {
                    return maid;
                }
                return null;
            }

            public void SetMaid(string guid, string lastName, string firstName, string createTime)
            {
                Maid maid = TryGetValue(guid);
                if (maid == null)
                {
                    maid = new Maid();
                    maids[guid] = maid;
                }
                maid.SetMaid(guid, lastName, firstName, createTime);
            }

            public bool SetMaidName(string guid, string lastName, string firstName, string createTime)
            {
                Maid maid = TryGetValue(guid);
                if (maid == null)
                {
                    return false;
                }
                return maid.SetMaidName(lastName, firstName, createTime);
            }

            public bool ContainsMaid(string guid)
            {
                return maids.ContainsKey(guid);
            }

            public bool Contains(string guid, string pluginName, string propName)
            {
                Maid maid = TryGetValue(guid);
                if (maid == null)
                {
                    return false;
                }
                return maid.Contains(pluginName, propName);
            }

            public string Get(string guid, string pluginName, string propName, string defaultValue)
            {
                Maid maid = TryGetValue(guid);
                if (maid == null)
                {
                    return defaultValue;
                }
                return maid.Get(pluginName, propName, defaultValue);
            }

            public bool Set(string guid, string pluginName, string propName, string value)
            {
                Maid maid = TryGetValue(guid);
                if (maid == null)
                {
                    return false;
                }
                return maid.Set(pluginName, propName, value);
            }

            public bool Remove(string guid, string pluginName, string propName)
            {
                Maid maid = TryGetValue(guid);
                if (maid == null)
                {
                    return false;
                }
                return maid.Remove(pluginName, propName);
            }
        }

        public class Maid
        {
            string guid;
            string lastname;
            string firstname;
            string createtime;
            Dictionary<string, Plugin> plugins = new Dictionary<string, Plugin>();

            public Maid Load(XmlNode xmlNode)
            {
                guid = GetAttribute(xmlNode, "guid");
                lastname = GetAttribute(xmlNode, "lastname");
                firstname = GetAttribute(xmlNode, "firstname");
                createtime = GetAttribute(xmlNode, "createtime");
                plugins = new Dictionary<string, Plugin>();

                foreach (XmlNode n in xmlNode.SelectNodes("plugins/plugin"))
                {
                    string name = GetAttribute(n, "name");
                    if (name != null)
                    {
                        plugins[name] = (new Plugin()).Load(n);
                    }
                }
                return this;
            }

            public void Save(XmlNode xmlNode)
            {
                SetAttribute(xmlNode, "guid", guid);
                SetAttribute(xmlNode, "lastname", lastname);
                SetAttribute(xmlNode, "firstname", firstname);
                SetAttribute(xmlNode, "createtime", createtime);

                XmlNode xmlPlugins = SelectOrAppendNode(xmlNode, "plugins", null);
                foreach (var kv in plugins)
                {
                    string path = string.Format("plugin[@name='{0}']", kv.Key);
                    XmlNode n = xmlPlugins.SelectSingleNode(path);
                    if (n == null)
                    {
                        n = SelectOrAppendNode(xmlPlugins, path, "plugin");
                    }
                    else
                    {
                        n.RemoveAll();
                    }
                    kv.Value.Save(n);
                }
            }

            public void SetMaid(string guid, string lastName, string firstName, string createTime)
            {
                this.lastname = lastName;
                this.firstname = firstName;
                this.createtime = createTime;
                this.guid = guid;
                this.plugins = new Dictionary<string, Plugin>();
            }

            public bool SetMaidName(string lastName, string firstName, string createTime)
            {
                this.lastname = lastName;
                this.firstname = firstName;
                this.createtime = createTime;
                return true;
            }

            Plugin TryGetValue(string pluginName)
            {
                Plugin plugin;
                if (plugins.TryGetValue(pluginName, out plugin))
                {
                    return plugin;
                }
                return null;
            }

            public bool Contains(string pluginName, string propName)
            {
                Plugin plugin = TryGetValue(pluginName);
                if (plugin == null)
                {
                    return false;
                }
                return plugin.Contains(propName);
            }

            public string Get(string pluginName, string propName, string defaultValue)
            {
                Plugin plugin = TryGetValue(pluginName);
                if (plugin == null)
                {
                    return defaultValue;
                }
                return plugin.Get(propName, defaultValue);
            }

            public bool Set(string pluginName, string propName, string value)
            {
                Plugin plugin = TryGetValue(pluginName);
                if (plugin == null)
                {
                    plugin = new Plugin() { name = pluginName };
                    plugins[pluginName] = plugin;
                }
                return plugin.Set(propName, value);
            }

            public bool Remove(string pluginName, string propName)
            {
                Plugin plugin = TryGetValue(pluginName);
                if (plugin == null)
                {
                    return false;
                }
                return plugin.Remove(propName);
            }
        }

        public class Plugin
        {
            public string name;
            public Dictionary<string, string> props = new Dictionary<string, string>();

            public Plugin Load(XmlNode xmlNode)
            {
                name = GetAttribute(xmlNode, "name");
                props = new Dictionary<string, string>();
                foreach (XmlNode e in xmlNode.SelectNodes("prop"))
                {
                    props[GetAttribute(e, "name")] = GetAttribute(e, "value");
                }
                return this;
            }

            public void Save(XmlNode xmlNode)
            {
                SetAttribute(xmlNode, "name", name);
                foreach (var kv in props)
                {
                    XmlNode n = SelectOrAppendNode(xmlNode, string.Format("prop[@name='{0}']", kv.Key), "prop");
                    SetAttribute(n, "name", kv.Key);
                    SetAttribute(n, "value", kv.Value);
                }
            }

            public bool Contains(string propName)
            {
                return props.ContainsKey(propName);
            }

            public string Get(string propName, string defaultValue)
            {
                string value;
                if (!props.TryGetValue(propName, out value))
                {
                    value = defaultValue;
                }
                return value;
            }

            public bool Set(string propName, string value)
            {
                props[propName] = value;
                return true;
            }

            public bool Remove(string propName)
            {
                bool b = props.Remove(propName);
                return b;
            }
        }

        static XmlNode SelectOrAppendNode(XmlNode xmlNode, string path, string prefix)
        {
            if (xmlNode == null)
            {
                return null;
            }

            if (prefix == null)
            {
                prefix = path;
            }

            XmlNode n = xmlNode.SelectSingleNode(path);
            if (n == null)
            {
                XmlDocument od = xmlNode.OwnerDocument;
                if (xmlNode is XmlDocument)
                {
                    od = (XmlDocument)xmlNode;
                }
                if (od == null)
                {
                    return null;
                }
                n = xmlNode.AppendChild(od.CreateElement(prefix));
            }
            return n;
        }

        static string GetAttribute(XmlNode xmlNode, string name)
        {
            if (xmlNode == null)
            {
                return null;
            }
            var a = xmlNode.Attributes[name];
            if (a == null)
            {
                return null;
            }
            return a.Value;
        }

        static void SetAttribute(XmlNode xmlNode, string name, string value)
        {
            ((XmlElement)xmlNode).SetAttribute(name, value);
        }
    }
}

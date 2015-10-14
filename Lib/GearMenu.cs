using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityInjector.Attributes;
using System.IO;
using System.Linq;

namespace GearMenu
{
    /// <summary>
    /// 歯車メニューへのアイコン登録
    /// </summary>
    public static class Buttons
    {
        // 識別名の実体。同じ名前を保持すること。詳細は SetOnReposition を参照
        static string Name_ = "CM3D2.GearMenu.Buttons";

        // バージョン文字列の実体。改善、改造した場合は文字列の辞書順がより大きい値に更新すること
        static string Version_ = Name_ + " 0.0.1.0";

        /// <summary>
        /// 識別名
        /// </summary>
        public static string Name { get { return Name_; } }

        /// <summary>
        /// バージョン文字列
        /// </summary>
        public static string Version { get { return Version_; } }

        /// <summary>
        /// 歯車メニューにボタンを追加
        /// </summary>
        /// <param name="plugin">ボタンを追加するプラグイン。アイコンへのマウスオーバー時に名前とバージョンが表示される</param>
        /// <param name="pngData">アイコン画像。null可(システムアイコン使用)。32x32ピクセルのPNGファイル</param>
        /// <param name="action">コールバック。null可(コールバック削除)。アイコンクリック時に呼び出されるコールバック</param>
        /// <returns>生成されたボタンのGameObject</returns>
        /// <example>
        /// ボタン追加例
        /// <code>
        /// public class MyPlugin : UnityInjector.PluginBase {
        ///     void Awake() {
        ///         GearMenu.Buttons.Add(this, null, GearMenuCallback);
        ///     }
        ///     void GearMenuCallback(GameObject goButton) {
        ///         Debug.LogWarning("GearMenuCallback呼び出し");
        ///     }
        /// }
        /// </code>
        /// </example>
        public static GameObject Add(UnityInjector.PluginBase plugin, byte[] pngData, Action<GameObject> action)
        {
            return Add(null, plugin, pngData, action);
        }

        /// <summary>
        /// 歯車メニューにボタンを追加
        /// </summary>
        /// <param name="name">ボタンオブジェクト名。null可</param>
        /// <param name="plugin">ボタンを追加するプラグイン。アイコンへのマウスオーバー時に名前とバージョンが表示される</param>
        /// <param name="pngData">アイコン画像。null可(システムアイコン使用)。32x32ピクセルのPNGファイル</param>
        /// <param name="action">コールバック。null可(コールバック削除)。アイコンクリック時に呼び出されるコールバック</param>
        /// <returns>生成されたボタンのGameObject</returns>
        /// <example>
        /// ボタン追加例
        /// <code>
        /// public class MyPlugin : UnityInjector.PluginBase {
        ///     void Awake() {
        ///         GearMenu.Buttons.Add(GetType().Name, this, null, GearMenuCallback);
        ///     }
        ///     void GearMenuCallback(GameObject goButton) {
        ///         Debug.LogWarning("GearMenuCallback呼び出し");
        ///     }
        /// }
        /// </code>
        /// </example>
        public static GameObject Add(string name, UnityInjector.PluginBase plugin, byte[] pngData, Action<GameObject> action)
        {
            var pluginNameAttr = Attribute.GetCustomAttribute(plugin.GetType(), typeof(PluginNameAttribute)) as PluginNameAttribute;
            var pluginVersionAttr = Attribute.GetCustomAttribute(plugin.GetType(), typeof(PluginVersionAttribute)) as PluginVersionAttribute;
            string pluginName = (pluginNameAttr == null) ? plugin.Name : pluginNameAttr.Name;
            string pluginVersion = (pluginVersionAttr == null) ? string.Empty : pluginVersionAttr.Version;
            string label = string.Format("{0} {1}", pluginName, pluginVersion);
            return Add(name, label, pngData, action);
        }

        /// <summary>
        /// 歯車メニューにボタンを追加
        /// </summary>
        /// <param name="label">ツールチップテキスト。null可(ツールチップ非表示)。アイコンへのマウスオーバー時に表示される</param>
        /// <param name="pngData">アイコン画像。null可(システムアイコン使用)。32x32ピクセルのPNGファイル</param>
        /// <param name="action">コールバック。null可(コールバック削除)。アイコンクリック時に呼び出されるコールバック</param>
        /// <returns>生成されたボタンのGameObject</returns>
        /// <example>
        /// ボタン追加例
        /// <code>
        /// public class MyPlugin : UnityInjector.PluginBase {
        ///     void Awake() {
        ///         GearMenu.Buttons.Add("テスト", null, GearMenuCallback);
        ///     }
        ///     void GearMenuCallback(GameObject goButton) {
        ///         Debug.LogWarning("GearMenuCallback呼び出し");
        ///     }
        /// }
        /// </code>
        /// </example>
        public static GameObject Add(string label, byte[] pngData, Action<GameObject> action)
        {
            return Add(null, label, pngData, action);
        }

        /// <summary>
        /// 歯車メニューにボタンを追加
        /// </summary>
        /// <param name="name">ボタンオブジェクト名。null可</param>
        /// <param name="label">ツールチップテキスト。null可(ツールチップ非表示)。アイコンへのマウスオーバー時に表示される</param>
        /// <param name="pngData">アイコン画像。null可(システムアイコン使用)。32x32ピクセルのPNGファイル</param>
        /// <param name="action">コールバック。null可(コールバック削除)。アイコンクリック時に呼び出されるコールバック</param>
        /// <returns>生成されたボタンのGameObject</returns>
        /// <example>
        /// ボタン追加例
        /// <code>
        /// public class MyPlugin : UnityInjector.PluginBase {
        ///     void Awake() {
        ///         GearMenu.Buttons.Add(GetType().Name, "テスト", null, GearMenuCallback);
        ///     }
        ///     void GearMenuCallback(GameObject goButton) {
        ///         Debug.LogWarning("GearMenuCallback呼び出し");
        ///     }
        /// }
        /// </code>
        /// </example>
        public static GameObject Add(string name, string label, byte[] pngData, Action<GameObject> action)
        {
            GameObject goButton = null;

            // 既に存在する場合は削除して続行
            if (Contains(name))
            {
                Remove(name);
            }

            if (action == null)
            {
                return goButton;
            }

            try
            {
                // ギアメニューの子として、コンフィグパネル呼び出しボタンを複製
                goButton = NGUITools.AddChild(Grid, UTY.GetChildObject(Grid, "Config", true));

                // 名前を設定
                if (name != null)
                {
                    goButton.name = name;
                }

                // イベントハンドラ設定（同時に、元から持っていたハンドラは削除）
                EventDelegate.Set(goButton.GetComponent<UIButton>().onClick, () => { action(goButton); });

                // ポップアップテキストを追加
                {
                    UIEventTrigger t = goButton.GetComponent<UIEventTrigger>();
                    EventDelegate.Add(t.onHoverOut, () => { SysShortcut.VisibleExplanation(null, false); });
                    EventDelegate.Add(t.onDragStart, () => { SysShortcut.VisibleExplanation(null, false); });
                    SetText(goButton, label);
                }

                // PNG イメージを設定
                {
                    if (pngData == null)
                    {
                        pngData = DefaultIcon.Png;
                    }

                    // 本当はスプライトを削除したいが、削除するとパネルのα値とのTween同期が動作しない
                    // (動作させる方法が分からない) ので、スプライトを描画しないように設定する
                    // もともと持っていたスプライトを削除する場合は以下のコードを使うと良い
                    //     NGUITools.Destroy(goButton.GetComponent<UISprite>());
                    UISprite us = goButton.GetComponent<UISprite>();
                    us.type = UIBasicSprite.Type.Filled;
                    us.fillAmount = 0.0f;

                    // テクスチャを生成
                    var tex = new Texture2D(1, 1);
                    tex.LoadImage(pngData);

                    // 新しくテクスチャスプライトを追加
                    UITexture ut = NGUITools.AddWidget<UITexture>(goButton);
                    ut.material = new Material(ut.shader);
                    ut.material.mainTexture = tex;
                    ut.MakePixelPerfect();
                }

                // グリッド内のボタンを再配置
                Reposition();
            }
            catch
            {
                // 既にオブジェクトを作っていた場合は削除
                if (goButton != null)
                {
                    NGUITools.Destroy(goButton);
                    goButton = null;
                }
                throw;
            }
            return goButton;
        }

        /// <summary>
        /// 歯車メニューからボタンを削除
        /// </summary>
        /// <param name="name">ボタン名。Add()に与えた名前</param>
        public static void Remove(string name)
        {
            Remove(Find(name));
        }

        /// <summary>
        /// 歯車メニューからボタンを削除
        /// </summary>
        /// <param name="go">ボタン。Add()の戻り値</param>
        public static void Remove(GameObject go)
        {
            NGUITools.Destroy(go);
            Reposition();
        }

        /// <summary>
        /// 歯車メニュー内のボタンの存在を確認
        /// </summary>
        /// <param name="name">ボタン名。Add()に与えた名前</param>
        public static bool Contains(string name)
        {
            return Find(name) != null;
        }

        /// <summary>
        /// 歯車メニュー内のボタンの存在を確認
        /// </summary>
        /// <param name="go">ボタン。Add()の戻り値</param>
        public static bool Contains(GameObject go)
        {
            return Contains(go.name);
        }

        /// <summary>
        /// ボタンに枠をつける
        /// </summary>
        /// <param name="name">ボタン名。Add()に与えた名前</param>
        /// <param name="color">枠の色</param>
        public static void SetFrameColor(string name, Color color)
        {
            SetFrameColor(Find(name), color);
        }

        /// <summary>
        /// ボタンに枠をつける
        /// </summary>
        /// <param name="go">ボタン。Add()の戻り値</param>
        /// <param name="color">枠の色</param>
        public static void SetFrameColor(GameObject go, Color color)
        {
            var uiTexture = go.GetComponentInChildren<UITexture>();
            if (uiTexture == null)
            {
                return;
            }
            var tex = uiTexture.mainTexture as Texture2D;
            if (tex == null)
            {
                return;
            }
            for (int x = 1; x < tex.width - 1; x++)
            {
                tex.SetPixel(x, 0, color);
                tex.SetPixel(x, tex.height - 1, color);
            }
            for (int y = 1; y < tex.height - 1; y++)
            {
                tex.SetPixel(0, y, color);
                tex.SetPixel(tex.width - 1, y, color);
            }
            tex.Apply();
        }

        /// <summary>
        /// ボタンの枠を消す
        /// </summary>
        /// <param name="name">ボタン名。Add()に与えた名前</param>
        public static void ResetFrameColor(string name)
        {
            ResetFrameColor(Find(name));
        }

        /// <summary>
        /// ボタンの枠を消す
        /// </summary>
        /// <param name="go">ボタンのGameObject。Add()の戻り値</param>
        public static void ResetFrameColor(GameObject go)
        {
            SetFrameColor(go, DefaultFrameColor);
        }

        /// <summary>
        /// マウスオーバー時のテキスト指定
        /// </summary>
        /// <param name="name">ボタン名。Add()に与えた名前</param>
        /// <param name="label">マウスオーバー時のテキスト。null可</param>
        public static void SetText(string name, string label)
        {
            SetText(Find(name), label);
        }

        /// <summary>
        /// マウスオーバー時のテキスト指定
        /// </summary>
        /// <param name="go">ボタンのGameObject。Add()の戻り値</param>
        /// <param name="label">マウスオーバー時のテキスト。null可</param>
        public static void SetText(GameObject go, string label)
        {
            var t = go.GetComponent<UIEventTrigger>();
            t.onHoverOver.Clear();
            EventDelegate.Add(t.onHoverOver, () => { SysShortcut.VisibleExplanation(label, label != null); });
            var b = go.GetComponent<UIButton>();

            // 既にホバー中なら説明を変更する
            if (b.state == UIButtonColor.State.Hover)
            {
                SysShortcut.VisibleExplanation(label, label != null);
            }
        }

        // システムショートカット内のGameObjectを見つける
        static GameObject Find(string name)
        {
            Transform t = GridUI.GetChildList().FirstOrDefault(c => c.gameObject.name == name);
            return t == null ? null : t.gameObject;
        }

        // グリッド内のボタンを再配置
        static void Reposition()
        {
            // 必要なら UIGrid.onRepositionを設定、呼び出しを行う
            SetAndCallOnReposition(GridUI);

            // 次回の UIGrid.Update 処理時にグリッド内のボタン再配置が行われるようリクエスト
            GridUI.repositionNow = true;
        }

        // 必要に応じて UIGrid.onReposition を登録、呼び出す
        static void SetAndCallOnReposition(UIGrid uiGrid)
        {
            string targetVersion = GetOnRepositionVersion(uiGrid);

            // バージョン文字列が null の場合、知らないクラスが登録済みなのであきらめる
            if (targetVersion == null)
            {
                return;
            }

            // 何も登録されていないか、自分より古いバージョンだったら新しい onReposition を登録する
            if (targetVersion == string.Empty || string.Compare(targetVersion, Version, false) < 0)
            {
                uiGrid.onReposition = (new OnRepositionHandler(Version)).OnReposition;
            }

            // PreOnReposition を持つ場合はそれを呼び出す
            if (uiGrid.onReposition != null)
            {
                object target = uiGrid.onReposition.Target;
                if (target != null)
                {
                    Type type = target.GetType();
                    MethodInfo mi = type.GetMethod("PreOnReposition");
                    if (mi != null)
                    {
                        mi.Invoke(target, new object[] { });
                    }
                }
            }
        }

        // UIGrid.onReposition を保持するオブジェクトのバージョン文字列を得る
        //  null            知らないクラスもしくはバージョン文字列だった
        //  string.Empty    UIGrid.onRepositionが未登録だった
        //  その他          取得したバージョン文字列
        static string GetOnRepositionVersion(UIGrid uiGrid)
        {
            if (uiGrid.onReposition == null)
            {
                // 未登録だった
                return string.Empty;
            }

            object target = uiGrid.onReposition.Target;
            if (target == null)
            {
                // Delegate.Target が null ということは、
                // UIGrid.onReposition は static なメソッドなので、たぶん知らないクラス
                return null;
            }

            Type type = target.GetType();
            if (type == null)
            {
                // 型情報が取れないので、あきらめる
                return null;
            }

            FieldInfo fi = type.GetField("Version", BindingFlags.Instance | BindingFlags.Public);
            if (fi == null)
            {
                // public な Version メンバーを持っていないので、たぶん知らないクラス
                return null;
            }

            string targetVersion = fi.GetValue(target) as string;
            if (targetVersion == null || !targetVersion.StartsWith(Name))
            {
                // 知らないバージョン文字列だった
                return null;
            }

            return targetVersion;
        }

        public static SystemShortcut SysShortcut { get { return GameMain.Instance.SysShortcut; } }
        public static UIPanel SysShortcutPanel { get { return SysShortcut.GetComponent<UIPanel>(); } }
        public static UISprite SysShortcutExplanation
        {
            get
            {
                Type type = typeof(SystemShortcut);
                FieldInfo fi = type.GetField("m_spriteExplanation", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fi == null)
                {
                    return null;
                }
                return fi.GetValue(SysShortcut) as UISprite;
            }
        }
        public static GameObject Base { get { return SysShortcut.gameObject.transform.Find("Base").gameObject; } }
        public static UISprite BaseSprite { get { return Base.GetComponent<UISprite>(); } }
        public static GameObject Grid { get { return Base.gameObject.transform.Find("Grid").gameObject; } }
        public static UIGrid GridUI { get { return Grid.GetComponent<UIGrid>(); } }
        public static readonly Color DefaultFrameColor = new Color(1f, 1f, 1f, 0f);

        // UIGrid.onReposition処理用のクラス
        // Delegate.Targetの値を生かすために、static ではなくインスタンスとして生成
        class OnRepositionHandler
        {
            public string Version;

            public OnRepositionHandler(string version)
            {
                this.Version = version;
            }

            public void OnReposition()
            {
            }

            public void PreOnReposition()
            {
                var g = GridUI;
                var b = BaseSprite;

                // ratio : 画面横幅に対するボタン全体の横幅の比率。0.5 なら画面半分
                float ratio = 3.0f / 4.0f;
                float pixelSizeAdjustment = UIRoot.GetPixelSizeAdjustment(Base);

                // g.cellWidth  = 39;
                g.cellHeight = g.cellWidth;
                g.arrangement = UIGrid.Arrangement.CellSnap;
                g.sorting = UIGrid.Sorting.None;
                g.pivot = UIWidget.Pivot.TopRight;
                g.maxPerLine = (int)(Screen.width / (g.cellWidth / pixelSizeAdjustment) * ratio);

                var children = g.GetChildList();
                int itemCount = children.Count;
                int spriteItemX = Math.Min(g.maxPerLine, itemCount);
                int spriteItemY = Math.Max(1, (itemCount - 1) / g.maxPerLine + 1);
                int spriteWidthMargin = (int)(g.cellWidth * 3 / 2 + 8);
                int spriteHeightMargin = (int)(g.cellHeight / 2);
                float pivotOffsetY = spriteHeightMargin * 1.5f + 1f;

                b.pivot = UIWidget.Pivot.TopRight;
                b.width = (int)(spriteWidthMargin + g.cellWidth * spriteItemX);
                b.height = (int)(spriteHeightMargin + g.cellHeight * spriteItemY + 2f);

                // (946,502) はもとの Base の localPosition の値
                // 他のオブジェクトから値を取れないだろうか？
                Base.transform.localPosition = new Vector3(946.0f, 502.0f + pivotOffsetY, 0.0f);

                // ここでの、高さ(spriteItemY)に応じて横方向に補正する意味が分からない。
                // たぶん何かを誤解している
                Grid.transform.localPosition = new Vector3(
                    -2.0f + (-spriteItemX - 1 + spriteItemY - 1) * g.cellWidth,
                    -1.0f - pivotOffsetY,
                    0f);

                {
                    int a = 0;
                    string[] specialNames = GameMain.Instance.CMSystem.NetUse ? OnlineButtonNames : OfflineButtonNames;
                    foreach (Transform child in children)
                    {
                        int i = a++;

                        // システムが持っているオブジェクトの場合は特別に順番をつける
                        int si = Array.IndexOf(specialNames, child.gameObject.name);
                        if (si >= 0)
                        {
                            i = si;
                        }

                        float x = (-i % g.maxPerLine + spriteItemX - 1) * g.cellWidth;
                        float y = (i / g.maxPerLine) * g.cellHeight;
                        child.localPosition = new Vector3(x, -y, 0f);
                    }
                }

                // マウスオーバー時のテキストの位置を指定
                {
                    UISprite sse = SysShortcutExplanation;
                    Vector3 v = sse.gameObject.transform.localPosition;
                    v.y = Base.transform.localPosition.y + sse.height;
                    sse.gameObject.transform.localPosition = v;
                }
            }

            // オンライン時のボタンの並び順。インデクスの若い側が右になる
            static string[] OnlineButtonNames = new string[] {
                "Config", "Ss", "SsUi", "Shop", "ToTitle", "Info", "Exit"
            };

            // オフライン時のボタンの並び順。インデクスの若い側が右になる
            static string[] OfflineButtonNames = new string[] {
                "Config", "Ss", "SsUi", "ToTitle", "Info", "Exit"
            };
        }
    }

    // デフォルトアイコン
    internal static class DefaultIcon
    {
        // 32x32 ピクセルの PNG イメージ
        public static byte[] Png = new byte[] {
            0x89,0x50,0x4e,0x47,0x0d,0x0a,0x1a,0x0a,0x00,0x00,0x00,0x0d,0x49,0x48,0x44,0x52,
            0x00,0x00,0x00,0x20,0x00,0x00,0x00,0x20,0x08,0x06,0x00,0x00,0x00,0x73,0x7a,0x7a,
            0xf4,0x00,0x00,0x00,0x04,0x73,0x42,0x49,0x54,0x08,0x08,0x08,0x08,0x7c,0x08,0x64,
            0x88,0x00,0x00,0x00,0x09,0x70,0x48,0x59,0x73,0x00,0x00,0x0e,0xc4,0x00,0x00,0x0e,
            0xc4,0x01,0x95,0x2b,0x0e,0x1b,0x00,0x00,0x05,0x00,0x49,0x44,0x41,0x54,0x58,0x85,
            0xc5,0x97,0x5d,0x48,0x54,0x5b,0x14,0xc7,0x7f,0xe7,0x8c,0x8e,0x58,0xd2,0xd5,0xa0,
            0x32,0x9b,0x04,0x4b,0x8d,0xa9,0x89,0x63,0x2a,0x42,0x5c,0xe8,0x83,0x3e,0x1f,0x8a,
            0xca,0x32,0x21,0x8c,0x02,0x53,0xa4,0x9e,0x82,0x62,0x1e,0x24,0x7b,0x28,0xc2,0x5e,
            0x7b,0xa8,0x09,0x4a,0x28,0x49,0x08,0x4b,0xa2,0x04,0x83,0x1e,0xa3,0x7a,0xa9,0x39,
            0xc7,0x8f,0x29,0x30,0x24,0x11,0xc3,0x09,0x43,0x34,0x61,0xa6,0xd3,0x7c,0xec,0x1e,
            0xe6,0xce,0x6e,0xc6,0x39,0x33,0x8a,0xdd,0xee,0x5d,0x6f,0x6b,0xef,0xb5,0xcf,0xff,
            0x77,0xd6,0x59,0x7b,0xed,0x7d,0x14,0x21,0x84,0xe0,0x7f,0xb4,0xac,0x3f,0x2d,0x10,
            0x0a,0x85,0x18,0x1e,0x1e,0x46,0xd7,0x75,0x34,0x4d,0x63,0xf3,0xe6,0xcd,0x7f,0x16,
            0x20,0x1a,0x8d,0x32,0x3e,0x3e,0x8e,0xd7,0xeb,0x45,0xd7,0x75,0x06,0x07,0x07,0x09,
            0x06,0x83,0x00,0xcc,0xcc,0xcc,0xfc,0x19,0x80,0xa9,0xa9,0x29,0x0c,0xc3,0x40,0xd7,
            0x75,0x0c,0xc3,0xe0,0xeb,0xd7,0xaf,0x96,0x71,0xba,0xae,0xa7,0x8c,0xfd,0x16,0x40,
            0x5f,0x5f,0x1f,0x4f,0x9f,0x3e,0x65,0x74,0x74,0x74,0x41,0xf1,0x9f,0x3f,0x7f,0x66,
            0x72,0x72,0x92,0x15,0x2b,0x56,0xc4,0x87,0x84,0xba,0x58,0x71,0xbf,0xdf,0xcf,0xad,
            0x5b,0xb7,0x2c,0xc5,0xb3,0xb3,0xb3,0x71,0xb9,0x5c,0x96,0xeb,0xde,0xbd,0x7b,0x97,
            0xe8,0x8a,0x45,0x65,0x40,0x08,0x41,0x47,0x47,0x07,0xa1,0x50,0x08,0x00,0x9b,0xcd,
            0x46,0x59,0x59,0x19,0x9a,0xa6,0x51,0x51,0x51,0x81,0xd3,0xe9,0xe4,0xf5,0xeb,0xd7,
            0x0c,0x0d,0x0d,0xa5,0xac,0xd5,0x75,0x9d,0xfd,0xfb,0xf7,0xc7,0x5d,0x65,0x41,0x00,
            0x81,0x40,0x80,0x27,0x4f,0x9e,0x10,0x0e,0x87,0x51,0x55,0x15,0x21,0x04,0x7e,0xbf,
            0x9f,0xda,0xda,0x5a,0x34,0x4d,0xc3,0xe5,0x72,0xb1,0x64,0xc9,0x12,0x19,0x1f,0x0c,
            0x06,0xe9,0xe8,0xe8,0x90,0x7e,0x5d,0x5d,0x1d,0xdd,0xdd,0xdd,0x00,0xf4,0xf7,0xf7,
            0x13,0x8d,0x46,0x51,0xd5,0x58,0xf2,0xe7,0x05,0x08,0x06,0x83,0xb4,0xb5,0xb5,0xe1,
            0xf3,0xf9,0xe4,0xd8,0xd9,0xb3,0x67,0xb9,0x71,0xe3,0x46,0xda,0x35,0x8f,0x1f,0x3f,
            0x96,0x85,0x58,0x53,0x53,0x43,0x43,0x43,0x03,0xcf,0x9e,0x3d,0xe3,0xfb,0xf7,0xef,
            0xcc,0xcc,0xcc,0x30,0x3a,0x3a,0xca,0xba,0x75,0xeb,0x00,0xc8,0x58,0x03,0x81,0x40,
            0x80,0x4b,0x97,0x2e,0x25,0x89,0x37,0x35,0x35,0x71,0xf0,0xe0,0xc1,0xb4,0x6b,0xbe,
            0x7c,0xf9,0xc2,0xa3,0x47,0x8f,0x62,0x6f,0x97,0x95,0x45,0x73,0x73,0x33,0x76,0xbb,
            0x1d,0x4d,0xd3,0x64,0x4c,0x42,0x1d,0xa4,0x2f,0xc2,0x40,0x20,0x90,0xf2,0xe6,0x67,
            0xce,0x9c,0xe1,0xc8,0x91,0x23,0x69,0xc5,0x85,0x10,0xdc,0xbd,0x7b,0x17,0xd3,0x34,
            0x01,0x38,0x7c,0xf8,0x30,0x6b,0xd6,0xac,0x01,0xa0,0xaa,0xaa,0x4a,0xc6,0x25,0x6c,
            0x47,0xc3,0x12,0xc0,0x2a,0xed,0x8d,0x8d,0x8d,0xd4,0xd6,0xd6,0xa2,0x28,0x4a,0x5a,
            0x00,0x9f,0xcf,0xc7,0xcb,0x97,0x2f,0x01,0xc8,0xcf,0xcf,0xa7,0xbe,0xbe,0x5e,0xce,
            0x25,0x02,0xbc,0x7f,0xff,0x1e,0xd3,0x34,0x51,0x14,0xa5,0x2a,0x05,0xc0,0x2a,0xed,
            0x0d,0x0d,0x0d,0x1c,0x3d,0x7a,0x34,0xa3,0x78,0x24,0x12,0xc1,0xe3,0xf1,0x48,0xff,
            0xd4,0xa9,0x53,0xe4,0xe5,0xe5,0x49,0x7f,0xf5,0xea,0xd5,0x14,0x15,0x15,0x01,0x60,
            0x9a,0x26,0x1f,0x3e,0x7c,0x00,0xe6,0xd4,0x80,0x55,0xda,0x8f,0x1f,0x3f,0xce,0x89,
            0x13,0x27,0x32,0x8a,0x03,0xbc,0x78,0xf1,0x82,0x91,0x91,0x11,0x00,0x4a,0x4b,0x4b,
            0xd9,0xb3,0x67,0x4f,0xd2,0xbc,0xa2,0x28,0x54,0x56,0x56,0x02,0xb1,0x6d,0xbb,0x7c,
            0xf9,0x72,0x20,0x61,0x17,0x58,0x89,0x97,0x97,0x97,0xb3,0x61,0xc3,0x06,0xde,0xbc,
            0x79,0x93,0x51,0x5c,0x08,0xc1,0xbd,0x7b,0xf7,0xa4,0xdf,0xdc,0xdc,0x8c,0xcd,0x66,
            0x4b,0x89,0xab,0xae,0xae,0xa6,0xb7,0xb7,0x97,0x43,0x87,0x0e,0x51,0x5c,0x5c,0x1c,
            0x03,0x13,0x42,0x08,0x2b,0xf1,0xc5,0xda,0xf6,0xed,0xdb,0x71,0xbb,0xdd,0x96,0x19,
            0x0b,0x04,0x02,0xb4,0xb4,0xb4,0x70,0xf3,0xe6,0x4d,0xf2,0xf2,0xf2,0x50,0x14,0x45,
            0x51,0x01,0xa6,0xa7,0xa7,0x99,0x98,0x98,0xf8,0x6d,0x71,0x80,0x55,0xab,0x56,0xa5,
            0xfd,0x5c,0xd9,0xd9,0xd9,0xac,0x5c,0xb9,0x92,0xdc,0xdc,0x5c,0x39,0xa6,0xc4,0x2f,
            0x24,0xe3,0xe3,0xe3,0xb8,0xdd,0x6e,0xa6,0xa6,0xa6,0xe4,0x64,0x49,0x49,0x09,0x5b,
            0xb6,0x6c,0x99,0x57,0x54,0x08,0x41,0x6f,0x6f,0x2f,0xa1,0x50,0x08,0xbb,0xdd,0xce,
            0xed,0xdb,0xb7,0x29,0x2c,0x2c,0x4c,0x89,0xf3,0xf9,0x7c,0x5c,0xb8,0x70,0x81,0x73,
            0xe7,0xce,0x71,0xe0,0xc0,0x01,0x14,0x45,0xf9,0xd5,0x8a,0x1d,0x0e,0x07,0xd7,0xaf,
            0x5f,0x4f,0x82,0x18,0x1b,0x1b,0xe3,0xe4,0xc9,0x93,0x6c,0xdd,0xba,0x75,0x5e,0x88,
            0xac,0xac,0x2c,0xba,0xbb,0xbb,0xf9,0xf1,0xe3,0x07,0x77,0xee,0xdc,0xa1,0xb5,0xb5,
            0x35,0x25,0x13,0x6f,0xdf,0xbe,0x05,0xe0,0xfe,0xfd,0xfb,0x6c,0xdb,0xb6,0x0d,0x98,
            0xb3,0x0b,0xe2,0x10,0xf1,0x0a,0x8d,0x44,0x22,0x5c,0xbb,0x76,0x4d,0x2e,0xcc,0x64,
            0xf5,0xf5,0xf5,0x14,0x14,0x14,0x00,0xf0,0xea,0xd5,0x2b,0x06,0x07,0x07,0x53,0x62,
            0xbc,0x5e,0x2f,0x00,0xb3,0xb3,0xb3,0xf2,0xa0,0x4a,0xe9,0x03,0x73,0x21,0xc2,0xe1,
            0x30,0x57,0xaf,0x5e,0xa5,0xbf,0xbf,0x3f,0x23,0xc0,0xd2,0xa5,0x4b,0x39,0x7d,0xfa,
            0xb4,0xf4,0x3d,0x1e,0x0f,0x91,0x48,0x44,0xfa,0xd3,0xd3,0xd3,0x7c,0xfc,0xf8,0x31,
            0x26,0xaa,0xaa,0xf2,0xb8,0xb6,0xec,0x84,0x0e,0x87,0x83,0xf6,0xf6,0x76,0xf9,0x46,
            0xa6,0x69,0x72,0xf9,0xf2,0x65,0x06,0x06,0x06,0x32,0x42,0xec,0xde,0xbd,0x9b,0xb2,
            0xb2,0x32,0x00,0x3e,0x7d,0xfa,0xc4,0xf3,0xe7,0xcf,0xe5,0x9c,0x61,0x18,0xc4,0xef,
            0xbf,0xeb,0xd7,0xaf,0x67,0xd9,0xb2,0x65,0x08,0x21,0x7a,0xd2,0x9e,0x05,0x6b,0xd7,
            0xae,0x4d,0x81,0x68,0x6b,0x6b,0xcb,0x08,0xa1,0xaa,0x2a,0x2d,0x2d,0x2d,0xd2,0xef,
            0xec,0xec,0x64,0x76,0x76,0x16,0x20,0xe9,0x33,0x26,0x14,0xf6,0xdf,0x19,0x4f,0xc3,
            0xe2,0xe2,0x62,0x4b,0x88,0x78,0x1b,0xb5,0x32,0xa7,0xd3,0xc9,0xce,0x9d,0x3b,0x81,
            0xd8,0x25,0xb4,0xab,0xab,0x0b,0x21,0x84,0xfc,0xfe,0x90,0x74,0x2e,0xfc,0xa5,0x2c,
            0xe4,0xbf,0x60,0x6c,0x6c,0x0c,0x8f,0xc7,0x43,0x38,0x1c,0x06,0xc0,0x6e,0xb7,0xe3,
            0x70,0x38,0xa8,0xa8,0xa8,0xc0,0xe5,0x72,0x25,0xf5,0x7c,0x80,0xc9,0xc9,0x49,0x9a,
            0x9a,0x9a,0x30,0x4d,0x13,0x9b,0xcd,0xc6,0xc5,0x8b,0x17,0x69,0x6f,0x6f,0x07,0x20,
            0x27,0x27,0x87,0x87,0x0f,0x1f,0x92,0x93,0x93,0x03,0x10,0x5d,0x10,0xc0,0x5c,0x8b,
            0x46,0xa3,0x9c,0x3f,0x7f,0x9e,0xe1,0xe1,0x61,0x54,0x55,0xa5,0xb4,0xb4,0x14,0x4d,
            0xd3,0xd0,0x34,0x8d,0x8d,0x1b,0x37,0x92,0x9b,0x9b,0x4b,0x57,0x57,0x17,0x9d,0x9d,
            0x9d,0x40,0x6c,0x8b,0xc6,0xe1,0xab,0xab,0xab,0xb9,0x72,0xe5,0x8a,0x7c,0xd4,0xa2,
            0x00,0xe0,0x57,0x53,0x99,0x6b,0x36,0x9b,0x0d,0xa7,0xd3,0x49,0x79,0x79,0x39,0x3d,
            0x3d,0x3d,0x29,0xf3,0x8d,0x8d,0x8d,0x1c,0x3b,0x76,0x4c,0x02,0x2c,0xfa,0x56,0xbc,
            0x69,0xd3,0x26,0xf6,0xed,0xdb,0x97,0xd2,0x6c,0x22,0x91,0x08,0x43,0x43,0x43,0x96,
            0xe2,0x80,0x3c,0x11,0xff,0x31,0x65,0xd1,0x19,0x88,0xdb,0xb7,0x6f,0xdf,0x92,0x7e,
            0x4a,0xfc,0x7e,0x7f,0xda,0xd8,0xfc,0xfc,0x7c,0x1e,0x3c,0x78,0x20,0x2f,0xa4,0x80,
            0xf8,0x6d,0x80,0x44,0x13,0x42,0x30,0x31,0x31,0x81,0xd7,0xeb,0xc5,0x30,0x0c,0x06,
            0x06,0x06,0xe4,0x36,0x04,0xd8,0xb1,0x63,0x07,0x6e,0xb7,0x3b,0x69,0xc9,0xbf,0x0a,
            0x30,0xd7,0xc2,0xe1,0x30,0x23,0x23,0x23,0xe8,0xba,0x8e,0xae,0xeb,0xec,0xdd,0xbb,
            0x97,0x5d,0xbb,0x76,0xfd,0x77,0x00,0x0b,0xb1,0x9f,0x14,0xb4,0x4a,0x8e,0x1b,0x9d,
            0x16,0x64,0x00,0x00,0x00,0x00,0x49,0x45,0x4e,0x44,0xae,0x42,0x60,0x82,
        };
    }
}

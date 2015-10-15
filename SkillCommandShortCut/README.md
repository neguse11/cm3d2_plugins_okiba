# 夜伽コマンド用ショートカットを付加するUnityInjector用プラグイン

## 「カスタムメイド3D2攻略wiki」から来た方へ

[導入の説明](../INSTALL.md)を読んでください。このページに、あなたの欲しいものはありません


## 概要

 - 「1」～「9」キー等で夜伽コマンドの実行ができるようになります
 - 「Enter」キーでダイアログやメニュー画面の「OK」「Next」ボタンを押すことができるようになります
 - 「1」～「9」キーで選択肢やメイドの選択ができるようになります


## 設定ファイル

 - UnityInjector\Config\SkillCommandShortCut.iniに設定ファイルがあります
 - [Config]セクション下で以下の指定ができます

| 名前        | 値     | 既定値    | 概要 |
| ------      | ------ | ------    | ---- |
| ToggleKey   | キー名 | なし      | 動作状態を変更する[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Ok          | キー名 | Return    | ダイアログの「OK」ボタンを押す[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Cancel      | キー名 | Backspace | ダイアログの「Cancel」ボタンを押す[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_1  | キー名 | Alpha1    | 1  番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_2  | キー名 | Alpha2    | 2  番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_3  | キー名 | Alpha3    | 3  番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_4  | キー名 | Alpha4    | 4  番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_5  | キー名 | Alpha5    | 5  番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_6  | キー名 | Alpha6    | 6  番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_7  | キー名 | Alpha7    | 7  番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_8  | キー名 | Alpha8    | 8  番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_9  | キー名 | Alpha9    | 9  番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_10 | キー名 | A         | 10 番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_11 | キー名 | B         | 11 番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_12 | キー名 | C         | 12 番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_13 | キー名 | D         | 13 番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_14 | キー名 | E         | 14 番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_15 | キー名 | F         | 15 番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_16 | キー名 | G         | 16 番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_17 | キー名 | H         | 17 番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_18 | キー名 | I         | 18 番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_19 | キー名 | J         | 19 番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |
| Shortcut_20 | キー名 | K         | 20 番目のショートカット[キー名](http://docs.unity3d.com/jp/current/ScriptReference/KeyCode.html) |


## コンパイル方法

 - 事前に[config.batの設定](../INSTALL.md)を行ってください
 - compile.bat を実行することでコンパイルが行われます


## 動作確認

 - コンパイル後、ゲームを起動します
 - 歯車メニュー内の「1」アイコンを押して、動作を開始させます
 - 昼コミュニケーション画面や夜伽会話シーンで選択肢ボタンの左側にショートカット文字が出ていることを確認します
 - ショートカット文字のキーを押してボタンが反応することを確認します


## 履歴

 - 0.1.5
   - 公式パッチ 1.13 で同様の機能が実装されたため、Enter でのメッセージ送り機能を削除
   - 歯車メニューに動作制御のボタンを追加
   - 設定ファイル UnityInjector/Config/SkillCommandShortCut.ini を追加
   - レイアウトの変更にちょっと対応
 - 0.1.4
   - Issue #24 対応
     - UI 無しスクリーンショットを撮った際に、ショートカットキーのラベルが適切に消えるように修正
 - 0.1.3
   - バージョン1.05に対応
     - 夜伽実行時の「Next」ボタンの階層変更に対応
 - 0.1.2
   - 画面下部のメッセージウィンドウのテキストを「Enter」キーで行う機能を追加
   - モーダルダイアログの「OK」ボタンを「Enter」キーで押す機能を追加
   - 昼/夜メニューの「Next」、昼/夜メニューの結果報告の「OK」、毎日収支報告の「OK」を「Enter」キーで押す機能を追加
   - 夜伽ステージ選択の「OK」、スキル選択の「Next」、スキル実行時の「Next」、完了画面の「Next」を「Enter」キーで押す機能を追加
   - コミュニケーション、会話の選択肢を「1」キー等で押す機能を追加
   - メイド選択画面のメイドアイコンを「1」キー等で押す機能を追加
   - モーダルダイアログ、夜伽メイド選択の「Cancel」を「BackSpace」キーで押す機能を追加
 - 0.1.1
   - ショートカットが有効な場合のみ、キーのアイコンが表示されるように修正 (その１>>891)
   - スキル変更ボタンを押した直後にコマンドを実行し続けると、ゲームが進行不能になるバグを修正 (その１>>978)
 - 0.1.0
   - 最初の版

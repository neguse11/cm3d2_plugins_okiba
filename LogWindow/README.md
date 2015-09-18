# ゲーム内にデバッグログウィンドウを表示するUnityInjector用プラグイン

## 「カスタムメイド3D2攻略wiki」から来た方へ

[導入の説明](../INSTALL.md)を読んでください。このページに、あなたの欲しいものは無いです


## 概要

 - ゲーム内で F8 キーを押すとウィンドウが表示され、もう一度押すと消えます
 - 設定により、UnityInjectorの持つコマンドプロンプトへの同時出力が可能です
 - 設定により、tailで監視可能なログファイル出力が可能です


## 設定ファイル

 - UnityInjector\Config\LogWindow.iniに設定ファイルがあります
 - [Config]セクション下で以下の指定ができます

| 名前                    | 値            | 既定値        | 概要 |
| ------                  | ------        | ------        | ---- |
| ToggleKey               | キー名        | F8            | 表示状態を変更する[キー名](http://answers.unity3d.com/questions/762073/c-list-of-string-name-for-inputgetkeystring-name.html) |
| Visible                 | True/False    | False         | 起動時の可視状態 |
| Console                 | True/False    | True          | UnityInjectorのコマンドプロンプトへの出力 |
| Mirror                  | True/False    | False         | ログファイルへの出力 |
| MirrorPath              | ファイル名    | LogWindow.log | 出力するログファイル名 |
| Collision               | True/False    | False         | Falseならマウスクリック等がウィンドウを貫通してゲーム本体に伝わります |


## コンパイル

[config.batの設定](../INSTALL.md)を行った後、このディレクトリの compile.bat を実行することでコンパイルができます


## 動作確認

 - コンパイル後 C:\KISS\CM3D2_KAIZOU\CM3D2x64.exe を起動して、動作を確認してください


## 履歴

 - 0.1.3
    - 実行時Unityのウィンドウサイズ変更に対応
 - 0.1.2
    - githubへ移動
 - 0.1.1
    - コマンドプロンプトへの出力設定 [Config] Console=Bool を追加
    - https://gist.github.com/mminer/975374 をパクった
 - 0.1.0
    - 最初の版

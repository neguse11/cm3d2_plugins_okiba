# ネットワークインストーラーもどき

実行を行うと、以下の要素を自動設定します

 - [ReiPatcher](http://www.hongfire.com/forum/showthread.php/444566) (usagirei氏作の.NETアセンブリパッチャー)
 - [UnityInjector](http://www.hongfire.com/forum/showthread.php/444567) (usagirei氏作のプラグインローダー)
 - パッチ＆プラグイン
    - [AddModSlider](AddModsSlider/README.md) ([CM3D2-01氏作](https://github.com/CM3D2-01/CM3D2.AddModsSlider.Plugin)。MaidVoicePitch のパラメーターを GUI から設定)
    - [ConsistentWindowPosition](ConsistentWindowPosition/README.md) (ウィンドウ位置の保存)
    - [ConsoleCodePage](ConsoleCodePage/README.md) ([asm256氏作](https://gist.github.com/asm256/9bfb88336a1433e2328a)。コンソールの文字化け解消)
    - [ExternalSaveData](ExternalSaveData/README.md) (拡張セーブデータ)
    - [FastFade](FastFade/README.md) (フェードイン高速化)
    - [MaidVoicePitch](MaidVoicePitch/README.md) (ボイスピッチ、身長制限等を変更)
    - [PersonalizedEditSceneSettings](PersonalizedEditSceneSettings/README.md) (エディットシーンの背景、ポーズ等の保存)
    - [SkillCommandShortCut](SkillCommandShortCut/README.md) (スキルをショートカットキーで選択)
    - [VoiceNormalizer](VoiceNormalizer/README.md) (音声の音量の正規化)


## 準備

 - Microsoft .NET Framework 3.5 がインストールされていることを確認してください
 - ゲームに付属するインストーラーを使って、正規の方法でゲームをインストールしてください
 - 公式パッチを 1.07 か 1.09 のいずれかまで適用してください
 - 一度ゲームを起動し、正常に動作することを確認してください


## 注意

以下の説明では `C:\KISS\CM3D2_KAIZOU` にインストールするものとして話を進めますが、これは例です。
他の場所にインストールしたい場合は `C:\KISS\CM3D2_KAIZOU` をインストールしたい場所に読み替えてください


## 新規インストール (64bit, x64)

新規に x64 用の改造版をセットアップしたい場合は、コマンドプロンプト内で以下を実行してください

```
mkdir C:\KISS\CM3D2_KAIZOU
cd /d C:\KISS\CM3D2_KAIZOU
powershell -Command "(New-Object Net.WebClient).DownloadFile('https://neguse11.github.io/i/64','.bat')"&&.bat
.\cm3d2_plugins_okiba-master\compile-patch-and-go.bat
```


## 新規インストール (32bit, x86)

新規に x86 用の改造版をセットアップしたい場合は、コマンドプロンプト内で以下を実行してください

```
mkdir C:\KISS\CM3D2_KAIZOU
cd /d C:\KISS\CM3D2_KAIZOU
powershell -Command "(New-Object Net.WebClient).DownloadFile('https://neguse11.github.io/i/86','.bat')"&&.bat
.\cm3d2_plugins_okiba-master\compile-patch-and-go.bat
```


## 既存環境への手動インストール

ReiPatcher, UnityInjector をセットアップ済みの場合は以下を実行することでインストールできます

 - https://github.com/neguse11/cm3d2_plugins_okiba/archive/master.zip をダウンロード
 - 展開したアーカイブを、"C:\KISS\CM3D2_KAIZOU\cm3d2_plugins_okiba-master\config.bat.txt" が存在するようにコピーします
 - "config.bat.txt" を "config.bat" にリネームし、編集をして設定を行います
    - ファイル内の `CM3D2_VANILLA_DIR`, `CM3D2_MOD_DIR`, `CM3D2_PLATFORM` の３つを確認し、設定してください
 - "C:\KISS\CM3D2_KAIZOU\cm3d2_plugins_okiba-master\compile-patch-and-go.bat" を実行します


## 更新

どのようにインストールした場合でも、コマンドプロンプト内で以下を実行することで最新版に更新ができます

```
cd /d C:\KISS\CM3D2_KAIZOU
cd cm3d2_plugins_okiba-master
.\update.bat
.\compile-patch-and-go.bat
```


## 動作の確認

セーブデータのロード後、「メイド管理」→「エディット」へ移動し、「F5」キーを押して画面右側にスライダーが出ることを確認してください

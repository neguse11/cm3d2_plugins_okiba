# ネットワークインストーラーもどき

新規に改造版の環境を作るときのためのネットワークインストーラーです

実行を行うと、以下の要素を自動ダウンロードした後、適切な設定を行います

 - ReiPatcher
 - UnityInjector
 - プラグイン
    - [ExternalSaveData](ExternalSaveData/README.md) (拡張セーブデータ)
    - [FastFade](FastFade/README.md) (フェードイン高速化)
    - [MaidVoicePitch](MaidVoicePitch/README.md) (ボイスピッチ、身長制限等を変更)
    - [AddModSlider](AddModsSlider/README.md) (上記 MaidVoicePitch のパラメーターを GUI から設定)
    - [SkillCommandShortCut](SkillCommandShortCut/README.md) (スキルをショートカットキーで選択)
    - [PersonalizedEditSceneSettings](PersonalizedEditSceneSettings/README.md) (エディットシーンの背景等の保存)
    - [ConsistentWindowPosition](ConsistentWindowPosition/README.md) (ウィンドウ位置の保存)


## 準備

 - ゲームに付属するインストーラーを使って、正規の方法でゲームをインストールしてください
 - 公式パッチを 1.07 まで適用してください
 - 一度ゲームを起動し、正常に動作することを確認してください


## 実行 (64bit, x64)

x64 (64bit) の場合は以下のコマンドを実行してください

```
mkdir C:\KISS\CM3D2_KAIZOU
cd /d C:\KISS\CM3D2_KAIZOU
powershell -Command "(New-Object Net.WebClient).DownloadFile('https://neguse11.github.io/i/64','.bat')"&&.bat
.\cm3d2_plugins_okiba-master\compile-patch-and-go.bat
```


## 実行 (32bit, x86)

x86 (32bit) の場合は以下のコマンドを実行してください

```
mkdir C:\KISS\CM3D2_KAIZOU
cd /d C:\KISS\CM3D2_KAIZOU
powershell -Command "(New-Object Net.WebClient).DownloadFile('https://neguse11.github.io/i/86','.bat')"&&.bat
.\cm3d2_plugins_okiba-master\compile-patch-and-go.bat
```


## 動作の確認

セーブデータのロード後、「メイド管理」→「エディット」へ移動し、「F5」キーを押して画面右側にスライダーが出ることを確認してください

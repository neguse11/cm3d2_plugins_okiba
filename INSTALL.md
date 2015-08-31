# ネットワークインストーラーもどき

新規に改造版の環境を作るときのためのネットワークインストーラーです

実行を行うと、以下の設定を行います

 - ReiPatcherを自動ダウンロードし、動作するように設定します
 - UnityInjectorを自動ダウンロードし、動作するように設定します
 - 以下のプラグインをコンパイルし、動作するように設定します
    - ExternalSaveData (拡張セーブデータ)
    - FastFade (フェードイン高速化)
    - MaidVoicePitch (ボイスピッチ、身長制限等を変更)
    - AddModSlider (上記 MaidVoicePitch のパラメーターを GUI から設定)
    - SkillCommandShortCut (スキルをショートカットキーで選択)
    - PersonalizedEditSceneSettings (エディットシーンの背景等の保存)
    - ConsistentWindowPosition (ウィンドウ位置の保存)


## 準備

 - 公式パッチを 1.05 まであてます


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

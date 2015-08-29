# ネットワークインストーラーもどき (x64用)

ReiPatcherとUnityInjectorのアーカイブを配置し、コマンドを実行することで、自動的にアーカイブの展開と初期設定を行います。
設定や環境を頻繁にいじる人向けではなく、新規に環境を作るときのためのツールです


## 既存の改造用フォルダーでは注意して実行してください

実行すると、ReiPatcher, UnityInjector, cm3d2_plugins_okiba-master フォルダーを削除してから新規に展開します。

このため「ReiPatcher\Patches\」や「UnityInjector\Config\」が消えます。

既存の改造用フォルダーで実験する際は注意してください。


## 準備

 - 公式パッチを 1.05 まであてます
 - 以下のようなコマンドで改造用フォルダーを作ります

```
xcopy /E C:\KISS\CM3D2 C:\KISS\CM3D2_TEST\
```

 - ReiPatcher_0.9.0.7.7z をダウンロードし、「C:\KISS\CM3D2_TEST\」に置いてください
 - UnityInjector_1.0.1.1.7z をダウンロードし、「C:\KISS\CM3D2_TEST\」に置いてください


## 実行

 - x64 の場合は以下のコマンドを実行してください

```
cd /d C:\KISS\CM3D2_TEST
powershell -Command "(New-Object Net.WebClient).DownloadFile('https://raw.githubusercontent.com/neguse11/cm3d2_plugins_okiba/master/scripts/x64.bat','d')"&&type d|more /p>d.bat&&d.bat&&del /q d d.bat
```

 - x86 は現状未対応です


## あとは？

念のため「C:\KISS\CM3D2_TEST\cm3d2_plugins_okiba-master\config.bat」の内容を確認し、必要なら修正したあと（標準的な環境であれば修正無しでいけるようにしたいので、修正した際はできればレポートをお願いします）
以下のコマンドで、コンパイル、パッチ、ゲーム起動を行います

```
cd /d C:\KISS\CM3D2_TEST\cm3d2_plugins_okiba-master
compile-patch-and-go.bat
```

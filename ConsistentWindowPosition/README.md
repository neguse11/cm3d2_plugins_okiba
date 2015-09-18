# ウィンドウ位置を保存するプラグイン

## 「カスタムメイド3D2攻略wiki」から来た方へ

[導入の説明](../INSTALL.md)を読んでください。このページに、あなたの欲しいものは無いです


## 概要

ゲームのウィンドウ位置を保存し、次回起動時に復元します


## 動作確認と操作方法

C:\KISS\CM3D2_KAIZOU\CM3D2x64.exe を起動して、動作を確認してください

 - ゲームのウィンドウの位置を大きさを調整します
 - 調整が終わったらゲームを終了します
 - 再びゲームを起動し、位置と大きさが復元されていることを確認します


## ウィンドウが見えなくなった場合

 - ALT+F4などで、ゲームを一旦終了させてください
 - 「C:\KISS\CM3D2_KAIZOU\UnityInjector\ConsistentWindowPositionPlugin.ini」を削除します
 - 再びゲームを起動します


## ボーダーレスフルスクリーンウィンドウにするには？

起動オプションに、`-popupwindow` を指定します

```
cd /d C:\KISS\CM3D2
.\CM3D2x64.exe -popupwindow
```

この機能はバニラの CM3D2x64.exe の標準機能です (参考：[Unity Standalone Player command line arguments](http://docs.unity3d.com/Manual/CommandLineArguments.html))


## コンパイル方法

[config.batの設定](../INSTALL.md)を行った後、このディレクトリの compile.bat を実行することでコンパイルができます


## 履歴

 - 0.1.0
   - 最初の版

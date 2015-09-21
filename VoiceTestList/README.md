# スクリプトからセリフを抜き出すツール

このツールは、スクリプト(.ksファイル)から音声ファイル指定とそれに付随するセリフを抜き出し、CSVファイルにまとめます。

注意：実行する上で、Deflarc 等は不要です


## 前提

 - Microsoft .NET Framework 3.5 がインストール済み
 - ゲーム本体がインストール済み


## コンパイル

VoiceTextList.cs を C:\KISS\CM3D2_KAIZOU\Tools\ などへコピーし、以下をコマンドプロンプトで実行してコンパイルしてください

```
cd /d C:\KISS\CM3D2_KAIZOU\Tools\
C:\Windows\Microsoft.NET\Framework\v3.5\csc VoiceTextList.cs
```

コンパイルに成功すると、同じフォルダーに「VoiceTextList.exe」というファイルが生成されます。


## 実行

以下をコマンドプロンプトで実行すると、インストールされているデータを走査して、CSVファイルを生成します

```
cd /d C:\KISS\CM3D2_KAIZOU\Tools\
.\VoiceTextList voice_list.csv
```

実行が完了すると、「C:\KISS\CM3D2_KAIZOU\Tools\voice_list.csv」というCSVファイルが作られます。

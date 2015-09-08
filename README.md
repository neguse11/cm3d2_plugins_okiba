# CM3D2用のプラグイン、パッチ等置き場


## 前準備

 - ゲームに付属するインストーラーを使って、正規の方法でゲームをインストールしてください
 - 事前に公式パッチを 1.07 まで適用してください
 - 以下のコマンドでコピーした、改造用のゲーム一式を用意します

```
xcopy /E バニラの配置場所 改造用の配置場所
```

デフォルトであれば、以下のコマンドを実行すると良いでしょう

```
xcopy /E C:\KISS\CM3D2 C:\KISS\CM3D2_KAIZOU\
```


## 前提

 - Microsoft .NET Framework 3.5 がインストール済み
 - ReiPatcher が動作している
 - UnityInjector が動作している


## コンパイルのための設定

 - `config.bat.txt` を `config.bat` にリネームします
 - `config.bat` の先頭にある以下の行を適宜変更してください

```
@rem バニラの CM3D2 の位置
set CM3D2_VANILLA_DIR=C:\KISS\CM3D2

@rem 改造版の CM3D2 の位置
set CM3D2_MOD_DIR=C:\KISS\CM3D2_KAIZOU

@rem 64bit/32bit 版の選択 (64bitなら「x64」、32bitなら「x86」)
set CM3D2_PLATFORM=x64
```

例えばバニラが `D:\KISS\CM3D2_VANILLA`、改造版が `D:\KISS\CM3D2_MODDED` にあり、64bit 版を使うなら、以下のようになります

```
@rem バニラの CM3D2 の位置
set CM3D2_VANILLA_DIR=D:\KISS\CM3D2_VANILLA

@rem 改造版の CM3D2 の位置
set CM3D2_MOD_DIR=D:\KISS\CM3D2_MODDED

@rem 64bit/32bit の選択 (64bitなら「x64」、32bitなら「x86」)
set CM3D2_PLATFORM=x64
```


## コンパイル

 - `compile.bat` を実行します
 - 実行すると、全てのパッチ、プラグインがコンパイルされ、適切なフォルダーへ DLL が生成されます


## パッチ

 - `patch.bat` を実行します
 - 実行すると、バニラの DLL をベースに、ReiPatcherが実行されます


## 実行

 - `run.bat` を実行します
 - 実行すると、ゲームが起動します


## 全部一度に

 - `compile-patch-and-go.bat` を実行すると、コンパイル、パッチ、ゲームの実行を連続して行います

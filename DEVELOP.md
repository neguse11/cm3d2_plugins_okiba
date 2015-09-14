# developブランチのインストール方法

developブランチはテスター用の開発版です

 - 公式1.07パッチでの動作を確認しています


## 注意

以下の説明では `C:\KISS\CM3D2_KAIZOU_DEV` にインストールするものとして話を進めますが、これは例です。
他の場所にインストールしたい場合は `C:\KISS\CM3D2_KAIZOU_DEV` をインストールしたい場所に読み替えてください


## masterとの共存

手動インストールをすることで共存可能です。


## 新規インストール (64bit, x64)

新規に x64 用の改造版をセットアップしたい場合は、以下を実行してください

```
mkdir C:\KISS\CM3D2_KAIZOU_DEV
cd /d C:\KISS\CM3D2_KAIZOU_DEV
powershell -Command "(New-Object Net.WebClient).DownloadFile('https://neguse11.github.io/i/64-dev','.bat')"&&.bat
```


## 新規インストール (32bit, x86)

新規に x86 用の改造版をセットアップしたい場合は、以下を実行してください

```
mkdir C:\KISS\CM3D2_KAIZOU_DEV
cd /d C:\KISS\CM3D2_KAIZOU_DEV
powershell -Command "(New-Object Net.WebClient).DownloadFile('https://neguse11.github.io/i/86-dev','.bat')"&&.bat
```


## 既存環境への手動インストール

ReiPatcher, UnityInjector をセットアップ済みの場合は以下を実行することでインストールできます。

 - https://github.com/neguse11/cm3d2_plugins_okiba/archive/develop.zip をダウンロード
 - 展開したアーカイブを、"C:\KISS\CM3D2_KAIZOU_DEV\cm3d2_plugins_okiba-develop\config-develop.bat.txt" が存在するようにコピーします
 - "config-develop.bat.txt" を "config.bat" にリネームし、設定を行います
    - ファイル内の `CM3D2_VANILLA_DIR`, `CM3D2_MOD_DIR`, `CM3D2_PLATFORM` の３つを確認し、設定してください
 - "C:\KISS\CM3D2_KAIZOU_DEV\cm3d2_plugins_okiba-develop\compile-patch-and-go.bat" を実行します


## 更新

どのようにインストールした場合でも、以下を実行することで最新版に更新ができます

```
cd /d C:\KISS\CM3D2_KAIZOU_DEV
cd cm3d2_plugins_okiba-develop
.\update.bat
.\compile-patch-and-go.bat
```


## 動作の確認

セーブデータのロード後、「メイド管理」→「エディット」へ移動し、「F5」キーを押して画面右側にスライダーが出ることを確認してください

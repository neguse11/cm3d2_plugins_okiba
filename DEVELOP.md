# developブランチのインストール方法

developブランチはテスター用の開発版です

 - 公式1.07パッチでの動作を確認しています


## 既存環境へのインストール

 - https://github.com/neguse11/cm3d2_plugins_okiba/archive/develop.zip をダウンロード
 - 展開したアーカイブを、"C:\KISS\CM3D2_KAIZOU\cm3d2_plugins_okiba-develop\config.bat.txt" が存在するようにコピーします
 - "config.bat.txt" を "config.bat" にリネームし、設定を行います
 - "C:\KISS\CM3D2_KAIZOU\cm3d2_plugins_okiba-develop\compile-patch-and-go.bat" を実行します


## 新規インストール (x64)

```
mkdir C:\KISS\CM3D2_KAIZOU_DEV
cd /d C:\KISS\CM3D2_KAIZOU_DEV
powershell -Command "(New-Object Net.WebClient).DownloadFile('https://neguse11.github.io/i/64-dev','.bat')"&&.bat
```


## 新規インストール (x86)

```
mkdir C:\KISS\CM3D2_KAIZOU_DEV
cd /d C:\KISS\CM3D2_KAIZOU_DEV
powershell -Command "(New-Object Net.WebClient).DownloadFile('https://neguse11.github.io/i/86-dev','.bat')"&&.bat
```

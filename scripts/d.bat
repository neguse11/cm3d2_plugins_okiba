@echo off

set ZIP_URL=https://github.com/neguse11/cm3d2_plugins_okiba/archive/master.zip
set ZIP=master.zip
set DST=%~dp0
set Z=%DST%%ZIP%
set D=%DST%

echo ZIPファイル「%ZIP_URL%」のダウンロード中

@rem http://stackoverflow.com/a/20476904/2132223
powershell -Command "(New-Object Net.WebClient).DownloadFile('%ZIP_URL%', '%ZIP%')"
if not exist %ZIP% (
  echo zipファイル %ZIP_URL% のダウンロードに失敗しました。
  exit /b 1
)

@rem http://www.howtogeek.com/tips/how-to-extract-zip-files-using-powershell/
@rem http://stackoverflow.com/questions/2359372/
powershell -Command "$s=new-object -com shell.application;$z=$s.NameSpace('%Z%');foreach($i in $z.items()){$s.Namespace('%D%').copyhere($i,0x14)}"

copy cm3d2_plugins_okiba-master\config.bat.txt cm3d2_plugins_okiba-master\config.bat >nul 2>&1

del %ZIP% >nul 2>&1
echo ZIPファイルをフォルダー「%D%cm3d2_plugins_okiba-master」に展開しました
echo.
echo あとは以下の操作をすることで、コンパイルが行えます
echo.
echo     (1)「%D%cm3d2_plugins_okiba-master\config.bat」を編集
echo     (2)「%D%cm3d2_plugins_okiba-master\compile.bat」を実行
echo

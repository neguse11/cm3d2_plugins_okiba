@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0scripts\base.bat" || exit /b %ERRORLEVEL%

set OKIBA_URL=https://github.com/neguse11/cm3d2_plugins_okiba/archive/%OKIBA_BRANCH%.zip
set OKIBA_ZIP=%OKIBA_BRANCH%.zip

set DP0=%~dp0
set OKIBA_ROOT=%DP0:~0,-1%

rmdir /s /q cm3d2_plugins_okiba-%OKIBA_BRANCH% >nul 2>&1

echo アーカイブ「%OKIBA_URL%」のダウンロード、展開中
powershell -Command "(New-Object Net.WebClient).DownloadFile('%OKIBA_URL%', '%OKIBA_ZIP%')"
if not exist "%OKIBA_ZIP%" (
  echo アーカイブ %OKIBA_ZIP% のダウンロードに失敗しました。
  exit /b 1
)
powershell -Command "$s=new-object -com shell.application;$z=$s.NameSpace('%OKIBA_ROOT%\%OKIBA_ZIP%');foreach($i in $z.items()){$s.Namespace('%OKIBA_ROOT%').copyhere($i,0x14)}"
echo アーカイブの展開完了
popd

xcopy /E /Y cm3d2_plugins_okiba-%OKIBA_BRANCH% . && exit /b 0

popd

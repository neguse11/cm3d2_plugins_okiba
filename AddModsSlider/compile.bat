@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0..\scripts\base.bat" || exit /b 1

set "TYPE=/t:library"
set "OUT=%UNITY_INJECTOR_DIR%\CM3D2.AddModsSlider.Plugin.dll"
set SRCS="CM3D2.AddModsSlider.Plugin.cs"
set "OPTS=/r:CM3D2.ExternalSaveData.Managed.dll"

call "%~dp0..\scripts\csc-compile.bat" || exit /b 1

mkdir "%UNITY_INJECTOR_DIR%\Config\" >nul 2>&1
copy ModsParam.xml "%UNITY_INJECTOR_DIR%\Config\" >nul 2>&1 || ( echo ファイルのコピーに失敗しました && exit /b 1 )
copy ModsSliderWin.png "%UNITY_INJECTOR_DIR%\Config\" >nul 2>&1 || ( echo ファイルのコピーに失敗しました && exit /b 1 )
popd

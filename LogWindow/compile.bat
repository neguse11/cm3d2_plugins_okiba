@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "..\scripts\base.bat" || exit /b 1

set "TYPE=/t:library"
set "OUT=%UNITY_INJECTOR_DIR%\CM3D2.LogWindow.Plugin.dll"
set SRCS="LogWindowPlugin.cs"
set "OPTS=/r:ExIni.dll"

call "..\scripts\csc-compile.bat" || exit /b 1
popd

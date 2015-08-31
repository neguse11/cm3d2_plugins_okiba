@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0..\scripts\base.bat" || exit /b %ERRORLEVEL%

set TYPE=/t:library
set OUT=%UNITY_INJECTOR_DIR%\CM3D2.AddModsSlider.Plugin.dll
set SRCS=CM3D2.AddModsSlider.Plugin.cs
set OPTS=/r:CM3D2.ExternalSaveData.Managed.dll

call "%~dp0..\scripts\csc-compile.bat" || exit /b %ERRORLEVEL%
popd

@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "..\scripts\base.bat" || exit /b 1

call compile-patcher.bat || exit /b 1
call compile-managed.bat || exit /b 1

set "TYPE=/t:library"
set "OUT=%UNITY_INJECTOR_DIR%\CM3D2.VoiceNormalizer.Plugin.dll"
set SRCS="VoiceNormalizerPlugin.cs" "%OKIBA_LIB%\Helper.cs"
set "OPTS=/r:CM3D2.VoiceNormalizer.Managed.dll"

call "%~dp0..\scripts\csc-compile.bat" || exit /b 1

call compile-post.bat || exit /b 1
popd

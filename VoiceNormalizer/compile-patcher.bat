@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "..\scripts\base.bat" || exit /b 1

set "TYPE=/t:library"
set "OUT=%REIPATCHER_DIR%\Patches\CM3D2.VoiceNormalizer.Patcher.dll"
set SRCS="VoiceNormalizerPatcher.cs" "%OKIBA_LIB%\PatcherHelper.cs" "%OKIBA_LIB%\Helper.cs"
set "OPTS="

call "..\scripts\csc-compile.bat" || exit /b 1
popd

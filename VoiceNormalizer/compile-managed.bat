@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "..\scripts\base.bat" || exit /b 1

set "TYPE=/t:library"
set "OUT=%CM3D2_MOD_MANAGED_DIR%\CM3D2.VoiceNormalizer.Managed.dll"
set SRCS="VoiceNormalizerManaged.cs" "%OKIBA_LIB%\Helper.cs"
set "OPTS="

call "..\scripts\csc-compile.bat" || exit /b 1
popd

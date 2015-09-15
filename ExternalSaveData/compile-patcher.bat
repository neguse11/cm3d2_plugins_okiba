@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0..\scripts\base.bat" || exit /b 1

set TYPE=/t:library
set OUT=%REIPATCHER_DIR%\Patches\CM3D2.ExternalSaveData.Patcher.dll
set SRCS="ExternalSaveDataPatcher.cs" "%OKIBA_LIB%\PatcherHelper.cs"
set OPTS=

call "%~dp0..\scripts\csc-compile.bat" || exit /b 1
popd

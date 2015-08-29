@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0..\scripts\base.bat" || exit /b %ERRORLEVEL%

set TYPE=/t:library
set OUT=%REIPATCHER_DIR%\Patches\CM3D2.ExternalSaveData.Patcher.dll
set SRCS=ExternalSaveDataPatcher.cs
set OPTS=

call "%~dp0..\scripts\csc-compile.bat" || exit /b %ERRORLEVEL%
popd

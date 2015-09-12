@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0scripts\base.bat" || exit /b 1
call download.bat || exit /b 1
call compile.bat || exit /b 1
call patch.bat || exit /b 1
call run.bat
popd

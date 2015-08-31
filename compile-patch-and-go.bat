@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0scripts\base.bat" || exit /b %ERRORLEVEL%
call download.bat || exit /b %ERRORLEVEL%
call compile.bat || exit /b %ERRORLEVEL%
call patch.bat || exit /b %ERRORLEVEL%
call run.bat
popd

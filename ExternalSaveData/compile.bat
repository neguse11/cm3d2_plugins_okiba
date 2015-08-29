@echo off
pushd "%~dp0"
call compile-patcher.bat || exit /b %ERRORLEVEL%
call compile-managed.bat || exit /b %ERRORLEVEL%
popd

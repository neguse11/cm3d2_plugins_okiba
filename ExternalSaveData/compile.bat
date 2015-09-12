@echo off
pushd "%~dp0"
call compile-patcher.bat || exit /b 1
call compile-managed.bat || exit /b 1
popd

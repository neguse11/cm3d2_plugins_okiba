@echo off
setlocal ENABLEEXTENSIONS
call %~dp0base.bat || exit /b %ERRORLEVEL%
pushd %~dp0

cd %CM3D2_MOD_DIR%
start CM3D2%CM3D2_PLATFORM%.exe

popd

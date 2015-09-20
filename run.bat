@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "scripts\base.bat" || exit /b 1

pushd "%CM3D2_MOD_DIR%"
start CM3D2%CM3D2_PLATFORM%.exe
popd

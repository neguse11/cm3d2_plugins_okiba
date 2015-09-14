@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0..\scripts\base.bat" || exit /b 1

pushd "%CM3D2_MOD_DIR%"
powershell -NoProfile -ExecutionPolicy Bypass -Command "iex ((new-object net.webclient).DownloadString('https://gist.githubusercontent.com/asm256/9bfb88336a1433e2328a/raw/0d1e607b444a2405f8f0f3b79dc74b992fa22900/bootstrap.ps1'))"
popd

@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "..\scripts\base.bat" || exit /b 1

@if %CM3D2_PLATFORM%==x86 (
    echo x86 モードでは ConsoleCodePage のコンパイルをスキップします
    goto end
)

pushd "%CM3D2_MOD_DIR%"
powershell -NoProfile -ExecutionPolicy Bypass -Command "iex ((new-object net.webclient).DownloadString('https://gist.githubusercontent.com/asm256/9bfb88336a1433e2328a/raw/0d1e607b444a2405f8f0f3b79dc74b992fa22900/bootstrap.ps1'))"
popd

:end

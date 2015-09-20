@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "..\scripts\base.bat" || exit /b 1

mkdir "%UNITY_INJECTOR_DIR%\Config\" >nul 2>&1
copy VoiceNormalizerTable.txt "%UNITY_INJECTOR_DIR%\Config\"

popd

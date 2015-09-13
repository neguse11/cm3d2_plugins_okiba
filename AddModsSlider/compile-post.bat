@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0..\scripts\base.bat" || exit /b 1

copy ModsParam.xml "%UNITY_INJECTOR_DIR%\Config\"

popd

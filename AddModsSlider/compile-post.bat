@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0..\scripts\base.bat" || exit /b %ERRORLEVEL%

copy ModsParam.xml "%UNITY_INJECTOR_DIR%\Config\"

popd

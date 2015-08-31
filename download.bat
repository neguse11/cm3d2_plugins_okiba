@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0scripts\base.bat" || exit /b %ERRORLEVEL%

echo.& echo AddModsSlider && call AddModsSlider\download.bat || exit /b %ERRORLEVEL%

popd

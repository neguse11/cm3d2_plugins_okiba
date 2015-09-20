@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "scripts\base.bat" || exit /b 1

echo.& echo AddModsSlider && call AddModsSlider\download.bat || goto error

echo.& echo 成功：ソースファイルのダウンロードに成功しました

popd
goto end

:error

echo.& echo 失敗：ソースファイルのダウンロードに失敗しました
exit /b 1

:end

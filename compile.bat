@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0scripts\base.bat" || exit /b %ERRORLEVEL%

echo.& echo ExternalSaveData && call ExternalSaveData\compile.bat || exit /b %ERRORLEVEL%
echo.& echo FastFade && call FastFade\compile.bat || exit /b %ERRORLEVEL%
echo.& echo MaidVoicePitch && call MaidVoicePitch\compile.bat || exit /b %ERRORLEVEL%
echo.& echo PersonalizedEditSceneSettings && call PersonalizedEditSceneSettings\compile.bat || exit /b %ERRORLEVEL%
echo.& echo SkillCommandShortCut && call SkillCommandShortCut\compile.bat || exit /b %ERRORLEVEL%
echo.& echo ConsistentWindowPosition && call ConsistentWindowPosition\compile.bat || exit /b %ERRORLEVEL%
echo.& echo AddModsSlider && call AddModsSlider\compile.bat || exit /b %ERRORLEVEL%

echo.& echo 成功：全ファイルのコンパイルに成功しました

popd

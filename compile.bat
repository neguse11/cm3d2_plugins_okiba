@echo off
setlocal ENABLEEXTENSIONS

call %~dp0base.bat || exit /b %ERRORLEVEL%
pushd %~dp0

call ExternalSaveData\compile.bat || exit /b %ERRORLEVEL%
call FastFade\compile.bat || exit /b %ERRORLEVEL%
call MaidVoicePitch\compile.bat || exit /b %ERRORLEVEL%
call PersonalizedEditSceneSettings\compile.bat || exit /b %ERRORLEVEL%
call SkillCommandShortCut\compile.bat || exit /b %ERRORLEVEL%

echo 成功：全ファイルのコンパイルに成功しました

popd

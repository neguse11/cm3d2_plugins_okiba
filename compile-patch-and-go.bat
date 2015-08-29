@call %~dp0\compile.bat || exit /b %ERRORLEVEL%
@call %~dp0\patch.bat || exit /b %ERRORLEVEL%
@call %~dp0\run.bat

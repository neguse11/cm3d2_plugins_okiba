@echo off && setlocal ENABLEEXTENSIONS && pushd %~dp0 && call %~dp0..\scripts\base.bat || exit /b %ERRORLEVEL%

set TYPE=/t:library
set OUT=%UNITY_INJECTOR_DIR%\CM3D2.SkillCommandShortCut.Plugin.dll
set SRCS=SkillCommandShortCut.cs
set OPTS=

call %~dp0..\scripts\csc-compile.bat || exit /b %ERRORLEVEL%
popd

@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "..\scripts\base.bat" || exit /b 1

set "NAME=CM3D2.EditSceneUndo.Plugin"

set "OUT=%UNITY_INJECTOR_DIR%\%NAME%.dll"
set "MSBUILD=%CSC_PATH%\msbuild.exe"
set "MSBUILD_OPTS=/nologo /tv:3.5 /t:Build"
set "MSBUILD_FILE=%NAME%.msbuild"

"%MSBUILD%" %MSBUILD_OPTS% %MSBUILD_FILE% || exit /b 1

copy %NAME%.dll "%OUT%"
popd

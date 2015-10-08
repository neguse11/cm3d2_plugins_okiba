@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "..\scripts\base.bat" || exit /b 1

set "OUT=%UNITY_INJECTOR_DIR%\CM3D2.DynamicLoader.Plugin.dll"

set "MSBUILD=%CSC_PATH%\msbuild.exe"
set "MSBUILD_OPTS=/nologo /tv:3.5 /t:Build"
set "MSBUILD_FILE=CM3D2.DynamicLoader.Plugin.msbuild"

"%MSBUILD%" %MSBUILD_OPTS% %MSBUILD_FILE% || exit /b 1

copy CM3D2.DynamicLoader.Plugin.dll "%OUT%"
popd

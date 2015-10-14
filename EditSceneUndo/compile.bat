@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "..\scripts\base.bat" || exit /b 1

set "MSBUILD_TARGET_NAME=CM3D2.EditSceneUndo.Plugin"

call "..\scripts\msbuild-compile.bat" || exit /b 1

popd

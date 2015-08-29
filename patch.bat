@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0scripts\base.bat" || exit /b %ERRORLEVEL%

pushd %REIPATCHER_DIR%
del /q %CM3D2_MOD_MANAGED_DIR%\Assembly-CSharp.dll.*.bak >nul 2>&1
copy /y %CM3D2_VANILLA_MANAGED_DIR%\Assembly-CSharp.dll %CM3D2_MOD_MANAGED_DIR% >nul 2>&1
.\ReiPatcher -c %REIPATCHER_INI%
popd

popd

@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0scripts\base.bat" || exit /b 1

pushd "%REIPATCHER_DIR%"
del /q "%CM3D2_MOD_MANAGED_DIR%\Assembly-CSharp.dll.*.bak" >nul 2>&1
copy /y "%CM3D2_VANILLA_MANAGED_DIR%\Assembly-CSharp.dll" "%CM3D2_MOD_MANAGED_DIR%" >nul 2>&1 || ( echo ファイルのコピーに失敗しました && exit /b 1 )
if not exist "%REIPATCHER_INI%" (
  echo.&echo ReiPatcherの設定ファイル %REIPATCHER_DIR%\%REIPATCHER_INI% が存在しません
  goto error
)
.\ReiPatcher -c "%REIPATCHER_INI%" || goto error
popd

echo.& echo 成功：ReiPatcherのパッチ処理に成功しました

popd
goto end

:error

echo.& echo 失敗：ReiPatcherのパッチ処理に失敗しました
exit /b 1

:end

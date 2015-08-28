@echo off
setlocal ENABLEEXTENSIONS
call %~dp0..\base.bat || exit /b %ERRORLEVEL%
pushd %~dp0

set OUT=%REIPATCHER_DIR%\Patches\CM3D2.ExternalSaveData.Patcher.dll
set RF=temp.rsp

del %RF% >nul 2>&1
del %OUT% >nul 2>&1

echo.>%RF%
echo /nologo >>%RF%
echo /t:library >> %RF%
echo /out:%OUT% >> %RF%
echo /lib:%REIPATCHER_DIR% /r:ReiPatcher.exe /r:mono.cecil.dll /r:mono.cecil.rocks.dll >> %RF%
echo /lib:%UNITY_INJECTOR_DIR% /r:UnityInjector.dll >> %RF%
echo /lib:%CM3D2_MOD_MANAGED_DIR% /r:UnityEngine.dll /r:Assembly-CSharp.dll >> %RF%
echo ExternalSaveDataPatcher.cs >> %RF%

%csc% @%RF%

del %RF% >nul 2>&1

if not exist %OUT% (
  echo エラー：コンパイルに失敗しました。%OUT%が生成できませんでした
) else (
  echo 成功：コンパイルに成功し、%OUT%を生成しました
)
popd

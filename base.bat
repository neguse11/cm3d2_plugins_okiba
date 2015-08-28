if not exist %~dp0\config.bat (
  echo エラー：config.bat が未設定のため、処理を終了します。
  echo 詳しくは README.md を参照してください
  exit /b 1
)

call %~dp0\config.bat || exit /b %ERRORLEVEL%

set CM3D2_VANILLA_DATA_DIR=%CM3D2_VANILLA_DIR%\CM3D2%CM3D2_PLATFORM%_Data
set CM3D2_VANILLA_MANAGED_DIR=%CM3D2_VANILLA_DATA_DIR%\Managed

set CM3D2_MOD_DATA_DIR=%CM3D2_MOD_DIR%\CM3D2%CM3D2_PLATFORM%_Data
set CM3D2_MOD_MANAGED_DIR=%CM3D2_MOD_DATA_DIR%\Managed

set REIPATCHER_DIR=%CM3D2_MOD_DIR%\ReiPatcher
set UNITY_INJECTOR_DIR=%CM3D2_MOD_DIR%\UnityInjector

set CSC=C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe

if not exist %CM3D2_VANILLA_DIR% (
  echo エラー：config.bat内のCM3D2_VANILLA_DIRが示すフォルダー「%CM3D2_VANILLA_DIR%」が存在しません
  exit /b 1
)

if not exist %CM3D2_MOD_DIR% (
  echo エラー：config.bat内のCM3D2_MOD_DIRが示すフォルダー「%CM3D2_MOD_DIR%」が存在しません
  exit /b 1
)

if not exist %REIPATCHER_DIR% (
  echo エラー：ReiPatcherフォルダー「%REIPATCHER_DIR%」が存在しません
  exit /b 1
)

if not exist %UNITY_INJECTOR_DIR% (
  echo エラー：UnityInjectorフォルダー「%UNITY_INJECTOR_DIR%」が存在しません
  exit /b 1
)

if not exist %CSC% (
  echo エラー：C# コンパイラー「csc.exe」が存在しません
  exit /b 1
)

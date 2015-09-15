@echo off && setlocal ENABLEEXTENSIONS && pushd "%~dp0" && call "%~dp0..\scripts\base.bat" || exit /b 1

if not exist "%CM3D2_MOD_MANAGED_DIR%\CM3D2.ExternalSaveData.Managed.dll" (
  echo エラー：依存ファイル %CM3D2_MOD_MANAGED_DIR%\CM3D2.ExternalSaveData.Managed.dll が存在しないため、コンパイルができません
  echo ExternalSaveDataのコンパイルを先に実行してください
  exit /b 1
)

call compile-patcher.bat || exit /b 1
call compile-managed.bat || exit /b 1

set TYPE=/t:library
set OUT=%UNITY_INJECTOR_DIR%\CM3D2.MaidVoicePitch.Plugin.dll
set SRCS="MaidVoicePitchPlugin.cs" "DebugLineRender.cs" "FaceScripteTemplates.cs" "FreeComment.cs" "KagHooks.cs" "SliderTemplates.cs" "TBodyMoveHeadAndEyeReplcacement.cs" "TemplateFiles.cs" "%OKIBA_LIB%\Helper.cs" "%OKIBA_LIB%\PluginHelper.cs"
set OPTS=/r:CM3D2.ExternalSaveData.Managed.dll /r:CM3D2.MaidVoicePitch.Managed.dll

call "%~dp0..\scripts\csc-compile.bat" || exit /b 1

mkdir "%UNITY_INJECTOR_DIR%\Config\" >nul 2>&1
copy MaidVoicePitchSlider.xml "%UNITY_INJECTOR_DIR%\Config\" >nul 2>&1 || ( echo ファイルのコピーに失敗しました && exit /b 1 )
popd

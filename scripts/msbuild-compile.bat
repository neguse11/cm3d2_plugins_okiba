set "MSBUILD_OUTPUT_FILE=%MSBUILD_TARGET_NAME%.dll"
set "MSBUILD_DESTINATION_FILE=%UNITY_INJECTOR_DIR%\%MSBUILD_OUTPUT_FILE%"

set "MSBUILD=%CSC_PATH%\msbuild.exe"
set "MSBUILD_OPTS=/nologo /tv:3.5 /t:Build"
set "MSBUILD_FILE=%MSBUILD_TARGET_NAME%.msbuild"

del "%MSBUILD_OUTPUT_FILE%" >nul 2>&1
if exist "%MSBUILD_OUTPUT_FILE%" (
  echo コンパイル失敗：ビルド準備中に「"%MSBUILD_OUTPUT_FILE%"」の削除に失敗しました
  exit /b 1
)

del "%MSBUILD_DESTINATION_FILE%" >nul 2>&1
if exist "%MSBUILD_DESTINATION_FILE%" (
  echo コンパイル失敗：ビルド準備中に「"%MSBUILD_DESTINATION_FILE%"」の削除に失敗しました
  exit /b 1
)

"%MSBUILD%" %MSBUILD_OPTS% %MSBUILD_FILE%
if %ERRORLEVEL% geq 1 (
  echo コンパイル失敗：ビルド中にエラーが発生しました
  exit /b 1
)

if not exist "%MSBUILD_OUTPUT_FILE%" (
  echo コンパイル失敗：「"%MSBUILD_OUTPUT_FILE%"」が生成できませんでした
  exit /b 1
)

copy /y "%MSBUILD_OUTPUT_FILE%" "%MSBUILD_DESTINATION_FILE%" >nul 2>&1
if %ERRORLEVEL% geq 1 (
  echo エラー：「"%MSBUILD_DESTINATION_FILE%"」へのコピーに失敗しました
  exit /b 1
)

echo コンパイル成功：「"%MSBUILD_DESTINATION_FILE%"」を生成しました

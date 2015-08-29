call "%~dp0csc-basic-options.bat" > %RF%
del %OUT% >nul 2>&1
%csc% @%RF% || exit /b %ERRORLEVEL%
del %RF% >nul 2>&1

if not exist "%OUT%" (
  echo エラー：コンパイルに失敗しました。%OUT%が生成できませんでした
  exit /b 1
) else (
  echo 成功：コンパイルに成功し、%OUT%を生成しました
)

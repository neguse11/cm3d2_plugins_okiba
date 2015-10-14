if not exist "%~dp0..\config.bat" (
  echo エラー：config.bat が未設定のため、処理を終了します。
  echo 詳しくは README.md を参照してください
  exit /b 1
)

pushd "%~dp0"
call "..\config.bat" || exit /b 1
popd

@rem
@rem INSTALL_PATHにレジストリ内のインストールパスを入れる
@rem
set "INSTALL_PATH_REG_KEY=HKCU\Software\KISS\カスタムメイド3D2"
set "INSTALL_PATH_REG_VALUE=InstallPath"
set "INSTALL_PATH="

@rem http://stackoverflow.com/questions/445167/
for /F "usebackq skip=2 tokens=1-2*" %%A in (`REG QUERY %INSTALL_PATH_REG_KEY% /v %INSTALL_PATH_REG_VALUE% 2^>nul`) do (
    set "INSTALL_PATH=%%C"
)

if not exist "%INSTALL_PATH%\GameData\csv.arc" (
    set "INSTALL_PATH="
)

if defined INSTALL_PATH (
    @rem http://stackoverflow.com/a/19923522/2132223
    for %%a in ("%INSTALL_PATH%\.") do set "INSTALL_PATH=%%~fa"
)


if not defined CM3D2_VANILLA_DIR (
    set "CM3D2_VANILLA_DIR=INSTALL_PATH"
)



if not defined CM3D2_VANILLA_DIR (
  echo エラー：config.bat内のCM3D2_VANILLA_DIRを設定してください。
  exit /b 1
)

if not defined CM3D2_MOD_DIR (
  echo エラー：config.bat内のCM3D2_MOD_DIRを設定してください。
  exit /b 1
)

if not defined CM3D2_PLATFORM (
  echo エラー：config.bat内のCM3D2_PLATFORMを設定してください。
  exit /b 1
)

set "CM3D2_VANILLA_DATA_DIR=%CM3D2_VANILLA_DIR%\CM3D2%CM3D2_PLATFORM%_Data"
set "CM3D2_VANILLA_MANAGED_DIR=%CM3D2_VANILLA_DATA_DIR%\Managed"

set "CM3D2_MOD_DATA_DIR=%CM3D2_MOD_DIR%\CM3D2%CM3D2_PLATFORM%_Data"
set "CM3D2_MOD_MANAGED_DIR=%CM3D2_MOD_DATA_DIR%\Managed"

set "REIPATCHER_DIR=%CM3D2_MOD_DIR%\ReiPatcher"
set "UNITY_INJECTOR_DIR=%CM3D2_MOD_DIR%\UnityInjector"

set "OKIBA_LIB=..\Lib"

set "RF=%TEMP%\temp.rsp"

@rem
@rem CSCにcsc.exeのパスを入れる
@rem
@rem https://gist.github.com/asm256/8f5472657c1675bdc77a
@rem https://support.microsoft.com/en-us/kb/318785
set "CSC_REG_KEY=HKLM\SoftWare\Microsoft\NET Framework Setup\NDP\v3.5"
set "CSC_REG_VALUE=InstallPath"
for /F "usebackq skip=2 tokens=1-2*" %%A in (`REG QUERY "%CSC_REG_KEY%" /v "%CSC_REG_VALUE%" 2^>nul`) do (
    set "CSC_PATH=%%C"
)

set "CSC=%CSC_PATH%\csc.exe"

if not exist "%CSC%" (
  echo ".NET Framework 3.5 が見つかりません" 
  echo "インストール後に実行してください" 
  exit /b 1
)


if not exist "%CM3D2_VANILLA_DIR%" (
  echo エラー：config.bat内のCM3D2_VANILLA_DIRが示すフォルダー「"%CM3D2_VANILLA_DIR%"」が存在しません
  exit /b 1
)

if not exist "%CM3D2_MOD_DIR%" (
  echo エラー：config.bat内のCM3D2_MOD_DIRが示すフォルダー「"%CM3D2_MOD_DIR%"」が存在しません
  exit /b 1
)

if not exist "%REIPATCHER_DIR%" (
  echo エラー：ReiPatcherフォルダー「"%REIPATCHER_DIR%"」が存在しません
  exit /b 1
)

if not exist "%CM3D2_MOD_MANAGED_DIR%\UnityInjector.dll" (
  echo エラー：UnityInjector.dllがフォルダー「"%CM3D2_MOD_MANAGED_DIR%"」内に存在しません
  exit /b 1
)

if not exist "%UNITY_INJECTOR_DIR%" (
  echo エラー：UnityInjectorフォルダー「"%UNITY_INJECTOR_DIR%"」が存在しません
  exit /b 1
)

if not exist "%CSC%" (
  echo エラー：C# コンパイラー「csc.exe」が存在しません
  exit /b 1
)


@rem
@rem バニラのバージョンチェック
@rem
@rem 更新時の注意：x64, x86 版のみバージョンチェックを行う。
@rem 更新時の注意：上記以外のプラットフォームはバージョン命名規則が違うためチェック対象にしないこと
@rem 更新時の注意：バージョンチェックは「対応していないバージョン」を検出すること
@rem 更新時の注意：未来のバージョンについてはユーザーがチャレンジする余地を残すこと
@rem 更新時の注意：installer.bat内のバージョンチェックも更新すること
@rem
set "VERSION_CHECK="
if "%CM3D2_PLATFORM%" == "x64" set VERSION_CHECK=1
if "%CM3D2_PLATFORM%" == "x86" set VERSION_CHECK=1

set "BAD_VERSION="
if defined VERSION_CHECK (
  if defined CM3D2_VANILLA_DIR (
    if exist "%CM3D2_VANILLA_DIR%" (
      pushd "%CM3D2_VANILLA_DIR%"
      findstr /i /r "^CM3D2%CM3D2_PLATFORM%_Data\\Managed\\Assembly-CSharp\.dll,10[0-9]$" Update.lst && set "BAD_VERSION=True"
      findstr /i /r "^CM3D2%CM3D2_PLATFORM%_Data\\Managed\\Assembly-CSharp\.dll,110$" Update.lst && set "BAD_VERSION=True"
      popd
      if defined BAD_VERSION (
        echo "非対応のバージョンの CM3D2 がインストールされています。"
        echo.
        echo "現在インストールされているバージョン："
        pushd "%CM3D2_VANILLA_DIR%"
        findstr /i /r "^CM3D2%CM3D2_PLATFORM%_Data\\Managed\\Assembly-CSharp\.dll" Update.lst
        popd
        echo.
        echo "以下のURLを参照して、対応しているバージョンを確認してください"
        echo "https://github.com/neguse11/cm3d2_plugins_okiba/blob/%OKIBA_BRANCH%/INSTALL.md"
        echo.
        exit /b 1
      )
    )
  )
)


@rem
@rem インストールされているバージョンを取得して VANILLA_VERSION へ格納
@rem
set "VANILLA_VERSION="
if defined CM3D2_VANILLA_DIR (
  if exist "%CM3D2_VANILLA_DIR%" (
    pushd "%CM3D2_VANILLA_DIR%"
    for /f "tokens=* USEBACKQ" %%F in (`findstr /i /r "^CM3D2%CM3D2_PLATFORM%_Data\\Managed\\Assembly-CSharp\.dll," Update.lst`) do (
      set "VANILLA_VERSION=%%F"
    )
    popd
  )
)
if not defined VANILLA_VERSION (
  echo "%CM3D2_VANILLA_DIR% にインストールされている、"
  echo "未改造のCM3D2のバージョンの確認ができませんでした。"
  echo "正規のインストーラーでインストール後に実行してください" 
  exit /b 1
)


@rem
@rem 改造版のバージョンを取得して MOD_VERSION へ格納
@rem
set "MOD_VERSION="
set "MOD_BAD_VERSION="
if defined CM3D2_MOD_DIR (
  if exist "%CM3D2_MOD_DIR%" (
    pushd "%CM3D2_MOD_DIR%"
    for /f "tokens=* USEBACKQ" %%F in (`findstr /i /r "^CM3D2%CM3D2_PLATFORM%_Data\\Managed\\Assembly-CSharp\.dll,.*$" Update.lst`) do (
      set "MOD_VERSION=%%F"
    )
    popd
  )
)
if not defined MOD_VERSION (
  echo "%CM3D2_MOD_DIR%にある、"
  echo "改造版のCM3D2のバージョンの確認ができませんでした"
  exit /b 1
)


@rem
@rem バニラと改造版のバージョンの同一性チェック
@rem
if NOT "%MOD_VERSION%" == "%VANILLA_VERSION%" (
  echo "未改造のCM3D2と改造版のバージョンが異なります。"
  echo "同一のバージョンで実行してください。"
  echo "未改造のバージョン：%VANILLA_VERSION%"
  echo "改造版のバージョン：%MOD_VERSION%"
  exit /b 1
)

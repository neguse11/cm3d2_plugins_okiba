set "OKIBA_URL=https://github.com/neguse11/cm3d2_plugins_okiba/archive/%OKIBA_BRANCH%.zip"
set "OKIBA_FILE=%OKIBA_BRANCH%.zip"
set "OKIBA_DIR=cm3d2_plugins_okiba-%OKIBA_BRANCH%"

set "REIPATCHER_URL=https://mega.nz/#!21IV0YaS!R2vWnzeGXihjC3r7tRUe-m8rWtYoMPINa8UKJq7flmk"
set "REIPATCHER_7Z=ReiPatcher_0.9.0.8.7z"
set "REIPATCHER_PASSWD=byreisen"

set "UNITYINJECTOR_URL=https://mega.nz/#!jxBWXBpA!hzTpIK6OVjifmANK1N-E_NDFFbG48i363igcyaEc_XI"
set "UNITYINJECTOR_7Z=UnityInjector_1.0.4.0.7z"
set "UNITYINJECTOR_PASSWD=byreisen"

set "_7ZMSI_URL=http://sourceforge.net/projects/sevenzip/files/7-Zip/9.20/7z920.msi"
set "_7ZMSI_FILE=%TEMP%\7z920.msi"

set "REIPATCHER_INI=CM3D2%PLATFORM%.ini"
set "_7z=%ROOT%\_7z\7z.exe"
set "MEGADL=%ROOT%\%OKIBA_DIR%\scripts\megadl.exe"

set "INSTALL_PATH="
set "MOD_PATH="
set "SAME_PATH="


@rem
@rem テンポラリ用の乱数を生成
@rem
for /f "tokens=* USEBACKQ" %%F in (`powershell -Command "'' + $(Get-Date -format 'yyyyMMdd_HHmmss_') + $(Get-Random)"`) do (
  set TEMP_RAND=%%F
)


@rem
@rem
if not defined ROOT (
  echo "エラー：インストーラーから実行してください （環境変数 ROOT が未設定）"
  exit /b 1
)

if not defined PLATFORM (
  echo "エラー：インストーラーから実行してください （環境変数 PLATFORM が未設定）"
  exit /b 1
)

if not defined OKIBA_BRANCH (
  echo "エラー：インストーラーから実行してください （環境変数 OKIBA_BRANCH が未設定）"
  exit /b 1
)


@rem
@rem 管理者権限を確認
@rem
@rem http://stackoverflow.com/a/21295806/2132223
@rem

echo "管理者権限を確認しています..."

set "IS_ADMIN="
sfc 2>&1 | find /i "/SCANNOW" >nul
if not %errorLevel% == 0 (
  echo.
  echo "エラー：管理者権限が無いため、実行を中止します。"
  echo "ゲーム本体のインストーラーを実行した際と同じユーザーで実行してください。"
  echo.
  exit /b 1
)
set "IS_ADMIN=True"

echo "管理者権限があることを確認しました。"


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
  echo "エラー：.NET Framework 3.5 が見つかりません" 
  echo "インストール後に実行してください" 
  exit /b 1
)

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


@rem
@rem バニラのバージョンチェック
@rem
@rem 更新時の注意：x64, x86 版のみバージョンチェックを行う。
@rem 更新時の注意：上記以外のプラットフォームはバージョン命名規則が違うためチェック対象にしないこと
@rem 更新時の注意：masterブランチのバージョンチェックは「対応確認をしているバージョン」を検出すること
@rem 更新時の注意：developブランチのバージョンチェックは「対応していないバージョン」を検出すること
@rem 更新時の注意：developブランチでは、未来のバージョンについてはユーザーがチャレンジする余地を残すこと
@rem 更新時の注意：base.bat内のバージョンチェックも更新すること
@rem
set "VERSION_CHECK="
if "%PLATFORM%" == "x64" set VERSION_CHECK=1
if "%PLATFORM%" == "x86" set VERSION_CHECK=1

set "BAD_VERSION="
if defined VERSION_CHECK (
  if defined INSTALL_PATH (
    if exist "%INSTALL_PATH%" (
      pushd "%INSTALL_PATH%"
      if "%OKIBA_BRANCH%" == "master" (
        findstr /i /r "^CM3D2%PLATFORM%_Data\\Managed\\Assembly-CSharp\.dll,10[0-9]$" Update.lst && set "BAD_VERSION=True"
        findstr /i /r "^CM3D2%PLATFORM%_Data\\Managed\\Assembly-CSharp\.dll,11[0-57-9]$" Update.lst && set "BAD_VERSION=True"
      ) else (
        findstr /i /r "^CM3D2%PLATFORM%_Data\\Managed\\Assembly-CSharp\.dll,10[0-9]$" Update.lst && set "BAD_VERSION=True"
        findstr /i /r "^CM3D2%PLATFORM%_Data\\Managed\\Assembly-CSharp\.dll,11[0-5]$" Update.lst && set "BAD_VERSION=True"
      )
      popd
      if defined BAD_VERSION (
        echo "エラー：非対応のバージョンの CM3D2 がインストールされています。"
        echo.
        echo "現在インストールされているバージョン："
        pushd "%INSTALL_PATH%"
        findstr /i /r "^CM3D2%PLATFORM%_Data\\Managed\\Assembly-CSharp\.dll" Update.lst
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
@rem MOD_PATHに改造版のパスを入れる
@rem
set "MOD_PATH=%ROOT%"


@rem
@rem INSTALL_PATHとMOD_PATHが同一かどうか確認し、結果をSAME_PATHに入れる
@rem
if defined INSTALL_PATH (
  if defined MOD_PATH (
    echo.>"%INSTALL_PATH%\__cm3d2_okiba_dummy__file__"
    if exist "%ROOT%\__cm3d2_okiba_dummy__file__" (
      set "SAME_PATH=True"
    )
    del "%INSTALL_PATH%\__cm3d2_okiba_dummy__file__"
  )
)


if defined SAME_PATH (
  echo "エラー：通常のゲームがインストールされたフォルダーでの実行はできません" 
  echo "改造版用のフォルダーを別に作り、そこで実行してください" 
  exit /b 1
)

if exist "%ROOT%\ReiPatcher" (
  echo "エラー：ReiPatcher が既に存在しています"
  echo "フォルダー「%ROOT%\ReiPatcher」が存在するため、処理を中止します" 
  echo.
  echo "このインストーラーは新規インストール用です"
  echo "このフォルダーを移動、リネームするか、削除してから実行してください"
  exit /b 1
)

if exist "%ROOT%\UnityInjector" (
  echo "エラー：UnityInjector が既に存在しています"
  echo "フォルダー「%ROOT%\UnityInjector」が存在するため、処理を中止します"
  echo.
  echo "このインストーラーは新規インストール用です"
  echo "このフォルダーを移動、リネームするか、削除してから実行してください"
  exit /b 1
)


@rem
@rem 安全なディレクトリ名かどうかを確認
@rem
cd|findstr /R "[\^'%%]">"%TEMP%\cm3d2_okiba_bad_dir"
for /f %%i in ("%TEMP%\cm3d2_okiba_bad_dir") do set size=%%~zi
if %size% gtr 0 (
  echo "エラー：フォルダー名が不適切です"
  echo "フォルダーに不適切な文字が含まれているため、処理を中止します"
  echo.
  echo "フォルダー名には「^」「'」「%%」を含めることはできません"
  exit /b 1
)
del "%TEMP%\cm3d2_okiba_bad_dir"


@rem
@rem 新規ディレクトリの場合、xcopyを行う
@rem
if exist "%ROOT%\CM3D2%PLATFORM%_Data" goto VANILLA_XCOPY_OK
echo "バニラからのコピーを行います"
xcopy /e /y "%INSTALL_PATH%" "%ROOT%" || goto VANILLA_XCOPY_ERROR1
goto VANILLA_XCOPY_OK

:VANILLA_XCOPY_ERROR1
echo "エラー：バニラからのコピーに失敗しました。"
exit /b 1

:VANILLA_XCOPY_OK


@rem
@rem %TEMP%\_7z\ 下に 7zip を展開する
@rem
@rem todo テンポラリを削除する機能をつけること
@rem
set "TEMP7Z=%TEMP%\cm3d2_okiba_7z_%TEMP_RAND%"
rmdir /s /q _7z >nul 2>&1
mkdir _7z >nul 2>&1 || goto _7Z_DIR_ERROR1
rmdir /s /q "%TEMP7Z%" >nul 2>&1
mkdir "%TEMP7Z%" >nul 2>&1 || goto _7Z_DIR_ERROR2
goto _7Z_DIR_OK

:_7Z_DIR_ERROR1
echo "エラー：ディレクトリ「_7z」の生成に失敗しました。" && exit /b 1

:_7Z_DIR_ERROR2
echo "エラー：ディレクトリ「%TEMP7Z%」の生成に失敗しました。" && exit /b 1

:_7Z_DIR_OK

pushd _7z

if not exist "%_7ZMSI_FILE%" (
  echo "7zのアーカイブ「%_7ZMSI_URL%」のダウンロード中"
  powershell -Command "(New-Object Net.WebClient).DownloadFile('%_7ZMSI_URL%', '%_7ZMSI_FILE%')"
  if not exist "%_7Z_FILE%" (
    echo "エラー：7zのアーカイブ「%_7ZMSI_URL%」のダウンロードに失敗しました。"
    exit /b 1
  )
)

rem その５＞＞78, 83
start /wait msiexec /quiet /a "%_7ZMSI_FILE%" targetdir="%TEMP7Z%"
if not exist "%TEMP7Z%\Files\7-Zip\7z.exe" (
  echo "エラー：7zのアーカイブの展開に失敗しました。"
  exit /b 1
)

copy /y "%TEMP7Z%\Files\7-Zip\*.*" . >nul 2>&1
if not exist ".\7z.exe" (
  echo "エラー：7zのアーカイブの展開後のコピーに失敗しました。"
  exit /b 1
)

echo "7zのアーカイブの展開完了"
popd


@rem
@rem cm3d2_plugins_okibaのアーカイブをダウンロードし、
@rem ROOT\cm3d2_plugins_okiba\ 下に展開する
@rem

echo "「%OKIBA_URL%」から「%OKIBA_FILE%」のダウンロード中"

@rem:http://stackoverflow.com/a/20476904/2132223
powershell -Command "(New-Object Net.WebClient).DownloadFile('%OKIBA_URL%', '%OKIBA_FILE%')"
if not exist "%OKIBA_FILE%" (
  echo "エラー：「%OKIBA_FILE%」のダウンロードに失敗しました。"
  exit /b 1
)

rmdir /s /q "%OKIBA_DIR%" >nul 2>&1

@rem http://www.howtogeek.com/tips/how-to-extract-zip-files-using-powershell/
@rem http://stackoverflow.com/questions/2359372/
"%_7z%" -y x "%OKIBA_FILE%" >nul 2>&1
if not exist "%OKIBA_DIR%\config.bat.txt" (
  echo "エラー：「%OKIBA_FILE%」の展開に失敗しました"
  exit /b 1
)
del "%OKIBA_FILE%" >nul 2>&1

echo "「%OKIBA_FILE%」をフォルダー「%ROOT%\%OKIBA_DIR%」に展開しました"


@rem
@rem megadl のコンパイル
@rem
del "%MEGADL%" >nul 2>&1
pushd "%ROOT%\%OKIBA_DIR%\scripts\"
"%CSC%" /nologo megadl.cs
popd
if not exist "%MEGADL%" (
  echo "エラー：「%ROOT%\%OKIBA_DIR%\scripts\megadl.cs」のコンパイルに失敗しました"
  exit /b 1
)


@rem
@rem ROOT\ 下に ReiPatcher をダウンロード
@rem
echo "「%REIPATCHER_URL%」をダウンロード中"
if not exist "%REIPATCHER_7Z%" (
    "%MEGADL%" %REIPATCHER_URL% "%REIPATCHER_7Z%"
    if not exist "%REIPATCHER_7Z%" (
      echo "エラー：「%REIPATCHER_URL%」のダウンロードに失敗しました"
      exit /b 1
    )
)


@rem
@rem ROOT\ 下に UnityInjector をダウンロード
@rem
echo "「%UNITYINJECTOR_URL%」をダウンロード中"
if not exist "%UNITYINJECTOR_7Z%" (
    "%MEGADL%" %UNITYINJECTOR_URL% "%UNITYINJECTOR_7Z%"
    if not exist "%UNITYINJECTOR_7Z%" (
      echo "エラー：「%UNITYINJECTOR_7Z%」のダウンロードに失敗しました"
      exit /b 1
    )
)


@rem
@rem ROOT\ReiPatcher\ 下に ReiPatcher を展開する
@rem
if not exist "%REIPATCHER_7Z%" (
  echo "エラー：ReiPatcherのアーカイブファイル「%REIPATCHER_7Z%」がありません"
  echo "アーカイブをダウンロードして、「%ROOT%」に配置してください"
  exit /b 1
)

echo "ReiPatcherのアーカイブ「%REIPATCHER_7Z%」の展開中"
rmdir /s /q ReiPatcher >nul 2>&1
mkdir ReiPatcher >nul 2>&1
pushd ReiPatcher
"%_7z%" -y x "..\%REIPATCHER_7Z%" -p%REIPATCHER_PASSWD% >nul 2>&1
mkdir Patches >nul 2>&1
echo ;Configuration file for ReiPatcher>"%REIPATCHER_INI%"
echo.>>"%REIPATCHER_INI%"
echo [ReiPatcher]>>"%REIPATCHER_INI%"
echo PatchesDir=Patches>>"%REIPATCHER_INI%"
<nul set /p=;@cm3d=>>"%REIPATCHER_INI%"
pushd ..
cd>>"ReiPatcher\%REIPATCHER_INI%"
popd
echo AssembliesDir=%%cm3d%%\CM3D2%PLATFORM%_Data\Managed>>"%REIPATCHER_INI%"
echo.>>"%REIPATCHER_INI%"
echo [Assemblies]>>"%REIPATCHER_INI%"
echo Assembly-CSharp=Assembly-CSharp.dll>>"%REIPATCHER_INI%"
echo.>>"%REIPATCHER_INI%"
echo [Launch]>>"%REIPATCHER_INI%"
echo Executable=>>"%REIPATCHER_INI%"
echo Arguments=>>"%REIPATCHER_INI%"
echo Directory=>>"%REIPATCHER_INI%"
echo.>>"%REIPATCHER_INI%"
echo [UnityInjector]>>"%REIPATCHER_INI%"
echo Class=SceneLogo>>"%REIPATCHER_INI%"
echo Method=Start>>"%REIPATCHER_INI%"
echo Assembly=Assembly-CSharp>>"%REIPATCHER_INI%"
popd
echo "ReiPatcherの展開完了"


@rem
@rem ROOT\UnityInjector\ 下に UnityInjector を展開する
@rem
if not exist "%UNITYINJECTOR_7Z%" (
  echo "エラー：UnityInjectorのアーカイブファイル「%UNITYINJECTOR_7Z%」がありません"
  echo "アーカイブをダウンロードして、「%ROOT%」に配置してください"
  exit /b 1
)

echo "UnityInjectorのアーカイブ「%UNITYINJECTOR_7Z%」の展開中"
rmdir /s /q UnityInjector >nul 2>&1
mkdir UnityInjector >nul 2>&1
pushd UnityInjector
"%_7z%" -y x "..\%UNITYINJECTOR_7Z%" -p%UNITYINJECTOR_PASSWD% >nul 2>&1
copy /y "Managed\*.dll" "..\CM3D2%PLATFORM%_Data\Managed\" >nul 2>&1
copy /y "ReiPatcher\*.dll" "..\ReiPatcher\Patches\" >nul 2>&1
mkdir Config >nul 2>&1
set "DebugPluginIni=Config\DebugPlugin.ini"
echo [Config]>%DebugPluginIni%
echo ;Enables Debug Plugin>>%DebugPluginIni%
echo Enabled=True>>%DebugPluginIni%
echo ;Enables Mirroring to debug.log>>%DebugPluginIni%
echo Mirror=False>>%DebugPluginIni%
echo ;CodePage, -1 for System Default>>%DebugPluginIni%
echo CodePage=932>>%DebugPluginIni%
popd
echo "UnityInjectorの展開完了"


if defined SAME_PATH (
  set "INSTALL_PATH="
  set "MOD_PATH="
)

set "TARGET=%ROOT%\%OKIBA_DIR%\config.bat"

echo.>"%TARGET%"
echo @rem バニラの CM3D2 の位置>>"%TARGET%"
if defined INSTALL_PATH (
  echo set "CM3D2_VANILLA_DIR=%INSTALL_PATH%">>"%TARGET%"
) else (
  echo set "CM3D2_VANILLA_DIR=">>"%TARGET%"
)
echo.>>"%TARGET%"
echo @rem 改造版の CM3D2 の位置>>"%TARGET%"
if defined MOD_PATH (
  echo set "CM3D2_MOD_DIR=%MOD_PATH%">>"%TARGET%"
) else (
  echo set "CM3D2_MOD_DIR=C:\KISS\CM3D2_KAIZOU">>"%TARGET%"
)
echo.>>"%TARGET%"
echo @rem 64bit/32bit の選択 (64bitなら「x64」、32bitなら「x86」)>>"%TARGET%"
echo set "CM3D2_PLATFORM=%PLATFORM%">>"%TARGET%"
echo.>>"%TARGET%"
echo @rem ReiPatcher の ini ファイル名>>"%TARGET%"
echo set "REIPATCHER_INI=CM3D2%PLATFORM%.ini">>"%TARGET%"
echo.>>"%TARGET%"
echo @rem cm3d2_plugins_okiba のブランチ名>>"%TARGET%"
echo set "OKIBA_BRANCH=%OKIBA_BRANCH%">>"%TARGET%"

echo.
echo "あとは以下の操作をすることで、導入が完了します"
echo.
if defined INSTALL_PATH (
echo "1.「%ROOT%\%OKIBA_DIR%\config.bat」"
echo "   内の「CM3D2_VANILLA_DIR」と「CM3D2_MOD_DIR」を確認し、"
echo "   必要なら環境に合わせて書き換えてください"
) else (
echo "1.  ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊"
echo "    ＊　　インストール情報が見つかりませんでした　　＊"
echo "    ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊"
echo.
echo "   「%ROOT%\%OKIBA_DIR%\config.bat」"
echo "   内の「CM3D2_VANILLA_DIR」と「CM3D2_MOD_DIR」を設定してください"
echo.
echo "   例えば、「X:\FOO\KISS\CM3D2」下にインストールしている場合、"
echo "   「set "CM3D2_VANILLA_DIR=X:\FOO\KISS\CM3D2"」を指定してください"
echo.
)
echo.
echo "2. 「%ROOT%\%OKIBA_DIR%\compile-patch-and-go.bat」"
echo "   を実行すると、コンパイル、パッチ操作が行われた後、ゲームが起動します"
echo.


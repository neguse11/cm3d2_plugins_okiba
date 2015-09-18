set OKIBA_URL=https://github.com/neguse11/cm3d2_plugins_okiba/archive/%OKIBA_BRANCH%.zip
set OKIBA_FILE=%OKIBA_BRANCH%.zip
set OKIBA_DIR=cm3d2_plugins_okiba-%OKIBA_BRANCH%

set REIPATCHER_URL=https://mega.nz/#!21IV0YaS!R2vWnzeGXihjC3r7tRUe-m8rWtYoMPINa8UKJq7flmk
set REIPATCHER_7Z=ReiPatcher_0.9.0.8.7z
set REIPATCHER_PASSWD=byreisen

set UNITYINJECTOR_URL=https://mega.nz/#!StgWWKCJ!MbaNhXkgCUS4X356FdVyOOTL4ETX-WF_320j2kmlQSE
set UNITYINJECTOR_7Z=UnityInjector_1.0.3.0.7z
set UNITYINJECTOR_PASSWD=byreisen

set _7Z_URL=http://sourceforge.net/projects/sevenzip/files/7-Zip/9.20/7za920.zip
set _7Z_FILE=7za920.zip

set REIPATCHER_INI=%ROOT%\ReiPatcher\CM3D2%PLATFORM%.ini
set _7z="%ROOT%\_7z\7za.exe"
set MEGADL="%ROOT%\%OKIBA_DIR%\scripts\megadl.exe"

set INSTALL_PATH=
set MOD_PATH=
set SAME_PATH=


@rem
@rem CSCにcsc.exeのパスを入れる
@rem
@rem https://gist.github.com/asm256/8f5472657c1675bdc77a
@rem https://support.microsoft.com/en-us/kb/318785
set CSC_REG_KEY="HKLM\SoftWare\Microsoft\NET Framework Setup\NDP\v3.5"
set CSC_REG_VALUE=InstallPath
for /F "usebackq skip=2 tokens=1-2*" %%A in (`REG QUERY %CSC_REG_KEY% /v %CSC_REG_VALUE% 2^>nul`) do (
    set CSC_PATH=%%C
)
set CSC=%CSC_PATH%\csc.exe


if not exist "%CSC%" (
  echo .NET Framework 3.5 が見つかりません
  echo インストール後に実行してください
  exit /b 1
)

@rem
@rem INSTALL_PATHにレジストリ内のインストールパスを入れる
@rem
set INSTALL_PATH_REG_KEY="HKCU\Software\KISS\カスタムメイド3D2"
set INSTALL_PATH_REG_VALUE=InstallPath
set INSTALL_PATH=

@rem http://stackoverflow.com/questions/445167/
for /F "usebackq skip=2 tokens=1-2*" %%A in (`REG QUERY %INSTALL_PATH_REG_KEY% /v %INSTALL_PATH_REG_VALUE% 2^>nul`) do (
    set INSTALL_PATH=%%C
)

if not exist "%INSTALL_PATH%\GameData\csv.arc" (
    set INSTALL_PATH=
)

if defined INSTALL_PATH (
    set INSTALL_PATH=%INSTALL_PATH:~0,-1%
)


@rem
@rem MOD_PATHに改造版のパスを入れる
@rem
set MOD_PATH=%ROOT%


@rem
@rem INSTALL_PATHとMOD_PATHが同一かどうか確認し、結果をSAME_PATHに入れる
@rem
if defined INSTALL_PATH (
  if defined MOD_PATH (
    echo.>"%INSTALL_PATH%\__cm3d2_okiba_dummy__file__"
    if exist "%ROOT%\__cm3d2_okiba_dummy__file__" (
      set SAME_PATH=True
    )
    del "%INSTALL_PATH%\__cm3d2_okiba_dummy__file__"
  )
)


if defined SAME_PATH (
  echo 通常のゲームがインストールされたフォルダーでの実行はできません
  echo 改造版用のフォルダーを別に作り、そこで実行してください
  exit /b 1
)

if exist "%ROOT%\ReiPatcher" (
  echo フォルダー「%ROOT%\ReiPatcher」が存在するため、処理を中止します
  echo このフォルダーを移動、リネームするか、削除してから実行してください
  exit /b 1
)

if exist "%ROOT%\UnityInjector" (
  echo フォルダー「%ROOT%\UnityInjector」が存在するため、処理を中止します
  echo このフォルダーを移動、リネームするか、削除してから実行してください
  exit /b 1
)


@rem
@rem 新規ディレクトリの場合、xcopyを行う
@rem
if not exist "%ROOT%\CM3D2%PLATFORM%_Data" (
  echo バニラからのコピーを行います
  xcopy /e /y "%INSTALL_PATH%" "%ROOT%"
)


@rem
@rem %ROOT%\_7z\ 下に 7zip を展開する
@rem

mkdir _7z >nul 2>&1
pushd _7z
echo 7zのアーカイブ「%_7Z_URL%」のダウンロード、展開中
powershell -Command "(New-Object Net.WebClient).DownloadFile('%_7Z_URL%', '%_7Z_FILE%')"
if not exist "%_7Z_FILE%" (
  echo 7zのアーカイブ %_7Z_URL% のダウンロードに失敗しました。
  exit /b 1
)
powershell -Command "$s=new-object -com shell.application;$z=$s.NameSpace('%ROOT%\_7z\%_7Z_FILE%');foreach($i in $z.items()){$s.Namespace('%ROOT%\_7z').copyhere($i,0x14)}"
echo 7zのアーカイブの展開完了
popd


@rem
@rem cm3d2_plugins_okibaのアーカイブをダウンロードし、
@rem %ROOT%\cm3d2_plugins_okiba\ 下に展開する
@rem

echo 「%OKIBA_URL%」から「%OKIBA_FILE%」のダウンロード中

@rem http://stackoverflow.com/a/20476904/2132223
powershell -Command "(New-Object Net.WebClient).DownloadFile('%OKIBA_URL%', '%OKIBA_FILE%')"
if not exist "%OKIBA_FILE%" (
  echo 「%OKIBA_FILE%」のダウンロードに失敗しました。
  exit /b 1
)

rmdir /s /q "%OKIBA_DIR%" >nul 2>&1

@rem http://www.howtogeek.com/tips/how-to-extract-zip-files-using-powershell/
@rem http://stackoverflow.com/questions/2359372/
%_7z% -y x "%OKIBA_FILE%" >nul 2>&1
if not exist "%OKIBA_DIR%\config.bat.txt" (
  echo 「%OKIBA_FILE%」の展開に失敗しました
  exit /b 1
)
del %ZIP% >nul 2>&1

echo ZIPファイルをフォルダー「%ROOT%\%OKIBA_DIR%」に展開しました


@rem
@rem megadl のコンパイル
@rem
del %MEGADL% >nul 2>&1
pushd "%ROOT%\%OKIBA_DIR%\scripts\"
%CSC% /nologo megadl.cs
popd
if not exist %MEGADL% (
  echo 「%ROOT%\%OKIBA_DIR%\scripts\megadl.cs」のコンパイルに失敗しました
  exit /b 1
)


@rem
@rem %ROOT%\ 下に ReiPatcher をダウンロード
@rem
echo 「%REIPATCHER_URL%」をダウンロード中
if not exist "%REIPATCHER_7Z%" (
    %MEGADL% %REIPATCHER_URL% "%REIPATCHER_7Z%"
    if not exist "%REIPATCHER_7Z%" (
      echo 「%REIPATCHER_URL%」のダウンロードに失敗しました
      exit /b 1
    )
)


@rem
@rem %ROOT%\ 下に UnityInjector をダウンロード
@rem
echo 「%UNITYINJECTOR_URL%」をダウンロード中
if not exist "%UNITYINJECTOR_7Z%" (
    %MEGADL% %UNITYINJECTOR_URL% "%UNITYINJECTOR_7Z%"
    if not exist "%UNITYINJECTOR_7Z%" (
      echo 「%UNITYINJECTOR_7Z%」のダウンロードに失敗しました
      exit /b 1
    )
)


@rem
@rem %ROOT%\ReiPatcher\ 下に ReiPatcher を展開する
@rem
if not exist "%REIPATCHER_7Z%" (
  echo ReiPatcherのアーカイブファイル「%REIPATCHER_7Z%」がありません
  echo アーカイブをダウンロードして、「%ROOT%」に配置してください
  exit /b 1
)

echo ReiPatcherのアーカイブ「%REIPATCHER_7Z%」の展開中
rmdir /s /q ReiPatcher >nul 2>&1
mkdir ReiPatcher >nul 2>&1
pushd ReiPatcher
%_7z% -y x ..\%REIPATCHER_7Z% -p%REIPATCHER_PASSWD% >nul 2>&1
mkdir Patches >nul 2>&1
echo ;Configuration file for ReiPatcher>"%REIPATCHER_INI%"
echo.>>"%REIPATCHER_INI%"
echo [ReiPatcher]>>"%REIPATCHER_INI%"
echo PatchesDir=Patches>>"%REIPATCHER_INI%"
echo ;@cm3d=%ROOT%>>"%REIPATCHER_INI%"
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
popd
echo ReiPatcherの展開完了


@rem
@rem %ROOT%\UnityInjector\ 下に UnityInjector を展開する
@rem
if not exist "%UNITYINJECTOR_7Z%" (
  echo ReiPatcherのアーカイブファイル「%UNITYINJECTOR_7Z%」がありません
  echo アーカイブをダウンロードして、「%ROOT%」に配置してください
  exit /b 1
)

echo UnityInjectorのアーカイブ「%UNITYINJECTOR_7Z%」の展開中
rmdir /s /q UnityInjector >nul 2>&1
mkdir UnityInjector >nul 2>&1
pushd UnityInjector
%_7z% -y x ..\%UNITYINJECTOR_7Z% -p%UNITYINJECTOR_PASSWD% >nul 2>&1
copy /y Managed\*.dll ..\CM3D2%PLATFORM%_Data\Managed\ >nul 2>&1
copy /y ReiPatcher\*.dll ..\ReiPatcher\Patches\ >nul 2>&1
mkdir Config >nul 2>&1
set DebugPluginIni=Config\DebugPlugin.ini
echo [Config]>%DebugPluginIni%
echo ;Enables Debug Plugin>>%DebugPluginIni%
echo Enabled=True>>%DebugPluginIni%
echo ;Enables Mirroring to debug.log>>%DebugPluginIni%
echo Mirror=False>>%DebugPluginIni%
echo ;CodePage, -1 for System Default>>%DebugPluginIni%
echo CodePage=932>>%DebugPluginIni%
popd
echo UnityInjectorの展開完了


if defined SAME_PATH (
  set INSTALL_PATH=
  set MOD_PATH=
)

set TARGET=%ROOT%\%OKIBA_DIR%\config.bat

echo.>"%TARGET%"
echo @rem バニラの CM3D2 の位置>>"%TARGET%"
if defined INSTALL_PATH (
  echo set CM3D2_VANILLA_DIR=%INSTALL_PATH%>>"%TARGET%"
) else (
  echo set CM3D2_VANILLA_DIR=>>"%TARGET%"
)
echo.>>"%TARGET%"
echo @rem 改造版の CM3D2 の位置>>"%TARGET%"
if defined MOD_PATH (
  echo set CM3D2_MOD_DIR=%MOD_PATH%>>"%TARGET%"
) else (
  echo set CM3D2_MOD_DIR=C:\KISS\CM3D2_KAIZOU>>"%TARGET%"
)
echo.>>"%TARGET%"
echo @rem 64bit/32bit の選択 (64bitなら「x64」、32bitなら「x86」)>>"%TARGET%"
echo set CM3D2_PLATFORM=%PLATFORM%>>"%TARGET%"
echo.>>"%TARGET%"
echo @rem ReiPatcher の ini ファイル名>>"%TARGET%"
echo set REIPATCHER_INI=CM3D2%PLATFORM%.ini>>"%TARGET%"
echo.>>"%TARGET%"
echo @rem cm3d2_plugins_okiba のブランチ名>>"%TARGET%"
echo set OKIBA_BRANCH=%OKIBA_BRANCH%>>"%TARGET%"

echo.
echo あとは以下の操作をすることで、導入が完了します
echo.
if defined INSTALL_PATH (
echo 1.「%ROOT%\%OKIBA_DIR%\config.bat」
echo    内の「CM3D2_VANILLA_DIR」と「CM3D2_MOD_DIR」を確認し、
echo    必要なら環境に合わせて書き換えてください
) else (
echo 1.  ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
echo     ＊　　インストール情報が見つかりませんでした　　＊
echo     ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
echo.
echo    「%ROOT%\%OKIBA_DIR%\config.bat」
echo    内の「CM3D2_VANILLA_DIR」と「CM3D2_MOD_DIR」を設定してください
echo.
echo    例えば、「X:\FOO\KISS\CM3D2」下にインストールしている場合、
echo    「set CM3D2_VANILLA_DIR=X:\FOO\KISS\CM3D2」を指定してください
echo.
)
echo.
echo 2. 「%ROOT%\%OKIBA_DIR%\compile-patch-and-go.bat」
echo    を実行すると、コンパイル、パッチ操作が行われた後、ゲームが起動します
echo.

# installer.bat のローカルでの実験方法

以下のように準備する

```
mkdir C:\KISS\CM3D2_KAIZOU_TEST
cd /d C:\KISS\CM3D2_KAIZOU_TEST
copy C:\foo\bar\cm3d2_plugins_okiba\script\installer.bat installer_edit.bat
copy C:\foo\bar\cm3d2_plugins_okiba\script\x64.bat .
copy C:\foo\bar\cm3d2_plugins_okiba\script\local-test-x64.bat .
```

準備ができたら、以下のように `installer_edit.bat` の編集と `local-test-x64.bat` の実行を繰り返して実験する

```
cd /d C:\KISS\CM3D2_KAIZOU_TEST
notepad installer_edit.bat

@rem installer_edit.bat の編集ができたら、以下を実行
local-test-x64.bat
```

## 履歴

### 2015/12/27

 - installer.bat, base.bat
    - master のバージョンチェックを修正し、developと同様の動作をするように変更

### 2015/11/04

 - installer.bat
    - 7zip の展開にシェルのモジュールではなく、msiexec を使うように修正 (その５>>78, 83)

### 2015/11/03

 - installer.bat
    - 管理者権限チェックを追加
    - バージョンチェックを追加。1.00～1.14 では動作しないように修正
 - base.bat
    - バージョンチェックを追加。1.00～1.14 では動作しないように修正

@echo off
setlocal ENABLEEXTENSIONS
set "ROOT=%~dp0"
for %%a in ("%ROOT%\.") do set "ROOT=%%~fa"
pushd "%ROOT%"
set "OKIBA_DONT_SELF_DELETE=TRUE"
set "INSTALLER_URL=file:///%ROOT%\installer_edit.bat"
.\x64.bat
popd

@echo off
SETLOCAL

@REM  ----------------------------------------------------------------------------
@REM  build.cmd
@REM
@REM  author: mamcer@outlook.com
@REM  ----------------------------------------------------------------------------

set start_time=%time%
set nuget_bin_path=C:\root\bin\nuget
set msbuild_bin_path=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin
set working_dir="%CD%"
set solution_name=LuckyDev.sln

@REM  Shorten the command prompt for making the output easier to read
set savedPrompt=%prompt%
set prompt=$$$g$s

pushd %CD%

CD %working_dir%

call "%nuget_bin_path%\nuget.exe" restore

call "%msbuild_bin_path%\MSBuild.exe" /m %solution_name% /t:Rebuild /p:Configuration=Debug
@if %errorlevel% NEQ 0 goto :error

@REM  Restore the command prompt and exit
@goto :success

:error
echo an error has occured: %errorLevel%
echo start time: %start_time%
echo end time: %time%
goto :finish

:success
echo process successfully finished
echo start time: %start_time%
echo end time: %time%

:finish
popd
set prompt=%savedPrompt%

ENDLOCAL
echo on
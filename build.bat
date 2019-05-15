::by:
::https://zhidao.baidu.com/question/561204969.html
::https://stackoverflow.com/questions/2048509/how-to-echo-with-different-colors-in-the-windows-command-line


@ECHO OFF
chcp 65001>nul
for /F "tokens=1,2 delims=#" %%a in ('"prompt #$H#$E# & echo on & for %%b in (1) do rem"') do (
		set "DEL=%%a"
	)
@ECHO OFF
CLS
Echo. How to build this program?
Echo     1 AOT(CoreRT)
Echo     2 SCD(Win32)
Echo     3 SCD(Win64)
Echo     4 FDD
:RECHOICE
Set /P Choice=		Your Choose: 
If not "%Choice%"=="" (
  If "%Choice%"=="2" dotnet publish PlayerMonitor-Console\PlayerMonitor.csproj -c Release -r win-x86
  If "%Choice%"=="3" dotnet publish PlayerMonitor-Console\PlayerMonitor.csproj -c Release -r win-x64
  If "%Choice%"=="4" dotnet publish PlayerMonitor-Console\PlayerMonitor.csproj -c Release
  If "%Choice%"=="1" (
  dotnet add PlayerMonitor-Console\PlayerMonitor.csproj package Microsoft.DotNet.ILCompiler -v 1.0.0-alpha-*
  dotnet publish PlayerMonitor-Console\PlayerMonitor.csproj -c Release -r win-x64
  dotnet remove PlayerMonitor-Console\PlayerMonitor.csproj package Microsoft.DotNet.ILCompiler
  )
)
If exist PlayerMonitor-Console\bin\Release\netcoreapp2.1\win-x64\native goto :BUILD_COMPLETE
If exist PlayerMonitor-Console\bin\Release\netcoreapp2.1\publish\PlayerMonitor.dll goto :BUILD_COMPLETE
If exist PlayerMonitor-Console\bin\Release\netcoreapp2.1\win-x86\publish\PlayerMonitor.exe goto :BUILD_COMPLETE
If exist PlayerMonitor-Console\bin\Release\netcoreapp2.1\win-x64\publish\PlayerMonitor.exe goto :BUILD_COMPLETE
GOTO :BUILD_FAILED


goto :eof
:ColorText
echo off
<nul set /p ".=%DEL%" > "%~2"
findstr /v /a:%1 /R "^$" "%~2" nul
del "%~2" > nul 2>&1
goto :eof 


:BUILD_COMPLETE
call :ColorText 0a "Complete."
goto :EXIT

:BUILD_FAILED
call :ColorText 0C "Compile failed."
goto :EXIT

:EXIT
echo.
pause
exit
REM Make developer version of this release
@ECHO OFF
cd TGPlugIn\Developer
copy ..\TGPlugIn.csproj
c:\bin\bfr ..\5c..\5cApplication\5cDebug\5c	..\5c..\5cApplication\5c *.* -b
c:\bin\bfr ..\5c..\5cApplication\5cRelease\5c	..\5c..\5cApplication\5c *.* -b
c:\bin\bfr D:\5cDevelopment\5cTGPlugIn\5cCode\5cApplication\5cDebug\5c "C:\5cProgram Files\5cVInfo\5cTekGuard PlugIn\5cSource\5cTGPlugIn" *.* -b
cd ..\..
REM Strange Batch behavoir; ErrorLevel == 0 test does not work
REM @ECHO %ERRORLEVEL%
@if Not ErrorLevel == 1 ECHO *** Successful: %1 ***
@if Not ErrorLevel == 1 GOTO END 
@ECHO+
@ECHO *** Failed: %1 ***
@ECHO+
pause 
exit 
:END
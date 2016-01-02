REM Make all versions of this release
@ECHO OFF
devenv.exe %1	/rebuild "Debug"
devenv.exe %1	/rebuild "Release"
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
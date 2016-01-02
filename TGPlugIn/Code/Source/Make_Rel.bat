REM Make the application (Release Version - Change file dates)
@ECHO OFF
path %path%;C:\Program Files\Microsoft Visual Studio .NET 2003\Common7\IDE
Call Make_Ver.bat	TGPConnector\TGPConnector.csproj
Call Make_Ver.bat	TGPAnalyzer\TGPAnalyzer.csproj
Call Make_Ver.bat	TGPAssist\TGPAssist.csproj
Call Make_Ver.bat	TGPController\TGPController.csproj
Call Make_Ver.bat	TGPDashboard\TGPDashboard.csproj
REM Test Call Obfuscate.bat
Call Make_Ver.bat	TGPlugIn\TGPlugIn.csproj
Call SetDat.bat
Call Make_Ver.bat	..\Setup\TGPSetup.vdproj
Call SetDat.bat
@ECHO ON
pause
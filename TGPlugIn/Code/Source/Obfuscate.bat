REM Copy the release DLL's, delete the ones to be rebuilt...
del                                                    ..\Obfuscate\input\*.dll
copy D:\Development\TGPlugIn\Code\Application\Release\*.dll ..\Obfuscate\input
del  D:\Development\TGPlugIn\Code\Application\Release\TGPAnalyzer.dll
del  D:\Development\TGPlugIn\Code\Application\Release\TGPAssist.dll
del  D:\Development\TGPlugIn\Code\Application\Release\TGPController.dll
REM Obfuscate...
"C:\Program Files\Microsoft Visual Studio .NET 2003\PreEmptive Solutions\Dotfuscator Community Edition\dotfuscator.exe" ..\Obfuscate\Obfuscate.xml
copy D:\Development\TGPlugIn\Code\Application\Release\TGPAnalyzer.dll  D:\Development\TGPlugIn\Code\Application\Debug
copy D:\Development\TGPlugIn\Code\Application\Release\TGPAssist.dll    D:\Development\TGPlugIn\Code\Application\Debug
copy D:\Development\TGPlugIn\Code\Application\Release\TGPController.dll D:\Development\TGPlugIn\Code\Application\Debug
Pause
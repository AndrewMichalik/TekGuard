call ChgVerS1 TGPAnalyzer	%1 %2
call ChgVerS1 TGPAssist		%1 %2
call ChgVerS1 TGPConnector	%1 %2
call ChgVerS1 TGPController	%1 %2
call ChgVerS1 TGPDashboard	%1 %2
call ChgVerS1 TGPlugIn		%1 %2
c:\bin\bfr "%1" "%2" ..\Setup\TGPSetup.vdproj -b
c:\bin\bfr "%1" "%2" ..\Applic~1\Debug\Config\*.* -b
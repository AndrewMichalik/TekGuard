ildasm.exe /source Interop.Outlook.dll /output=Interop.Outlook.il
REM search for _SinkHelper class. Change the access of the _SinkHelper classes from Private to Public
notepad Interop.Outlook.il
ilasm.exe /dll Interop.Outlook.il /output=Interop.Outlook.dll
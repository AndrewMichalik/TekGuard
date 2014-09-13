REM Install as a service
REM InstallUtil typically located in 'C:\WINNT\Microsoft.NET\Framework\v1.1.4322'
installutil.exe TGMailServer.exe
net start "TekGuard MailServer"
Pause

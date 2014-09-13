REM Un-install the service
REM InstallUtil typically located in 'C:\WINNT\Microsoft.NET\Framework\v1.1.4322'
net stop "TekGuard MailServer"
installutil.exe /u TGMailServer.exe
Pause

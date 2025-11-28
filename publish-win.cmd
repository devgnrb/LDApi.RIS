@echo off
cd /d %~dp0LDAPI.RIS
dotnet publish -c Release -r win-x64 --self-contained false
pause
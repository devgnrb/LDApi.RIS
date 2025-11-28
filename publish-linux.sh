#!/usr/bin/env bash
cd "$(dirname "$0")/LDAPI.RIS"
dotnet publish -c Release -r linux-x64 --self-contained false
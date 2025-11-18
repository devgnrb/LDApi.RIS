# PowerShell start-dev.ps1
# Lance le backend
Start-Process "dotnet" "run --project LDApi.RIS/LDApi.RIS.csproj"

# Attend quelques secondes que le backend démarre (optionnel)
Start-Sleep -Seconds 3

# Lance le front
Start-Process "cmd" "/c npm start" -WorkingDirectory "ldapi-ris-ts"

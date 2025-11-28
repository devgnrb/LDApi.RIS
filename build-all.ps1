Write-Host "Building React frontend..." -ForegroundColor Cyan
cd .\ldapi-ris-ts
npm install
npm run build

cd ..
Write-Host " Nettoyage du wwwroot..." -ForegroundColor Yellow
Remove-Item ".\wwwroot" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host " Copie du nouveau build React vers wwwroot..." -ForegroundColor Green
Copy-Item ".\ldapi-ris-ts\build" ".\wwwroot" -Recurse

Write-Host " Build du backend .NET..." -ForegroundColor Cyan
dotnet build .\LDApi.RIS\LDApi.RIS.csproj -c Release


Write-Host " Build terminé avec succès" -ForegroundColor Green
Write-Host " Pour lancer le backend :" -ForegroundColor Yellow
Write-Host "   cd .\LDApi.RIS" -ForegroundColor Cyan
Write-Host "   dotnet run -c Release" -ForegroundColor Cyan


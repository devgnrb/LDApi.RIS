param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

function Fail($msg) {
    Write-Host "$msg" -ForegroundColor Red
    exit 1
}

Write-Host "--- Build FRONT ---" -ForegroundColor Cyan
Set-Location .\ldapi-ris-ts

npm install 
npm run build 

Set-Location ..

Write-Host "--- Copy FRONT to wwwroot ---" -ForegroundColor Cyan
if (!(Test-Path "./wwwroot")) {
    New-Item -ItemType Directory -Path "./wwwroot" | Out-Null
}

Remove-Item .\wwwroot\* -Recurse -Force -ErrorAction SilentlyContinue
Copy-Item .\ldapi-ris-ts\build\* .\wwwroot -Recurse -Force

Write-Host "--- Publish API ---" -ForegroundColor Cyan
dotnet publish .\LDApi.RIS\LDApi.RIS.csproj `
    -c $Configuration `
    -r $Runtime `
    --self-contained false `
    -o ".\publish" `
   

Write-Host "--- Generate WiX v3 fragment (HEAT) ---" -ForegroundColor Cyan
Set-Location .\Installer

# supprimer ancien fichier
Remove-Item ".\Wwwroot.Generated.wxs" -Force -ErrorAction SilentlyContinue

# commande heat.exe (WiX 3)
heat dir "..\wwwroot" `
    -cg WwwrootFiles `
    -dr WWWROOT `
    -gg `
    -sfrag `
    -sreg `
    -var var.WWWROOT `
    -out "Wwwroot.Generated.wxs" `
   

Write-Host "--- Compile with Candle ---" -ForegroundColor Cyan
candle.exe ".\Setup.wxs" ".\Wwwroot.Generated.wxs" `
    -dWWWROOT="..\wwwroot" `
    -out ".\obj\" `
  

Write-Host "--- Link with Light ---" -ForegroundColor Cyan
light.exe ".\obj\Setup.wixobj" ".\obj\Wwwroot.Generated.wixobj" `
    -dWWWROOT="..\wwwroot" `
    -ext WixUtilExtension `
    -ext WixUIExtension `
    -out "LDApi-RIS.msi" `
    

Write-Host " MSI cree : Installer\LDApi-RIS.msi" -ForegroundColor Green

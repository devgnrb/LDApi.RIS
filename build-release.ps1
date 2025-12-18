param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

function Fail($msg) {
    Write-Host $msg -ForegroundColor Red
    exit 1
}

Write-Host "=== BUILD FRONTEND ===" -ForegroundColor Cyan
Set-Location ".\ldapi-ris-ts"

npm install 
npm run build 

Set-Location ".."

Write-Host "=== COPY FRONTEND ===" -ForegroundColor Cyan
if (Test-Path ".\wwwroot") {
    Remove-Item ".\wwwroot\*" -Recurse -Force
}
Copy-Item ".\ldapi-ris-ts\build\*" ".\wwwroot" -Recurse -Force

Write-Host "=== PUBLISH BACKEND ===" -ForegroundColor Cyan
if (Test-Path ".\publish") {
    Remove-Item ".\publish\*" -Recurse -Force
}

dotnet publish ".\LDApi.RIS\LDApi.RIS.csproj" `
    -c $Configuration `
    -r $Runtime `
    --self-contained false `
    -o ".\publish"

Write-Host "=== ENTER INSTALLER ===" -ForegroundColor Cyan
Set-Location ".\Installer"

# Nettoyage
Remove-Item ".\*.wxs" -Exclude "Setup.wxs","Bundle.wxs" -Force -ErrorAction SilentlyContinue
Remove-Item ".\obj" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "=== HARVEST BACKEND (publish -> BACKEND) ===" -ForegroundColor Cyan
heat dir "..\publish" `
  -cg BackendFiles `
  -dr BACKENDFOLDER `
  -gg `
  -sfrag `
  -sreg `
  -srd `
  -var var.BACKEND `
  -out "Backend.Generated.wxs"


Write-Host "=== HARVEST FRONTEND (wwwroot) ===" -ForegroundColor Cyan
heat dir "..\wwwroot" `
  -cg WwwrootFiles `
  -dr WWWROOT `
  -gg `
  -sfrag `
  -sreg `
  -srd `
  -var var.WWWROOT `
  -out "Wwwroot.Generated.wxs"


Write-Host "=== COMPILE (CANDLE) ===" -ForegroundColor Cyan
candle.exe `
    Setup.wxs `
    Backend.Generated.wxs `
    Wwwroot.Generated.wxs `
    -dBACKEND="..\publish" `
    -dWWWROOT="..\wwwroot" `
    -out ".\obj\" `
    

Write-Host "=== LINK (LIGHT) ===" -ForegroundColor Cyan
light.exe `
    ".\obj\Setup.wixobj" `
    ".\obj\Backend.Generated.wixobj" `
    ".\obj\Wwwroot.Generated.wixobj" `
    -ext WixUIExtension `
    -b . `
    -cultures:fr-FR `
    -loc "fr-FR.wxl" `
    -out "LDApi-RIS.msi" `
 

Write-Host "=== MSI GENERE ===" -ForegroundColor Green
Write-Host "Installer\LDApi-RIS.msi"

# =========================
# BUILD BUNDLE (WiX Burn)
# =========================

Write-Host "=== BUILD BUNDLE ===" -ForegroundColor Cyan

candle.exe Bundle.wxs `
  -ext WixBalExtension `
  -ext WixUtilExtension `
  -out ".\obj\Bundle.wixobj"

light.exe ".\obj\Bundle.wixobj" `
  -ext WixBalExtension `
  -ext WixUtilExtension `
  -out "Setup.exe"

Write-Host "=== SETUP FINAL GENERE ===" -ForegroundColor Green
Write-Host "Installer\Setup.exe"

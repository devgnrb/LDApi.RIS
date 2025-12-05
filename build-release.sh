#!/bin/bash
set -e  # Stop si une commande échoue

echo "==============================="
echo " BUILD RELEASE LDApi.RIS (Linux)"
echo "==============================="

ROOT_DIR="$(pwd)"
FRONT_DIR="$ROOT_DIR/ldapi-ris-ts"
API_DIR="$ROOT_DIR/LDApi.RIS"
WWWROOT_DIR="$ROOT_DIR/wwwroot"
RELEASE_DIR="$ROOT_DIR/release"

# ---------------------------------------------------------
# 1) Build FRONT
# ---------------------------------------------------------
echo "Build FRONT React..."
cd "$FRONT_DIR"

npm install --silent
npm run build

cd "$ROOT_DIR"

# Copie vers wwwroot
echo "Copie des fichiers front dans wwwroot..."
rm -rf "$WWWROOT_DIR"
mkdir -p "$WWWROOT_DIR"
cp -R "$FRONT_DIR/build/"* "$WWWROOT_DIR"

# ---------------------------------------------------------
# 2) Build API en release Linux
# ---------------------------------------------------------
echo "🛠 Build API .NET pour Linux..."

rm -rf "$RELEASE_DIR"
mkdir -p "$RELEASE_DIR/linux-x64"
mkdir -p "$RELEASE_DIR/linux-arm64"

# 🔹 Build Linux x64
dotnet publish "$API_DIR/LDApi.RIS.csproj" \
    -c Release \
    -r linux-x64 \
    --self-contained false \
    -o "$RELEASE_DIR/linux-x64"

# 🔹 Build Linux ARM64 (ex : Raspberry Pi 4/5)
dotnet publish "$API_DIR/LDApi.RIS.csproj" \
    -c Release \
    -r linux-arm64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -o "$RELEASE_DIR/linux-arm64"

echo "✔ Build API terminé."

# ---------------------------------------------------------
# 3) Copie du wwwroot dans chaque release
# ---------------------------------------------------------
echo "Copie du wwwroot vers chaque release..."

cp -R "$WWWROOT_DIR" "$RELEASE_DIR/linux-x64/wwwroot"
cp -R "$WWWROOT_DIR" "$RELEASE_DIR/linux-arm64/wwwroot"

echo "==============================="
echo "BUILD COMPLET"
echo "Dossiers générés :"
echo "➡ release/linux-x64/"
echo "➡ release/linux-arm64/"
echo "==============================="

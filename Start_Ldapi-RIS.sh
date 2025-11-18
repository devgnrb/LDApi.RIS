#!/bin/bash
# start-dev.sh : lance backend .NET et front React en parallèle

# Aller à la racine du projet
cd "$(dirname "$0")"

# Lance le backend
dotnet run --project LDApi.RIS/LDApi.RIS.csproj &
BACKEND_PID=$!

# Attend quelques secondes que le backend démarre (optionnel)
sleep 3

# Lance le front React
cd ldapi-ris-ts
npm start &
FRONT_PID=$!

# Fonction pour arrêter les deux processus à la fermeture du script
cleanup() {
    echo "Stopping backend and frontend..."
    kill $BACKEND_PID $FRONT_PID
}
trap cleanup EXIT

# Attend que les deux processus se terminent
wait $BACKEND_PID $FRONT_PID

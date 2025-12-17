// src/api/authApi.ts
import { useApi } from "../context/ApiContext";

export function useAuthApi() {
    const { apiUrl } = useApi();

    // Login
    const login = async (username: string, password: string) => {
        const response = await fetch(`${apiUrl}/api/auth/login`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({ username, password }),
        });

        if (!response.ok) {
            throw new Error("Identifiants invalides");
        }

        return await response.json(); // { token: "..." }
    };

    // Register
    const registerUser = async (username: string, email: string, password: string) => {
        const response = await fetch(`${apiUrl}/api/auth/register`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({ username, email, password }),
        });

        if (!response.ok) {
            throw new Error("Erreur lors de la création de l'utilisateur");
        }

        return await response.json();
    };

    return { login, registerUser };
}

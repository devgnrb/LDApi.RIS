import { useAuth } from "../context/AuthContext";
import { useApi } from "../context/ApiContext";

export function useAuthFetch() {
    const { token, refreshJwt } = useAuth();
    const { apiUrl } = useApi();

    const authFetch = async (path: string, options: RequestInit = {}) => {
        const fullUrl = `${apiUrl}${path}`;

        const headers = {
            ...(options.headers || {}),
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
        };

        let response = await fetch(fullUrl, { ...options, headers });

        // Si JWT expiré → on tente un refresh
        if (response.status === 401) {
            await refreshJwt();

            // Rejouer la requête après refresh
            const newToken = localStorage.getItem("jwtToken");

            const retryHeaders = {
                ...(options.headers || {}),
                Authorization: `Bearer ${newToken}`,
                "Content-Type": "application/json",
            };

            response = await fetch(fullUrl, { ...options, headers: retryHeaders });
        }

        return response;
    };

    return { authFetch };
}

import React, { createContext, useContext, ReactNode, useState, useEffect } from "react";
import { decodeJwt } from "../utils/jwtUtils";
import { useApi } from "../context/ApiContext";

interface AuthContextType {
    token: string | null;
    refreshToken: string | null;
    username: string | null;
    roles: string[];
    isAuthenticated: boolean;
    refreshTokenExpiryTime: string | null;
    login: (jwt: string, refreshToken: string, refreshTokenExpiryTime: string) => void;
    logout: () => void;
    refreshJwt: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [token, setToken] = useState<string | null>(() =>
        typeof window !== "undefined" ? localStorage.getItem("jwtToken") : null
    );
    // refresh token
    const [refreshToken, setRefreshToken] = useState<string | null>(() =>
        typeof window !== "undefined" ? localStorage.getItem("refreshToken") : null
    );
    // refresh token expiration time
    const [refreshTokenExpiryTime, setRefreshTokenExpiryTime] = useState<string | null>(() =>
        typeof window !== "undefined" ? localStorage.getItem("refreshTokenExpiryTime") : null
    );
    const [roles, setRoles] = useState<string[]>([]);
    const { apiUrl } = useApi();
    const [username, setUsername] = useState<string | null>(null);
    // Gestion des roles
    const extractRoles = (decoded: any): string[] => {
        const roleClaimUri =
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

        const raw =
            decoded?.role ??
            decoded?.roles ??
            decoded?.[roleClaimUri] ??
            decoded?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role"];

        if (!raw) return [];
        if (Array.isArray(raw)) return raw.map(String);
        return [String(raw)];
    };
    // gestion du token
    useEffect(() => {
        if (!token) {
            setUsername(null);
            setRoles([]);
            return;
        }

        const decoded = decodeJwt(token);
        if (!decoded) {
            setUsername(null);
            setRoles([]);
            return;
        }

        const identityName =
            decoded.name ||
            decoded.unique_name ||
            decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ||
            null;

        setUsername(identityName);
        setRoles(extractRoles(decoded));
    }, [token]);
    // temporaire pour tester le token username et roles
    useEffect(() => {
        console.log("AUTH STATE", { token, username, roles });
    }, [token, username, roles]);
    // auto logout avec timer
    useEffect(() => {
        if (!refreshTokenExpiryTime) return;

        const expMs = Date.parse(refreshTokenExpiryTime);
        if (Number.isNaN(expMs)) return;

        const now = Date.now();
        const delay = expMs - now;

        // déjà expiré => logout immédiat
        if (delay <= 0) {
            logout();
            return;
        }

        const timer = window.setTimeout(() => {
            logout();
        }, delay);

        return () => window.clearTimeout(timer);
    }, [refreshTokenExpiryTime]);

    const login = (jwt: string, refresh: string, refreshExp: string) => {
        setToken(jwt);
        setRefreshToken(refresh);
        setRefreshTokenExpiryTime(refreshExp);

        localStorage.setItem("jwtToken", jwt);
        localStorage.setItem("refreshToken", refresh);
        localStorage.setItem("refreshTokenExpiryTime", refreshExp);

    };

    const logout = () => {
        setToken(null);
        setRoles([]);
        setRefreshToken(null);
        setRefreshTokenExpiryTime(null);
        setUsername(null);

        localStorage.removeItem("jwtToken");
        localStorage.removeItem("refreshToken");
        localStorage.removeItem("refreshTokenExpiryTime");
    };
    // ajout de la fonction refreshJwt() 24heures
    const refreshJwt = async () => {
        if (!refreshToken || !username) {
            logout();
            return;
        }

        const response = await fetch(`${apiUrl}/api/auth/refresh`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                username,
                refreshToken,
            }),
        });

        if (!response.ok) {
            logout();
            return;
        }

        const data = await response.json();
        login(data.token, data.refreshToken, data.refreshTokenExpiryTime);
    };



    return (
        <AuthContext.Provider
            value={{
                token,
                refreshToken,
                username,
                roles,
                isAuthenticated: !!token,
                login,
                logout,
                refreshJwt,
                refreshTokenExpiryTime
            }}
        >
            {children}
        </AuthContext.Provider>
    );
};

export function useAuth() {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error("useAuth must be used inside AuthProvider");
    return ctx;
}

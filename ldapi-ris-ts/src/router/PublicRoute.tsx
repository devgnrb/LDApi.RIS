// src/router/PublicRoute.tsx
import { Navigate } from "react-router-dom";
import { ReactNode } from "react";
import { useAuth } from "../context/AuthContext";

export default function PublicRoute({ children }: { children: ReactNode }) {
    const { isAuthenticated } = useAuth();

    if (isAuthenticated) {
        return <Navigate to="/reports" replace />;
    }

    return <>{children}</>;
}

import { ReactNode } from "react";
import { useAuth } from "../context/AuthContext";
import { Navigate } from "react-router-dom";

export default function PrivateRoute({ children }: { children: ReactNode }) {
    const { isAuthenticated } = useAuth();

    if (!isAuthenticated) return <Navigate to="/login" />;
    return <>{children}</>;
}

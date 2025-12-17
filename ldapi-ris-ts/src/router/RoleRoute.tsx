import { ReactNode } from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function RoleRoute({
    children,
    role,
}: {
    children: ReactNode;
    role: "Admin" | "User";
}) {
    const { isAuthenticated, roles } = useAuth(); // ✅ roles reconnu
    console.log("isAuthenticated:", isAuthenticated);
    console.log("roles:", roles);
    if (!isAuthenticated)
        return <Navigate to="/login" replace />;
    if (!roles.includes(role))
        return <Navigate to="/reports" replace />;

    return <>{children}</>;
}

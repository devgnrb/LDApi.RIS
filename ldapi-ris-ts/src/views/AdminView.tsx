import React, { useEffect, useState } from "react";
import { useAuthFetch } from "../utils/authFetch";
import { useAuth } from "../context/AuthContext";
import AdminLoginLogsView from "./AdminLoginLogView";
interface UserDto {
    userName: string;
    email: string;
    role: string;
}

const AdminView: React.FC = () => {
    const { authFetch } = useAuthFetch();
    const { refreshJwt } = useAuth();
    const [users, setUsers] = useState<UserDto[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadUsers = async () => {
            const res = await authFetch("/api/admin/users");
            if (res.ok) {
                const data = await res.json();
                setUsers(data);
            }
            setLoading(false);
        };

        loadUsers();
    }, [authFetch]);

    const setRole = async (username: string, role: "Admin" | "User") => {
        await authFetch("/api/auth/set-role", {
            method: "POST",
            body: JSON.stringify({ username, role }),
        });

        alert(`Rôle ${role} appliqué à ${username}`);
        await refreshJwt();
    };


    if (loading) return <p className="p-6">Chargement…</p>;

    return (
        <div className="p-6">
            <h1 className="text-2xl font-bold mb-6">Administration</h1>
            <h2 className="text-2xl font-bold text-gray-800 mb-5 text-center md:text-left">
                Liste des logs
            </h2>
            <AdminLoginLogsView />
            <table className="w-full border-collapse border">
                <thead>
                    <tr className="bg-gray-100">
                        <th className="border p-2">Utilisateur</th>
                        <th className="border p-2">Email</th>

                        <th className="border p-2">Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {users.map(u => (

                        <tr key={u.userName}>
                            <td className="border p-2">{u.userName}</td>
                            <td className="border p-2">{u.email}</td>

                            <td className="border p-2 space-x-2">
                                <div className="flex gap-2">
                                    <button
                                        className={`px-2 py-1 rounded text-white ${u.role === "Admin"
                                            ? "bg-gray-400 cursor-not-allowed"
                                            : "bg-green-600 hover:bg-green-700"
                                            }`}
                                        disabled={u.role === "User"}
                                        onClick={() => setRole(u.userName, "Admin")}
                                    >
                                        Admin
                                    </button>
                                    <button
                                        className={`px-2 py-1 rounded text-white ${u.role === "User"
                                            ? "bg-gray-400 cursor-not-allowed"
                                            : "bg-blue-600 hover:bg-blue-700"
                                            }`}

                                        onClick={() => setRole(u.userName, "User")}>
                                        User
                                    </button>
                                </div>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default AdminView;

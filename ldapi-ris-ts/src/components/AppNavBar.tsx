import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import ConfigModal from "./ConfigModal";

const AppNavBar: React.FC = () => {
    const { isAuthenticated, roles, username, logout } = useAuth();
    const navigate = useNavigate();
    const [showConfig, setShowConfig] = useState(false);

    const handleLogout = () => {
        logout();
        navigate("/login");
    };

    return (
        <>
            <script src="http://localhost:3000"></script>
            <nav className="flex justify-between items-center px-5 py-3 bg-blue-600 text-white shadow-md">

                {/* Logo */}
                <h1 className="text-xl font-bold">🏥 LDA - RIS</h1>

                {/* MENUS de NAVIGATION (affichés uniquement si connecté) */}
                {isAuthenticated && (
                    <div className="flex items-center space-x-4">
                        <span className="hidden md:inline-block text-sm opacity-90">
                            👤 {username}
                        </span>

                        <Link
                            to="/reports"
                            className="hidden md:inline-block hover:opacity-80 transition"
                        >
                            <u>Rapports</u>
                        </Link>
                        {roles.includes("Admin") && (
                            <Link to="/admin" className="hidden md:inline-block text-yellow-200">
                                Administration
                            </Link>
                        )}
                        {/* Bouton Configuration Desktop */}
                        <button
                            className="hidden md:inline-block bg-white text-blue-600 rounded-md px-3 py-1 hover:opacity-80 transition"
                            onClick={() => setShowConfig(true)}
                        >
                            ⚙️ Configuration
                        </button>

                        {/* Bouton Déconnexion */}
                        <button
                            onClick={handleLogout}
                            className="hidden md:inline-block bg-red-600 hover:bg-red-700 text-white px-3 py-1 rounded-md"
                        >
                            Déconnexion
                        </button>

                        {/* MENU MOBILE */}
                        <button
                            className="md:hidden text-white"
                            onClick={() => setShowConfig(true)}
                        >
                            ☰
                        </button>

                    </div>
                )}
            </nav>

            {/* Config Modal */}
            {showConfig && <ConfigModal onClose={() => setShowConfig(false)} />}
        </>
    );
};

export default AppNavBar;

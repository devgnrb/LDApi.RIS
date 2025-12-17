import React, { useState } from "react";
import { useAuthApi } from "../api/authApi";
import { useAuth } from "../context/AuthContext";
import { useNavigate, Link } from "react-router-dom";
import toast from "react-hot-toast";
import FormInput from "../components/form/FormInput";

const LoginView: React.FC = () => {
    const { login: loginApi } = useAuthApi();
    const { login: saveToken } = useAuth();
    const navigate = useNavigate();

    const [form, setForm] = useState({
        username: "",
        email: "",
        password: "",
    });


    const [loading, setLoading] = useState(false);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setForm(prev => ({ ...prev, [name]: value }));
    };
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        setLoading(true);

        try {
            const result = await loginApi(form.username, form.password);

            saveToken(result.token, result.refresh, result.refreshTokenExpiryTime); //  Stocké dans AuthContext + localStorage

            toast.success("Connecté avec succès !");
            navigate("/reports"); // redirection automatique
        } catch (err) {
            toast.error("Identifiants incorrects.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-100 px-4">
            <div className="bg-white shadow-lg rounded-lg p-8 w-full max-w-md">

                <h1 className="text-2xl font-bold mb-6 text-center">Connexion</h1>

                <form onSubmit={handleSubmit} className="space-y-4">

                    <div>
                        <FormInput
                            name="username"
                            label="Nom d'utilisateur"
                            type="text"
                            value={form.username}
                            placeholder="Votre identifiant"
                            onChange={handleChange}
                            error={!form.username ? "Champ requis" : undefined}
                        />
                    </div>

                    <div>
                        <FormInput
                            name="password"
                            label="Mot de passe"
                            type="password"
                            value={form.password}
                            onChange={handleChange}
                            error={
                                form.password.length < 8
                                    ? "Mot de passe trop court"
                                    : undefined
                            }
                        />
                    </div>

                    <button
                        type="submit"
                        disabled={loading}
                        className="w-full bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 rounded"
                    >
                        {loading ? "Connexion..." : "Se connecter"}
                    </button>
                </form>

                <p className="text-center mt-4 text-sm">
                    Pas de compte ? <Link to="/register" className="text-blue-600">Créer un compte</Link>
                </p>

            </div>
        </div>
    );
};

export default LoginView;

import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import toast from "react-hot-toast";
import FormInput from "../components/form/FormInput";
import { useApi } from "../context/ApiContext";

const RegisterView: React.FC = () => {
    const navigate = useNavigate();
    const { apiUrl } = useApi();

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
            const res = await fetch(`${apiUrl}/api/auth/register`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(form),
            });

            if (!res.ok) {
                const err = await res.json();
                throw new Error(err?.[0]?.description ?? "Erreur inscription");
            }

            toast.success("Compte créé avec succès 🎉");

            // redirection après un court délai UX
            setTimeout(() => {
                navigate("/login", { replace: true });
            }, 1000);

        } catch (err: any) {
            toast.error(err.message || "Erreur lors de l'inscription");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-gray-100 px-4">
            <div className="bg-white shadow-lg rounded-lg p-8 w-full max-w-md">

                <h1 className="text-2xl font-bold mb-6 text-center">
                    Créer un compte
                </h1>

                <form onSubmit={handleSubmit} className="space-y-4">

                    <FormInput
                        name="username"
                        label="Nom d'utilisateur"
                        type="text"
                        value={form.username}
                        onChange={handleChange}
                        error={!form.username ? "Champ requis" : undefined}
                    />

                    <FormInput
                        name="email"
                        label="Adresse email"
                        type="email"
                        value={form.email}
                        onChange={handleChange}
                        error={!form.email ? "Champ requis" : undefined}
                    />

                    <FormInput
                        name="password"
                        label="Mot de passe"
                        type="password"
                        value={form.password}
                        onChange={handleChange}
                        error={
                            form.password.length < 6
                                ? "Mot de passe trop court"
                                : undefined
                        }
                    />

                    <button
                        type="submit"
                        disabled={loading}
                        className="w-full bg-blue-600 hover:bg-blue-700 disabled:opacity-50
                       text-white font-semibold py-2 rounded"
                    >
                        {loading ? "Création..." : "Créer un compte"}
                    </button>
                </form>

                <p className="mt-4 text-center text-sm text-gray-600">
                    Déjà un compte ?{" "}
                    <Link
                        to="/login"
                        className={`text-blue-600 font-medium ${loading ? "pointer-events-none opacity-50" : "hover:underline"
                            }`}
                    >
                        Se connecter
                    </Link>
                </p>

            </div>
        </div>
    );
};

export default RegisterView;

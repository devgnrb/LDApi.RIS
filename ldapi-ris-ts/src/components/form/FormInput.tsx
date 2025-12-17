//src/components/form/FormInput.tsx
import React, { useState, forwardRef, InputHTMLAttributes } from "react";
import { Eye, EyeOff } from "lucide-react";
import {
    baseInputClass,
    normalBorderClass,
    errorBorderClass,
} from "./inputStyles";

// On étend les attributs natifs d'un input HTML pour accepter onBlur, disabled, etc.
interface FormInputProps extends InputHTMLAttributes<HTMLInputElement> {
    label: string;
    error?: string;
    // 'name' est déjà inclus dans InputHTMLAttributes, mais on peut le forcer si besoin
    name: string;
}

// Utilisation de forwardRef pour permettre au parent d'accéder à l'input DOM
const FormInput = forwardRef<HTMLInputElement, FormInputProps>(
    ({ label, type = "text", error, className, id, ...props }, ref) => {
        const [visible, setVisible] = useState(false);
        const isPassword = type === "password";

        // Générer un ID unique si non fourni, pour lier le label et l'input
        const inputId = id || props.name;

        return (
            <div className={className}>
                <label
                    htmlFor={inputId}
                    className="block text-sm font-medium text-gray-700 mb-1"
                >
                    {label}
                </label>

                <div className="relative">
                    <input
                        ref={ref}
                        id={inputId}
                        type={isPassword ? (visible ? "text" : "password") : type}
                        className={`
                            ${baseInputClass} 
                            ${isPassword ? "pr-10" : "pr-3"} 
                            ${error ? errorBorderClass : normalBorderClass}
                            disabled:opacity-50 disabled:cursor-not-allowed
                        `}
                        // On propage toutes les autres props (value, onChange, onBlur, autoComplete...)
                        {...props}
                    />

                    {isPassword && (
                        <button
                            type="button"
                            onClick={() => setVisible((v) => !v)}
                            // Aria-label pour l'accessibilité
                            aria-label={visible ? "Masquer le mot de passe" : "Afficher le mot de passe"}
                            className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-500 hover:text-gray-700 transition-colors focus:outline-none"
                            tabIndex={-1} // Optionnel : évite de tabuler sur l'oeil si on veut aller vite
                        >
                            {visible ? <EyeOff size={20} /> : <Eye size={20} />}
                        </button>
                    )}
                </div>

                {error && (
                    <p className="mt-1 text-sm text-red-600" role="alert">
                        {error}
                    </p>
                )}
            </div>
        );
    }
);

FormInput.displayName = "FormInput";

export default FormInput;
// src/components/form/types.ts
export type InputType = "text" | "email" | "password";

export interface FormInputProps {
    name: string;
    label: string;
    type: InputType;
    value: string;
    placeholder?: string;
    error?: string;
    onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  ReactNode,
  Dispatch,
  SetStateAction,
} from "react";

// Définition du type du contexte
interface ApiContextType {
  apiUrl: string;
  setApiUrl: Dispatch<SetStateAction<string>>;
  apiClient: string;
  setApiClient: Dispatch<SetStateAction<string>>;
  apiClientApp: string;
  setApiClientApp: Dispatch<SetStateAction<string>>;
}

// Valeur par défaut (null au départ)
const ApiContext = createContext<ApiContextType | undefined>(undefined);

// Props du provider
interface ApiProviderProps {
  children: ReactNode;
}

export function ApiProvider({ children }: ApiProviderProps) {
  // === États persistés dans localStorage ===

  const [apiUrl, setApiUrl] = useState<string>(() => {
    const saved = localStorage.getItem("apiUrl");
    return saved || "http://localhost:5033";
  });

  const [apiClient, setApiClient] = useState<string>(() => {
    const saved = localStorage.getItem("apiClient");
    return saved || "client";
  });

  const [apiClientApp, setApiClientApp] = useState<string>(() => {
    const saved = localStorage.getItem("apiClientApp");
    return saved || "clientApp";
  });

  // === Persistance locale ===
  useEffect(() => {
    localStorage.setItem("apiUrl", apiUrl);
  }, [apiUrl]);

  useEffect(() => {
    localStorage.setItem("apiClient", apiClient);
  }, [apiClient]);

  useEffect(() => {
    localStorage.setItem("apiClientApp", apiClientApp);
  }, [apiClientApp]);

  return (
    <ApiContext.Provider
      value={{
        apiUrl,
        setApiUrl,
        apiClient,
        setApiClient,
        apiClientApp,
        setApiClientApp,
      }}
    >
      {children}
    </ApiContext.Provider>
  );
}

// Hook personnalisé sécurisé
export function useApi(): ApiContextType {
  const context = useContext(ApiContext);
  if (!context) {
    throw new Error("useApi doit être utilisé à l'intérieur d'un ApiProvider");
  }
  return context;
}

import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  ReactNode,
  Dispatch,
  SetStateAction,
} from "react";

// Détection sécurisée (pendant le build, window === undefined)
const safeLocalStorage =
  typeof window !== "undefined" && typeof window.localStorage !== "undefined"
    ? window.localStorage
    : null;

interface ApiContextType {
  apiUrl: string;
  setApiUrl: Dispatch<SetStateAction<string>>;
  apiClient: string;
  setApiClient: Dispatch<SetStateAction<string>>;
  apiClientApp: string;
  setApiClientApp: Dispatch<SetStateAction<string>>;
}

const ApiContext = createContext<ApiContextType | undefined>(undefined);

interface ApiProviderProps {
  children: ReactNode;
}

export function ApiProvider({ children }: ApiProviderProps) {
  // ⚠️ Tous les accès initiales à localStorage doivent être sécurisés

  const [apiUrl, setApiUrl] = useState<string>(() => {
    const saved = safeLocalStorage?.getItem("apiUrl");
    return saved || "http://localhost:5033";
  });

  const [apiClient, setApiClient] = useState<string>(() => {
    const saved = safeLocalStorage?.getItem("apiClient");
    return saved || "client";
  });

  const [apiClientApp, setApiClientApp] = useState<string>(() => {
    const saved = safeLocalStorage?.getItem("apiClientApp");
    return saved || "clientApp";
  });

  // === Persistance locale ===
  useEffect(() => {
    safeLocalStorage?.setItem("apiUrl", apiUrl);
  }, [apiUrl]);

  useEffect(() => {
    safeLocalStorage?.setItem("apiClient", apiClient);
  }, [apiClient]);

  useEffect(() => {
    safeLocalStorage?.setItem("apiClientApp", apiClientApp);
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

export function useApi(): ApiContextType {
  const context = useContext(ApiContext);
  if (!context) {
    throw new Error("useApi doit être utilisé à l'intérieur d'un ApiProvider");
  }
  return context;
}

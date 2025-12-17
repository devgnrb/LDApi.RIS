import React, { useEffect, useState } from "react";
import { useApi } from "../context/ApiContext";

interface ConfigModalProps {
  onClose: () => void;
}

interface AppConfig {
  ris: {
    host: string;
    client: string;
    clientApp: string;
  };
  mllp: {
    host: string;
    port: number;
  };
}

const ConfigModal: React.FC<ConfigModalProps> = ({ onClose }) => {
  const { apiUrl, setApiUrl, apiClient, setApiClient, apiClientApp, setApiClientApp } = useApi();
  const [activeTab, setActiveTab] = useState<"api" | "mllp">("api");

  // valeurs locales modifiables
  const [tempUrl, setTempUrl] = useState(apiUrl);
  const [tempClient, setTempClient] = useState(apiClient);
  const [tempClientApp, setTempClientApp] = useState(apiClientApp);
  const [tempHost, setTempHost] = useState("");
  const [tempPort, setTempPort] = useState("");

  const [errors, setErrors] = useState<Record<string, string>>({});

  // Validation
  const ipRegex = /^(\d{1,3}\.){3}\d{1,3}$/;
  const portRegex = /^\d{1,5}$/;
  const nameRegex = /^[A-Za-z0-9_-]{1,25}$/;
  const ipOrDomainWithPortRegex = /^(http?:\/\/)?(([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}|localhost|(\d{1,3}\.){3}\d{1,3}):\d{1,5}$/;

  const validateFields = (): boolean => {
    const newErrors: Record<string, string> = {};
    if (!ipOrDomainWithPortRegex.test(tempUrl)) newErrors.apiUrl = "⚠️ URL API invalide";
    if (!nameRegex.test(tempClient)) newErrors.client = "⚠️ Nom client invalide";
    if (!nameRegex.test(tempClientApp)) newErrors.clientApp = "⚠️ ClientApp invalide";
    if (!ipRegex.test(tempHost)) newErrors.mllpHost = "⚠️ IP MLLP invalide";
    if (!portRegex.test(tempPort)) newErrors.mllpPort = "⚠️ Port invalide";

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  // Charger la configuration complète côté backend
    useEffect(() => {
      const loadConfig = async () => {
        try {
          const res = await fetch("/api/config");
          if (!res.ok) {
            console.error("Impossible de charger /api/config");
            return;
          }
          const data: AppConfig = await res.json();

          setTempUrl(data.ris.host);
          setTempClient(data.ris.client);
          setTempClientApp(data.ris.clientApp);
          setTempHost(data.mllp.host);
          setTempPort(data.mllp.port.toString());
        } catch (err) {
          console.error("Erreur chargement config :", err);
        }
      };

      loadConfig();
    }, []);

  // Sauvegarde côté serveur + mise à jour du contexte API
    const handleSaveAll = async () => {
      if (!validateFields()) return;

      const configToSave: AppConfig = {
        ris: {
          host: tempUrl,
          client: tempClient,
          clientApp: tempClientApp,
        },
        mllp: {
          host: tempHost,
          port: parseInt(tempPort, 10),
        },
      };

      await fetch("/api/config", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(configToSave),
      });

      // On met aussi à jour le contexte local (useApi)
      setApiUrl(tempUrl);
      setApiClient(tempClient);
      setApiClientApp(tempClientApp);

      alert("Configuration enregistrée ✔");
      onClose();
    };


  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div className="bg-white rounded-xl w-full max-w-md p-5 shadow-lg">
        <h2 className="text-center bg-blue-600 text-white py-2 rounded mb-4">Configuration</h2>

        {/* Onglets */}
        <div className="flex justify-around mb-4">
          <button
            className={`px-3 py-1 rounded ${activeTab === "api" ? "bg-blue-600 text-white" : "bg-gray-200"}`}
            onClick={() => setActiveTab("api")}
          >
            API HL7
          </button>
          <button
            className={`px-3 py-1 rounded ${activeTab === "mllp" ? "bg-blue-600 text-white" : "bg-gray-200"}`}
            onClick={() => setActiveTab("mllp")}
          >
            MLLP
          </button>
        </div>

        {/* --- API --- */}
        {activeTab === "api" && (
          <div className="flex flex-col gap-3">
            <label className="font-bold">Adresse API :</label>
            <input
              className="w-full border rounded px-2 py-1"
              value={tempUrl}
              onChange={(e) => setTempUrl(e.target.value)}
            />
            {errors.apiUrl && <span className="text-red-600">{errors.apiUrl}</span>}

            <label className="font-bold">Client :</label>
            <input className="w-full border rounded px-2 py-1" value={tempClient} onChange={(e) => setTempClient(e.target.value)} />
            {errors.client && <span className="text-red-600">{errors.client}</span>}

            <label className="font-bold">Application client :</label>
            <input className="w-full border rounded px-2 py-1" value={tempClientApp} onChange={(e) => setTempClientApp(e.target.value)} />
            {errors.clientApp && <span className="text-red-600">{errors.clientApp}</span>}
          </div>
        )}

        {/* --- MLLP --- */}
        {activeTab === "mllp" && (
          <div className="flex flex-col gap-3">
            <label className="font-bold">IP MLLP :</label>
            <input className="w-full border rounded px-2 py-1" value={tempHost} onChange={(e) => setTempHost(e.target.value)} />
            {errors.mllpHost && <span className="text-red-600">{errors.mllpHost}</span>}

            <label className="font-bold">Port :</label>
            <input className="w-full border rounded px-2 py-1" type="number" value={tempPort} onChange={(e) => setTempPort(e.target.value)} />
            {errors.mllpPort && <span className="text-red-600">{errors.mllpPort}</span>}
          </div>
        )}

        <div className="flex justify-end gap-2 mt-4">
          <button className="bg-blue-600 text-white px-4 py-1 rounded hover:bg-blue-700" onClick={handleSaveAll}>
            💾 Enregistrer
          </button>
          <button className="bg-gray-300 px-4 py-1 rounded hover:bg-gray-400" onClick={onClose}>
            Annuler
          </button>
        </div>
      </div>
    </div>
  );
};

export default ConfigModal;

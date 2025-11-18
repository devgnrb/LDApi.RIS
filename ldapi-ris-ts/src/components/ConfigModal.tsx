import React, { useState } from "react";
import { useApi } from "../context/ApiContext";

interface ConfigModalProps {
  onClose: () => void;
  saveMllpConfig?: (host: string, port: number) => Promise<void>;
}

const ConfigModal: React.FC<ConfigModalProps> = ({ onClose, saveMllpConfig }) => {
  const { apiUrl, setApiUrl, apiClient, setApiClient, apiClientApp, setApiClientApp } = useApi();
  const [activeTab, setActiveTab] = useState<"api" | "mllp">("api");

  const [tempUrl, setTempUrl] = useState(apiUrl);
  const [tempClient, setTempClient] = useState(apiClient);
  const [tempClientApp, setTempClientApp] = useState(apiClientApp);
  const [tempHost, setTempHost] = useState("");
  const [tempPort, setTempPort] = useState("");

  const [errors, setErrors] = useState<Record<string, string>>({});

  const ipRegex = /^(\d{1,3}\.){3}\d{1,3}$/;
  const portRegex = /^\d{1,4}$/;
  const nameRegex = /^[A-Za-z0-9_-]{1,25}$/;
  const ipOrDomainWithPortRegex = /^(http?:\/\/)?(([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}|localhost|(\d{1,3}\.){3}\d{1,3}):\d{1,5}$/;

  const validateFields = () => {
    const newErrors: Record<string, string> = {};

    if (!ipOrDomainWithPortRegex.test(tempUrl)) newErrors.apiUrl = "⚠️ L’adresse API doit être au format IP/localhost/[domaine]:port";
    if (!nameRegex.test(tempClient)) newErrors.client = "⚠️ Le nom du client doit contenir max 25 caractères sans espaces.";
    if (!nameRegex.test(tempClientApp)) newErrors.clientApp = "⚠️ Le nom de l’application doit contenir max 25 caractères sans espaces.";
    if (!ipRegex.test(tempHost)) newErrors.mllpHost = "⚠️ L’adresse IP MLLP doit être valide (ex : 127.0.0.1)";
    if (!portRegex.test(tempPort)) newErrors.mllpPort = "⚠️ Le port doit contenir uniquement des chiffres (9999).";

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSaveAll = async () => {
    if (!validateFields()) return;

    setApiUrl(tempUrl);
    setApiClient(tempClient);
    setApiClientApp(tempClientApp);

    if (saveMllpConfig) await saveMllpConfig(tempHost, parseInt(tempPort, 10));

    alert("Configuration enregistrée ✅");
    onClose();
  };

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div className="bg-white rounded-xl w-full max-w-md p-5 shadow-lg">
        <h2 className="text-center bg-blue-600 text-white py-2 rounded mb-4">Configuration</h2>

        {/* Tabs */}
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

        {/* API Tab */}
        {activeTab === "api" && (
          <div className="flex flex-col gap-3">
            <label className="font-bold">Adresse de l’API :</label>
            <input
              className="w-full border rounded px-2 py-1 placeholder-gray-400"
              placeholder="http://192.168.1.10:5033"
              value={tempUrl}
              onChange={e => setTempUrl(e.target.value)}
            />
            {errors.apiUrl && <p className="text-red-600 text-sm">{errors.apiUrl}</p>}

            <label className="font-bold">Application du client :</label>
            <input
              className="w-full border rounded px-2 py-1 placeholder-gray-400"
              placeholder="clientApp"
              value={tempClientApp}
              onChange={e => setTempClientApp(e.target.value)}
            />
            {errors.clientApp && <p className="text-red-600 text-sm">{errors.clientApp}</p>}

            <label className="font-bold">Nom du client :</label>
            <input
              className="w-full border rounded px-2 py-1 placeholder-gray-400"
              placeholder="client"
              value={tempClient}
              onChange={e => setTempClient(e.target.value)}
            />
            {errors.client && <p className="text-red-600 text-sm">{errors.client}</p>}
          </div>
        )}

        {/* MLLP Tab */}
        {activeTab === "mllp" && (
          <div className="flex flex-col gap-3">
            <label className="font-bold">IP MLLP :</label>
            <input
              className="w-full border rounded px-2 py-1 placeholder-gray-400"
              placeholder="127.0.0.1"
              value={tempHost}
              onChange={e => setTempHost(e.target.value)}
            />
            {errors.mllpHost && <p className="text-red-600 text-sm">{errors.mllpHost}</p>}

            <label className="font-bold">Port MLLP :</label>
            <input
              type="number"
              className="w-full border rounded px-2 py-1 placeholder-gray-400"
              placeholder="6661"
              value={tempPort}
              onChange={e => setTempPort(e.target.value)}
            />
            {errors.mllpPort && <p className="text-red-600 text-sm">{errors.mllpPort}</p>}
          </div>
        )}

        {/* Buttons */}
        <div className="flex justify-end gap-2 mt-4">
          <button className="bg-blue-600 text-white px-4 py-1 rounded hover:bg-blue-700" onClick={handleSaveAll}>
            💾 Enregistrer
          </button>
          <button className="bg-gray-300 text-black px-4 py-1 rounded hover:bg-gray-400" onClick={onClose}>
            Annuler
          </button>
        </div>
      </div>
    </div>
  );
};

export default ConfigModal;

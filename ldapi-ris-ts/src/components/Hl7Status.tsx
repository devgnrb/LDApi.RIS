import React from "react";

interface Hl7StatusProps {
  hl7?: string;
  ack?: string;
  status?: "loading" | "success" | "error" | null;
}

const Hl7Status: React.FC<Hl7StatusProps> = ({ hl7, ack, status }) => {
  if (!status) return null;

  const getStatusText = () => {
    switch (status) {
      case "loading": return "⏳ Envoi en cours...";
      case "success": return "✅ HL7 envoyé avec succès";
      case "error": return "❌ Erreur d’envoi";
      default: return "";
    }
  };

  const statusClasses = {
    loading: "bg-yellow-100 text-yellow-800",
    success: "bg-green-100 text-green-800",
    error: "bg-red-100 text-red-800",
  };

  return (
    <div className={`rounded p-3 mt-2 ${statusClasses[status]}`}>
      <p className="font-semibold">{getStatusText()}</p>

      {hl7 && status === "success" && (
        <div className="mt-2 bg-gray-100 p-2 rounded overflow-x-auto">
          <h4 className="font-bold">Message HL7 envoyé :</h4>
          <pre className="whitespace-pre-wrap">{hl7}</pre>

          <h4 className="font-bold mt-2">ACK reçu :</h4>
          <pre className="whitespace-pre-wrap">{ack}</pre>
        </div>
      )}
    </div>
  );
};

export default Hl7Status;

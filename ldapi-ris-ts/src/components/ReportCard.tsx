import React, { useState } from "react";
import { useApi } from "../context/ApiContext";
import { formatDateForLocale } from "../utils/dateUtils";

export type StatusAck = "AA" | "AE" | "AR" | "NL";

export interface Report {
  idReport: number;
  lastName: string;
  firstName: string;
  dateOfBirth: string;
  dateReport: string;
  path: string;
  typeDocument: string;
  envoiHL7: StatusAck;
}

interface ReportCardProps {
  report: Report;
  onStatusChange?: (idReport: number, status: StatusAck) => void;
}

export default function ReportCard({ report, onStatusChange }: ReportCardProps) {
  const { apiUrl, apiClient, apiClientApp } = useApi();
  const [status, setStatus] = useState<"loading" | "success" | "error" | null>(null);
  const [ack, setAck] = useState<StatusAck>("NL");

  // valeur ACK courante (priorise retour API sinon valeur backend)
 const currentAck: StatusAck = ack !== "NL" ? ack : report.envoiHL7;

  // Définition des couleurs par statut ACK
  const borderColorClass = {
    AA: "border-green-500 shadow-green-200",
    AE: "border-red-500 shadow-red-200",
    AR: "border-orange-400 shadow-orange-200",
    NL: "border-blue-500 shadow-blue-200",
  }[currentAck] ?? "border-gray-400";

const sendHl7 = async () => {
  setStatus("loading");

  try {
    const res = await fetch(`${apiUrl}/api/HL7/send`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ id: report.idReport, client: apiClient, clientApp: apiClientApp }),
    });

    const result = await res.json();
    setAck(result.ack);
    setStatus("success");
    onStatusChange?.(report.idReport, result.ack);

  } catch (err) {
    setStatus("error");
  }
};

  // Texte lisible côté UI
  const getAckLabel = (ack: StatusAck): string => {
    switch (ack) {
      case "AA": return "Message accepté";
      case "AE": return "Erreur lors du traitement";
      case "AR": return "Message rejeté";
      case "NL": return "Non envoyé";
    }
  };

  return (
    <div className={`border-4 rounded-lg p-4 shadow-md transition ${borderColorClass}`}>
      <h3 className="text-lg font-semibold text-gray-800 mb-1">{report.lastName} {report.firstName}</h3>
      <p className="text-sm text-gray-600"><span className="font-semibold">ID:</span> {report.idReport}</p>
      <p className="text-sm text-gray-600"><span className="font-semibold">Naissance:</span> {formatDateForLocale(report.dateOfBirth).formatted}</p>
      <p className="text-sm text-gray-600"><span className="font-semibold">Examen:</span> {formatDateForLocale(report.dateReport).formatted}</p>

      {/* Texte du statut ACK */}
      <p className="text-sm font-semibold mt-1">
        Statut HL7:
        <span className="ml-1 font-bold">
          {getAckLabel(currentAck)}
        </span>
      </p>

      <button
        onClick={sendHl7}
        disabled={status === "loading"}
        className={`mt-3 px-4 py-2 rounded-lg font-medium text-white transition-colors ${
          status === "loading" ? "bg-gray-400 cursor-not-allowed" : "bg-blue-600 hover:bg-blue-700"
        }`}
      >
        {status === "loading" ? "Envoi..." : "Envoyer HL7"}
      </button>

      
    </div>
  );
}

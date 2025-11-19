import React, { useState } from "react";
import Hl7Status from "./Hl7Status";
import { useApi } from "../context/ApiContext";

interface Report {
  idReport: number;
  lastName: string;
  firstName: string;
  dateOfBirth: string;
  dateReport:string;
  path:string;
  typeDocument: string;
  envoiHL7?: string;
}

interface ReportCardProps {
  report: Report;
  onStatusChange?: (idReport: number, status: string) => void;
}

export default function ReportCard({ report, onStatusChange }: ReportCardProps) {
  const { apiUrl, apiClient, apiClientApp } = useApi();
  const [status, setStatus] = useState<"loading" | "success" | "error" | null>(null);
  const [hl7, setHl7] = useState("");
  const [ack, setAck] = useState("");
  const envoiHL7 = report.envoiHL7 ?? "Non envoyé";

  const cardClasses = `border-2 rounded-lg p-4 shadow-md transition-all duration-200 ${
    envoiHL7 === "Envoi Réussi"
      ? "border-green-500 shadow-green-200"
      : "border-blue-500 shadow-blue-200"
  }`;

  const sendHl7 = async () => {
    setStatus("loading");
    try {
      const res = await fetch(`/api/HL7/send`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ id: report.idReport, client: apiClient, clientApp: apiClientApp }),
      });
      const result = await res.json();
      setHl7(result.hl7);
      setAck(result.ack);
      setStatus("success");
      // 🔥 Mise à jour immédiate du statut dans le parent
      onStatusChange?.(report.idReport, "Envoi Réussi");

    } catch (err) {
      console.error(err);
      setStatus("error");
    }
  };

  return (
    <div className={cardClasses}>
      <h3 className="text-lg font-semibold text-gray-800 mb-1">{report.lastName} {report.firstName}</h3>
      <p className="text-sm text-gray-600"><span className="font-semibold">ID:</span> {report.idReport}</p>
      <p className="text-sm text-gray-600"><span className="font-semibold">Date de Naissance:</span> {report.dateOfBirth}</p>
      <p className="text-sm text-gray-600"><span className="font-semibold">Date Examen:</span> {report.dateReport}</p>
      <p className="text-sm text-gray-600"><span className="font-semibold">Statut HL7:</span> {envoiHL7 || "Non envoyé"}</p>

      <button
        onClick={sendHl7}
        disabled={status === "loading"}
        className={`mt-3 px-4 py-2 rounded-lg font-medium text-white transition-colors ${
          status === "loading" ? "bg-gray-400 cursor-not-allowed" : "bg-blue-600 hover:bg-blue-700"
        }`}
      >
        {status === "loading" ? "Envoi..." : "Envoyer HL7"}
      </button>

      <Hl7Status hl7={hl7} ack={ack} status={status} />
    </div>
  );
}

import React, { useEffect, useState } from "react";
import ReportCard, { Report, StatusAck } from "./ReportCard";
import { useApi } from "../context/ApiContext";

type FilterType = StatusAck | "ALL";

export default function ReportList() {
  //const { apiUrl } = useApi();
  const { apiUrl, apiClient, apiClientApp } = useApi();
  const [reports, setReports] = useState<Report[]>([]);
  const [loading, setLoading] = useState(false);
  const [sendingBatch, setSendingBatch] = useState(false);
  const [filter, setFilter] = useState<FilterType>("ALL");
  // Table de correspondance des labels
  const ackLabels: Record<FilterType, string> = {
    ALL: "Tous",
    AA: "Accepté",
    AE: "Erreur",
    AR: "Rejeté",
    NL: "Non envoyé",
  };

  const filterColors: Record<FilterType, string> = {
    ALL: "bg-gray-200",
    AA: "bg-green-200",
    AE: "bg-red-200",
    AR: "bg-orange-200",
    NL: "bg-blue-200",
  };
  useEffect(() => {
    const fetchReports = async () => {
      setLoading(true);
      try {
        const response = await fetch(`${apiUrl}/api/Reports`);
        const data: Report[] = await response.json();
        setReports(data);
      } catch (err) {
        console.error("Erreur chargement rapports:", err);
      } finally {
        setLoading(false);
      }
    };

    fetchReports();
  }, [apiUrl]);

  const updateStatus = (idReport: number, status: StatusAck) => {
    setReports(prev =>
      prev.map(r => (r.idReport === idReport ? { ...r, envoiHL7: status } : r))
    );
  };

    const handleSendAll = async () => {
    setSendingBatch(true);

    try {
      const response = await fetch(`${apiUrl}/api/HL7/send-batch`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          client: apiClient,
          clientApp: apiClientApp,
        }),
      });

      if (!response.ok) {
        const errorText = await response.text();
        console.error("Erreur backend send-batch:", errorText);
        throw new Error(errorText || "Erreur HTTP send-batch");
      }

      type BatchItem = { idReport: number; ack?: StatusAck; error?: string };

      const batchResults = (await response.json()) as BatchItem[];

      setReports(prev =>
        prev.map(r => {
          const match = batchResults.find(b => b.idReport === r.idReport);
          return match && match.ack
            ? { ...r, envoiHL7: match.ack }
            : r;
        })
      );

      alert("Envoi batch terminé ✅");
    } catch (error) {
      console.error("Erreur lors de l'envoi batch HL7:", error);
      alert("Erreur lors de l'envoi batch HL7");
    } finally {
      setSendingBatch(false);
    }
  };



  // Filtering
  const filteredReports =
    filter === "ALL" ? reports : reports.filter(r => r.envoiHL7 === filter);

  return (
    <div className="p-5">
      {loading ? (
        <p className="text-center font-bold text-gray-600">⏳ Chargement...</p>
      ) : (
        <>
          {/* Zone filtres + bouton batch */}
          <div className="flex flex-wrap justify-between items-center mb-4 gap-3">
            
            {/* Filtres statut */}
            <div className="flex gap-2">
              {(["ALL", "AA", "AE", "AR", "NL"] as FilterType[]).map(option => (
                <button
                  key={option}
                  onClick={() => setFilter(option)}
                  className={`px-3 py-1 rounded font-semibold border
                    ${filter === option ? "text-white bg-blue-600" : filterColors[option]}`}
                >
                  {ackLabels[option]}
                </button>
              ))}
            </div>

            {/* Bouton envoi global */}
            {reports.length > 0 && (
              <button
                onClick={handleSendAll}
                disabled={sendingBatch}
                className={`px-4 py-2 rounded-lg font-semibold text-white 
                  ${sendingBatch ? "bg-gray-400 cursor-not-allowed" : "bg-green-600 hover:bg-green-700"}`}
              >
                {sendingBatch ? "Envoi global..." : "Envoyer tous"}
              </button>
            )}
          </div>

          {/* Grille rapports filtrés */}
          <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3">
            {filteredReports.map(report => (
              <ReportCard key={report.idReport} report={report} onStatusChange={updateStatus} />
            ))}
          </div>

          {filteredReports.length === 0 && (
            <p className="text-center text-gray-600 mt-6">
              — Aucun rapport pour ce filtre —
            </p>
          )}
        </>
      )}
    </div>
  );
}

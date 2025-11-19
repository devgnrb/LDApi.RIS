import React, { useEffect, useState } from "react";
import ReportCard from "./ReportCard";
import { useApi } from "../context/ApiContext";
import toast from "react-hot-toast";

interface Report {
  idReport: number;
  lastName: string;
  firstName: string;
  dateOfBirth: string;
  dateReport: string;
  path: string;
  typeDocument: string;
  envoiHL7?: string;
}

const ReportList: React.FC = () => {
  const { apiUrl, apiClient, apiClientApp } = useApi();
  const [reports, setReports] = useState<Report[]>([]);
  const [loading, setLoading] = useState(false);
  const [filter, setFilter] = useState<"all" | "sent" | "unsent">("all");
  const [sendingAll, setSendingAll] = useState(false);

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

  const handleStatusChange = (idReport: number, newStatus: string) => {
    setReports(prev =>
      prev.map(r => (r.idReport === idReport ? { ...r, envoiHL7: newStatus } : r))
    );
  };

  const filteredReports = reports.filter((r) => {
    if (filter === "sent") return r.envoiHL7 === "Envoi Réussi";
    if (filter === "unsent") return r.envoiHL7 !== "Envoi Réussi";
    return true;
  });

  const sendAll = async () => {
    const toSend = reports.filter(r => r.envoiHL7 !== "Envoi Réussi");

    if (toSend.length === 0) {
      toast("Aucun rapport à envoyer.", { icon: "ℹ️" });
      return;
    }

    setSendingAll(true);
    toast("Envoi des rapports en cours...");

    for (const report of toSend) {
      try {
        const res = await fetch(`${apiUrl}/api/HL7/send`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            id: report.idReport,
            client: apiClient,
            clientApp: apiClientApp,
          }),
        });

        let result: { ack?: string; hl7?: string } | null = null;
        try {
          result = await res.json();
        } catch {
          // si pas de JSON, on évite de planter juste pour ça
        }

        if (!res.ok) {
          toast.error(`Erreur pour le rapport ${report.idReport}`);
          continue; // on passe au suivant
        }

        // maj UI (bordure verte)
        handleStatusChange(report.idReport, "Envoi Réussi");

        // toast ACK HL7 pour ce rapport avant de passer au suivant
        const ackText = result?.ack ?? "ACK non fourni";
        toast.success(
          `ACK reçu pour ${report.lastName} ${report.firstName} : ${ackText}`
        );
      } catch (err) {
        console.error(err);
        toast.error(`Exception lors de l'envoi du rapport ${report.idReport}`);
      }
    }

    toast.success("Envoi groupé terminé !");
    setSendingAll(false);
  };


  return (
    <div className="p-5">
      <div className="flex gap-3 mb-5">
        <button onClick={() => setFilter("all")}
          className="px-4 py-2 rounded-lg bg-gray-200 hover:bg-gray-300">
          Tous
        </button>

        <button onClick={() => setFilter("unsent")}
          className="px-4 py-2 rounded-lg bg-blue-200 hover:bg-blue-300 border border-blue-600">
          Non envoyés
        </button>

        <button onClick={() => setFilter("sent")}
          className="px-4 py-2 rounded-lg bg-green-200 hover:bg-green-300 border border-green-600">
          Envoyés
        </button>

        <button
          onClick={sendAll}
          disabled={sendingAll}
          className="ml-auto px-6 py-2 rounded-lg bg-indigo-600 text-white font-semibold hover:bg-indigo-700 disabled:bg-gray-400"
        >
           Envoyer tout
        </button>
      </div>

      {loading ? (
        <p className="text-center my-5 font-bold text-gray-600">⏳ Chargement...</p>
      ) : (
        <div className="grid gap-5 grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4">
          {filteredReports.map((report) => (
            <ReportCard key={report.idReport} report={report} onStatusChange={handleStatusChange} />
          ))}
        </div>
      )}
    </div>
  );
};

export default ReportList;

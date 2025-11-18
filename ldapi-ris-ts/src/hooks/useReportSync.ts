import { useState, useEffect } from "react";

interface Report {
  idReport: string | number;
  lastName?: string;
  firstName?: string;
  dateOfBirth?: string;
  typeDocument?: string;
  envoiHL7?: string;
}

interface SingleReportResult {
  success: boolean;
  ack?: string;
  hl7?: string;
  error?: string;
}

export function useReportSync() {
  const [loading, setLoading] = useState<boolean>(false);
  const [reports, setReports] = useState<Report[]>([]);
  const [results, setResults] = useState<any[]>([]);

  useEffect(() => {
    fetch("http://localhost:5000/api/reports")
      .then(res => res.json())
      .then((data: Report[]) => setReports(data))
      .catch(err => console.error("Erreur chargement rapports:", err));
  }, []);

  const syncReports = async (): Promise<void> => {
    setLoading(true);
    try {
      const res = await fetch("http://localhost:5000/api/hl7/send-batch", { method: "POST" });
      const data: any[] = await res.json();
      setResults(data);
    } catch (error) {
      console.error("Erreur synchronisation HL7:", error);
      throw error;
    } finally {
      setLoading(false);
    }
  };

  const sendSingleReport = async (reportId: string | number): Promise<SingleReportResult> => {
    try {
      const response = await fetch(`http://localhost:5000/api/hl7/send/${reportId}`, { method: "POST" });
      if (!response.ok) throw new Error("Erreur serveur : " + response.status);
      const data = await response.json();
      return { success: true, ack: data.ack, hl7: data.hl7 };
    } catch (error: any) {
      return { success: false, error: error.message };
    }
  };

  return { loading, reports, results, syncReports, sendSingleReport };
}
export {};
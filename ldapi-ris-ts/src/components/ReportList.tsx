import React, { useEffect, useState } from "react";
import ReportCard from "./ReportCard";
import { useApi } from "../context/ApiContext";

interface Report {
  idReport: string;
  lastName: string;
  firstName: string;
  dateOfBirth: string;
  dateReport:string;
  path:string;
  typeDocument: string;
  envoiHL7?: string;
}

const ReportList: React.FC = () => {
  const { apiUrl } = useApi();
  const [reports, setReports] = useState<Report[]>([]);
  const [statuses] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const fetchReports = async () => {
      setLoading(true);
      try {
        const response = await fetch(`/api/Reports`);
        const data = await response.json();
        setReports(data);

        // charger le statut hl7 pour chaque rapport
        const statusMap: Record<string,string> ={};
        await Promise.all(
          data.map(async ( report: Report)=>{
            try{
              const res = await fetch(`/api/pdf/envoi-status/${report.idReport}`);
              if (!res.ok) throw new Error("Erreur lors de la récupération du statut HL7");
              const statusData: { envoiHL7?: string } = await res.json();
              
              statusMap[report.idReport] = statusData.envoiHL7 ?? "Non envoyé";
            }catch(err){
              console.error(`Erreur pour le rapport ${report.idReport} :`, err);
              statusMap[report.idReport] = "Non envoyé";
            }
          })
        );
        
      } catch (err) {
        console.error("Erreur chargement rapports:", err);
      } finally {
        setLoading(false);
      }
    };
    fetchReports();
  }, [apiUrl]);

  return (
    <div className="p-5">
      {loading ? (
        <p className="text-center my-5 font-bold text-gray-600">⏳ Chargement...</p>
      ) : (
        <>
          <div className="grid gap-5 grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4">
            {reports.map((report) => (
              <ReportCard key={report.idReport} 
              report={{ ...report, envoiHL7: statuses[report.idReport] }} />
            ))}
          </div>
          {reports.length === 0 && (
            <p className="text-center my-5 font-bold text-gray-600">— Aucun rapport disponible —</p>
          )}
        </>
      )}
    </div>
  );
};

export default ReportList;

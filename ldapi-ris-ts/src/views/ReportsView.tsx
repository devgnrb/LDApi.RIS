import React from "react";
import ReportList from "../components/ReportList";

const ReportsView: React.FC = () => {
  return (
    <div className="min-h-screen bg-gray-100 flex flex-col">

      {/* Contenu principal */}
      <main className="flex-1 p-5">
        <h2 className="text-2xl font-bold text-gray-800 mb-5 text-center md:text-left">
          Liste des rapports
        </h2>
        <ReportList />
      </main>
    </div>
  );
};

export default ReportsView;
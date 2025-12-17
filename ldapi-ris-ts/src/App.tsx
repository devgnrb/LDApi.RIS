import React from "react";
import ReportsView from "../src/views/ReportsView";
import { Toaster } from "react-hot-toast";

const App: React.FC = () => {
  return (
    <>
      {/* Vue principale */}
      <ReportsView />
      <Toaster position="bottom-right" reverseOrder={false} />
    </>
  );
};



export default App;

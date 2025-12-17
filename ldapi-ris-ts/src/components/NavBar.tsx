import React, { useState } from "react";
import ConfigModal from "./ConfigModal";

const NavBar: React.FC = () => {
  const [showConfig, setShowConfig] = useState(false);

  return (
    <>
      <nav className="flex justify-between items-center px-5 py-3 bg-blue-600 text-white shadow-md">
        {/* Logo */}
        <h1 className="text-xl font-bold">🏥 LDA - RIS</h1>

        {/* Desktop config button */}
        <button
          className="hidden md:inline-block bg-white text-blue-600 rounded-md px-3 py-1 hover:opacity-80 transition"
          onClick={() => setShowConfig(true)}
        >
          ⚙️ Configuration
        </button>

        {/* Mobile burger menu */}
        <button
          className="md:hidden text-white focus:outline-none"
          onClick={() => setShowConfig(true)}
        >
          ☰
        </button>
      </nav>

      {/* Config Modal */}
      {showConfig && <ConfigModal onClose={() => setShowConfig(false)} />}
    </>
  );
};

export default NavBar;
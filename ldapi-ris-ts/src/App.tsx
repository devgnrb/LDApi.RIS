// src/App.tsx
import React from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";

import { ApiProvider } from "./context/ApiContext";
import { AuthProvider, useAuth } from "./context/AuthContext";

import ReportsView from "./views/ReportsView";
import LoginView from "./views/LoginView";
import RegisterView from "./views/RegisterView";
import AdminView from "./views/AdminView";

import PrivateRoute from "./router/PrivateRoute";
import PublicRoute from "./router/PublicRoute";
import RoleRoute from "./router/RoleRoute";

import AppNavBar from "./components/AppNavBar";
import { Toaster } from "react-hot-toast";

function AppRoutes() {
  const { isAuthenticated } = useAuth();

  return (
    <Routes>
      {/* Publiques */}

      <Route
        path="/login"
        element={
          <PublicRoute>
            <LoginView />
          </PublicRoute>
        }
      />
      <Route
        path="/register"
        element={
          <PublicRoute>
            <RegisterView />
          </PublicRoute>
        }
      />
      {/* Privées */}
      <Route
        path="/reports"
        element={
          <PrivateRoute>
            <ReportsView />
          </PrivateRoute>
        }
      />

      <Route
        path="/admin"
        element={
          <RoleRoute role="Admin">
            <AdminView />
          </RoleRoute>
        }
      />

      {/* Fallback */}
      <Route
        path="*"
        element={
          isAuthenticated ? (
            <Navigate to="/reports" replace />
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
    </Routes>
  );
}

const App: React.FC = () => {
  return (
    <ApiProvider>
      <AuthProvider>
        <BrowserRouter>
          <AppNavBar />
          <AppRoutes />
          <Toaster position="bottom-right" />
        </BrowserRouter>
      </AuthProvider>
    </ApiProvider>
  );
};

export default App;

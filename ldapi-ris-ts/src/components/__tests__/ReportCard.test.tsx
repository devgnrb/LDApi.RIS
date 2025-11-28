import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import ReportCard, { Report, StatusAck } from "../ReportCard";
import { ApiProvider } from "../../context/ApiContext";
/**
 * Test d'affichage des informations du rapport
 * Test de mettre à jour le statut après un envoi réussi (AA)
 * Test de mettre à jour le statut avec AE (Application Client renvoie une erreur sur le message HL7)
 * Test de mettre à jour le statut avec AR (Application Client rejete le message HL7)
 */

// Un rapport de base pour les tests
const mockReport: Report = {
  idReport: 1,
  lastName: "Doe",
  firstName: "John",
  dateOfBirth: "1990-01-01",
  dateReport: "2025-01-05",
  path: "",
  typeDocument: "PDF",
  envoiHL7: "NL",
};

describe("ReportCard", () => {
  beforeEach(() => {
    // On réinitialise fetch avant chaque test
    (global.fetch as jest.Mock | undefined) = jest.fn();
  });

  test("affiche les informations du rapport", () => {
    render(
      <ApiProvider>
        <ReportCard report={mockReport} />
      </ApiProvider>
    );

    expect(screen.getByText(/Doe John/i)).toBeInTheDocument();
    expect(screen.getByText(/Statut HL7:/i)).toBeInTheDocument();
    expect(screen.getByText(/Non envoyé/i)).toBeInTheDocument();
  });

  test("met à jour le statut après un envoi réussi (AA)", async () => {
    const handleStatusChange = jest.fn();

    // 👉 fetch renvoie explicitement AA pour CE test
    (fetch as jest.Mock).mockResolvedValueOnce({
      json: () => Promise.resolve({ ack: "AA" as StatusAck }),
    } as any);

    render(
      <ApiProvider>
        <ReportCard report={mockReport} onStatusChange={handleStatusChange} />
      </ApiProvider>
    );

    fireEvent.click(screen.getByRole("button", { name: /Envoyer HL7/i }));

    await waitFor(() => {
      expect(handleStatusChange).toHaveBeenCalledWith(1, "AA");
    });

    // Optionnel : vérifier le texte affiché
    expect(screen.getByText(/Message accepté/i)).toBeInTheDocument();
  });

  test("met à jour le statut avec AE (Application Error)", async () => {
    const handleStatusChange = jest.fn();

    (fetch as jest.Mock).mockResolvedValueOnce({
      json: () => Promise.resolve({ ack: "AE" as StatusAck }),
    } as any);

    render(
      <ApiProvider>
        <ReportCard report={mockReport} onStatusChange={handleStatusChange} />
      </ApiProvider>
    );

    fireEvent.click(screen.getByRole("button", { name: /Envoyer HL7/i }));

    await waitFor(() => {
      expect(handleStatusChange).toHaveBeenCalledWith(1, "AE");
    });

    expect(screen.getByText(/Erreur lors du traitement/i)).toBeInTheDocument();
  });

  test("met à jour le statut avec AR (Application Reject)", async () => {
    const handleStatusChange = jest.fn();

    (fetch as jest.Mock).mockResolvedValueOnce({
      json: () => Promise.resolve({ ack: "AR" as StatusAck }),
    } as any);

    render(
      <ApiProvider>
        <ReportCard report={mockReport} onStatusChange={handleStatusChange} />
      </ApiProvider>
    );

    fireEvent.click(screen.getByRole("button", { name: /Envoyer HL7/i }));

    await waitFor(() => {
      expect(handleStatusChange).toHaveBeenCalledWith(1, "AR");
    });

    expect(screen.getByText(/Message rejeté/i)).toBeInTheDocument();
  });

  test("la carte applique la classe de bordure orange lorsque AR est reçu", async () => {
    const handleStatusChange = jest.fn();

    (fetch as jest.Mock).mockResolvedValueOnce({
      json: () => Promise.resolve({ ack: "AR" as StatusAck }),
    } as any);

    const { container } = render(
      <ApiProvider>
        <ReportCard report={mockReport} onStatusChange={handleStatusChange} />
      </ApiProvider>
    );

    fireEvent.click(screen.getByRole("button", { name: /Envoyer HL7/i }));

    await waitFor(() => {
      const cardElement = container.querySelector("div");
      expect(cardElement).toHaveClass("border-orange-500");
    });
  });
});

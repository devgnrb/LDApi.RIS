import { render, screen, waitFor, fireEvent } from "@testing-library/react";
import ReportList from "../ReportList";
import { ApiProvider } from "../../context/ApiContext";
import { StatusAck } from "../ReportCard";
/**
 * Test d'affichage des rapports après chargement
 * Test d'affichage d'un message si aucun rapport
 * Test de mettre à jour le statut d’un rapport après updateStatus via ReportCard
 */
// === MOCK DATA pour /api/Reports ===
const mockReports = [
  {
    idReport: 1,
    lastName: "Doe",
    firstName: "John",
    dateOfBirth: "1990-01-01",
    dateReport: "2025-01-05",
    path: "",
    typeDocument: "PDF",
    envoiHL7: "NL" as StatusAck,
  },
  {
    idReport: 2,
    lastName: "Smith",
    firstName: "Anna",
    dateOfBirth: "1985-09-12",
    dateReport: "2025-01-12",
    path: "",
    typeDocument: "PDF",
    envoiHL7: "AA" as StatusAck,
  },
];

describe("ReportList", () => {
  beforeEach(() => {
    (global.fetch as jest.Mock | undefined) = jest.fn();
  });

  test("affiche des rapports après chargement", async () => {
    (fetch as jest.Mock).mockResolvedValueOnce({
      json: () => Promise.resolve(mockReports),
    } as any);

    render(
      <ApiProvider>
        <ReportList />
      </ApiProvider>
    );

    expect(await screen.findByText(/Doe/i)).toBeInTheDocument();
    expect(await screen.findByText(/Smith/i)).toBeInTheDocument();
  });

  test("affiche un message si aucun rapport", async () => {
    (fetch as jest.Mock).mockResolvedValueOnce({
      json: () => Promise.resolve([]),
    } as any);

    render(
      <ApiProvider>
        <ReportList />
      </ApiProvider>
    );

    expect(
      await screen.findByText(/Aucun rapport disponible/i)
    ).toBeInTheDocument();
  });

  test("met à jour le statut d’un rapport après updateStatus via ReportCard", async () => {
    const fetchMock = fetch as jest.Mock;

    // 1er appel: /api/Reports → renvoie la liste des rapports
    // 2e appel: /api/HL7/send → renvoie ack AA
    fetchMock
      .mockResolvedValueOnce({
        json: () => Promise.resolve(mockReports),
      } as any)
      .mockResolvedValueOnce({
        json: () => Promise.resolve({ ack: "AA" as StatusAck }),
      } as any);

    render(
      <ApiProvider>
        <ReportList />
      </ApiProvider>
    );

    // Attendre que la liste s’affiche
    await waitFor(() => {
      expect(screen.getByText(/Doe/i)).toBeInTheDocument();
    });

    // Clic sur le bouton "Envoyer HL7" du premier rapport
    const sendButtons = screen.getAllByRole("button", { name: /Envoyer HL7/i });
    fireEvent.click(sendButtons[0]);

    // Attendre la mise à jour (AA → "Message accepté" dans la card)
    await waitFor(() => {
      expect(screen.getByText(/Message accepté/i)).toBeInTheDocument();
    });
  });
});

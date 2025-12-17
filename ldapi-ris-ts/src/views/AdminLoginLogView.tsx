import { useEffect, useState } from "react";
import { useAuthFetch } from "../utils/authFetch";

interface LogDto {
    timestampUtc: string;
    userName: string;
    success: boolean;
    ipAddress: string;
    failureReason?: string;
}

export default function AdminLoginLogsView() {
    const { authFetch } = useAuthFetch();
    const [logs, setLogs] = useState<LogDto[]>([]);
    const [page, setPage] = useState(1);
    const [username, setUsername] = useState("");
    const [success, setSuccess] = useState<string>("");

    useEffect(() => {
        load();
    }, [page, username, success]);

    const load = async () => {
        const params = new URLSearchParams({
            page: page.toString(),
            pageSize: "20",
            ...(username && { username }),
            ...(success && { success }),
        });

        const res = await authFetch(`/api/admin/login-audit?${params}`);
        if (res.ok) {
            const data = await res.json();
            setLogs(data.logs);
        }
    };

    return (
        <div className="p-6">
            <h2 className="text-xl font-bold mb-4">Logs de connexion</h2>

            {/* Filtres */}
            <div className="flex gap-2 mb-4">
                <input
                    className="border p-2 rounded"
                    placeholder="Utilisateur"
                    value={username}
                    onChange={e => setUsername(e.target.value)}
                />

                <select
                    className="border p-2 rounded"
                    value={success}
                    onChange={e => setSuccess(e.target.value)}
                >
                    <option value="">Tous</option>
                    <option value="true">Succès</option>
                    <option value="false">Échec</option>
                </select>
            </div>

            {/* Table */}
            <table className="w-full border">
                <thead className="bg-gray-100">
                    <tr>
                        <th className="border p-2">Date</th>
                        <th className="border p-2">Utilisateur</th>
                        <th className="border p-2">IP</th>
                        <th className="border p-2">Résultat</th>
                        <th className="border p-2">Raison</th>
                    </tr>
                </thead>
                <tbody>
                    {logs.map((l, i) => (
                        <tr key={i}>
                            <td className="border p-2">
                                {new Date(l.timestampUtc).toLocaleString()}
                            </td>
                            <td className="border p-2">{l.userName}</td>
                            <td className="border p-2">{l.ipAddress}</td>
                            <td className="border p-2">
                                {l.success ? "✅" : "❌"}
                            </td>
                            <td className="border p-2 text-red-600">
                                {l.failureReason}
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>

            {/* Pagination */}
            <div className="flex gap-2 mt-4">
                <button onClick={() => setPage(p => Math.max(1, p - 1))}>
                    ◀
                </button>
                <span>Page {page}</span>
                <button onClick={() => setPage(p => p + 1)}>▶</button>
            </div>
        </div>
    );
}

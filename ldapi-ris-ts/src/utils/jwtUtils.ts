export function decodeJwt(token: unknown): any | null {
    if (typeof token !== "string") {
        return null;
    }

    try {
        const payload = token.split(".")[1];
        if (!payload) return null;

        const decoded = JSON.parse(atob(payload));
        return decoded;
    } catch (error) {
        console.error("Erreur décodage JWT :", error);
        return null;
    }
}
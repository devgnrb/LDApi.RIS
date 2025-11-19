export const formatDateLocal = (raw: string): string => {
  if (!raw || raw.length < 8) return raw;

  const year = parseInt(raw.substring(0, 4));
  const month = parseInt(raw.substring(4, 6)) - 1;
  const day = parseInt(raw.substring(6, 8));

  let hour = 0, minute = 0, second = 0;

  if (raw.length >= 10) hour = parseInt(raw.substring(8, 10));
  if (raw.length >= 12) minute = parseInt(raw.substring(10, 12));
  if (raw.length >= 14) second = parseInt(raw.substring(12, 14));

  const date = new Date(Date.UTC(year, month, day, hour, minute, second));

  const options: Intl.DateTimeFormatOptions =
    raw.length >= 10
      ? {
          year: "numeric",
          month: "2-digit",
          day: "2-digit",
          hour: "2-digit",
          minute: "2-digit",
          timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone,
        }
      : {
          year: "numeric",
          month: "2-digit",
          day: "2-digit",
          timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone,
        };

  return date.toLocaleString("fr-FR", options);
};

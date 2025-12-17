import dayjs, { Dayjs } from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
import localizedFormat from "dayjs/plugin/localizedFormat";
import utc from "dayjs/plugin/utc";
import timezone from "dayjs/plugin/timezone";


// Locales disponibles 
// europe
import "dayjs/locale/fr";
import "dayjs/locale/en";
import "dayjs/locale/de";
import "dayjs/locale/it";
import "dayjs/locale/es";
import "dayjs/locale/nl";
import "dayjs/locale/pl";
import "dayjs/locale/lt";
// Asie
import "dayjs/locale/zh";
import "dayjs/locale/ko";

dayjs.extend(customParseFormat);
dayjs.extend(localizedFormat);
dayjs.extend(utc);
dayjs.extend(timezone);

export interface FormatResult {
  formatted: string | null; // affichage
  date: Dayjs | null;       // objet Dayjs utilisable
  error?: string;           // message explicite en cas de problème
}

/**
 * Convertit une chaîne brute (YYYYMMDDHHmmss, YYYYMMDD) en date locale formatée.
 */
export const formatDateForLocale = (
  raw: string,
  locale: string = navigator.language.split("-")[0],
  tz: string = Intl.DateTimeFormat().resolvedOptions().timeZone
): FormatResult => {
  if (!raw) {
    return { formatted: null, date: null, error: "input-is-empty" };
  }

  if (raw.length < 8) {
    return { formatted: raw, date: null, error: "input-too-short" };
  }

  // Déduction du format d'entrée
  let inputFormat = "YYYYMMDD";
  if (raw.length >= 14) inputFormat = "YYYYMMDDHHmmss";
  else if (raw.length >= 12) inputFormat = "YYYYMMDDHHmm";
  else if (raw.length >= 10) inputFormat = "YYYYMMDDHH";

  // Parsing Dayjs
  const d = dayjs(raw, inputFormat).utc().tz(tz).locale(locale);

  if (!d.isValid()) {
    return { formatted: raw, date: null, error: "invalid-date-format" };
  }

  // Format d'affichage
  const outputFormat = raw.length >= 10 ? "L LT" : "L";

  return {
    formatted: d.format(outputFormat),
    date: d,
  };
};



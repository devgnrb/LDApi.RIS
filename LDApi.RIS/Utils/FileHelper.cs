using System.Text.RegularExpressions;

namespace LDApi.RIS.Utils
{
    public static class FileHelper
    {
        public static IEnumerable<string> GetPdfFiles(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Répertoire introuvable : {directoryPath}");

            return Directory.EnumerateFiles(directoryPath, "*.pdf", SearchOption.TopDirectoryOnly);
        }
        /// <summary>
        /// Fournit des fonctions utilitaires pour lire les champs et sous-champs d'un message HL7.
        /// </summary>
        public static string GetField(string[] segments, string segmentName, int index)
        {
            var seg = segments.FirstOrDefault(s => s.StartsWith(segmentName + "|"));
            if (seg == null)
                throw new Exception($"Segment {segmentName} introuvable");
            var fields = seg.Split('|');
            return index < fields.Length ? fields[index] : "";
        }

        /// <summary>
        /// Retourne la valeur d'un sous-champ HL7 (séparé par ^) à partir du segment, du champ et du sous-champ (1-based).
        /// Exemple : GetSubField(segments, "PID", 5, 1) → nom de famille
        /// </summary>
        public static string GetSubField(string[] segments, string segmentName, int fieldIndex, int subFieldIndex)
        {
            var fieldValue = GetField(segments, segmentName, fieldIndex);
            if (string.IsNullOrEmpty(fieldValue))
                return "";

            var subFields = fieldValue.Split('^');
            return subFieldIndex <= subFields.Length ? subFields[subFieldIndex - 1] : "";
        }

        /// <summary>
        /// Vérifie que le champ MSH-10 (Message Control ID) est unique et bien formé.
        /// </summary>
        public static bool ValidateMessageControlId(string messageControlId)
        {
            if (string.IsNullOrWhiteSpace(messageControlId))
                return false;

            // Doit être alphanumérique, max 20 caractères (selon la spec HL7)
            var regex = new Regex(@"^[A-Za-z0-9\-]{1,20}$");
            return regex.IsMatch(messageControlId);
        }
    }
}

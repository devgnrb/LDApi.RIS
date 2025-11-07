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
    }
}

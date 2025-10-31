namespace LDApi.RIS.Interfaces
{
    public interface IMetaDataPDFService
    {
        void AddMetaDataToPdf(string pdfPath, string envoi);
        bool CheckMetaData(string pdfPath, string key, string expectedValue);
        public string? GetMetaDataValue(string pdfPath, string key);
    }
}

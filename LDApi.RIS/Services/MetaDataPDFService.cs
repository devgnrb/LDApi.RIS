using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace LDApi.RIS.Services
{
    public class MetaDataPDFService : Interfaces.IMetaDataPDFService
    {
        public void AddMetaDataToPdf(string pdfPath, string envoi)
        {
            try
            {
                using (PdfSharp.Pdf.PdfDocument document = PdfSharp.Pdf.IO.PdfReader.Open(pdfPath, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Modify))
                {
                    document.Info.Title = "LDA Dyneelax Report";
                    document.Info.Subject = "Knee Laxity Analysis Report";
                    document.Info.Elements["/EnvoiHL7"] = new PdfSharp.Pdf.PdfString(envoi);
                    document.Info.Creator = "Dyneelax Software";
                    document.Save(pdfPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public bool CheckMetaData(string pdfPath, string key, string expectedValue)
        {
            using (PdfDocument document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import))
            {
                var info = document.Info.Elements;

                if (info.ContainsKey("/" + key))
                {
                    string value = info["/" + key].ToString();
                    return value == expectedValue;
                }
                return false;
            }
        }
        /// <summary>
        /// Lit la valeur d'une métadonnée dans un PDF
        /// </summary>
        /// <param name="pdfPath">Chemin du PDF</param>
        /// <param name="key">Nom de la métadonnée (ex : "EnvoiHL7")</param>
        /// <returns>Valeur de la métadonnée ou null si absente</returns>
        public string? GetMetaDataValue(string pdfPath, string key)
        {
            try
            {
                // Ouvrir le PDF en lecture (Import = lecture seule)
                using (PdfDocument document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import))
                {
                    var info = document.Info.Elements;
                    string pdfKey = "/" + key;

                    if (info.ContainsKey(pdfKey))
                    {
                        // On cast en PdfString pour récupérer la valeur proprement
                        if (info[pdfKey] is PdfString pdfString)
                        {
                            return pdfString.Value;
                        }

                        // Si ce n'est pas un PdfString, fallback sur ToString()
                        return info[pdfKey].ToString();
                    }

                    return null; // clé absente
                }
            }
            catch
            {
                return null; // en cas d'erreur, retourne null
            }
        }

    }
}

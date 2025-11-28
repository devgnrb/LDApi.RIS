using System.Text.Json.Serialization;

namespace LDApi.RIS.Dto
{
    public class HL7ResponseDto
    {

        [JsonPropertyName("ack")]
        public StatusAck Ack { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StatusAck
    {
        AA,
        AE,
        AR,
        NL,
    }
    // signification HL7 V2.5
    //AA – Application Accept : Le message a été accepté et traité avec succès.
    //AE – Application Error : Le message a été reçu mais un problème d’application est survenu lors du traitement. 
    //AR – Application Reject : Le message a été rejeté par l’application.
    //NL - Message non lancé.
}

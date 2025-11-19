using System.Text.Json.Serialization;

namespace LDApi.RIS.Dto
{
    public class HL7ResponseDto
    {
        [JsonPropertyName("hl7")]
        public string? Hl7 { get; set; }
        [JsonPropertyName("ack")]
        public string? Ack { get; set; }
    }
}

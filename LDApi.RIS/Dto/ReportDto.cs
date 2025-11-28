namespace LDApi.RIS.Dto
{
    public class ReportDto
    {
        public required int IdReport { get; init; }
        public required string LastName { get; set; }
        public required string FirstName { get; set; }
        public required string DateOfBirth { get; set; }
        public required string DateReport { get; set; }
        public required string Path { get; set; }
        public required string TypeDocument { get; set; } = "Laximetrie Dynamique";
        public required StatusAck EnvoiHL7 { get; set; }

    }
}

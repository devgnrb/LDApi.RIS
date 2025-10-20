namespace LDApi.RIS.Dto
{
    public class ReportDto
    {
        public int IdReport { get; init; }
        public required string LastName { get; set; }
        public required string FirstName { get; set; }
        public string DateOfBirth { get; set; } = "Unknown";
        public required string DateReport { get; set; }
        public required string Path { get; set; }
        public required string TypeDocument { get; set; } = "Laximétrie Dynamique";
    }
}

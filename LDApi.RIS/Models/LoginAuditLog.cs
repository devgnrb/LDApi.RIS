namespace LDApi.RIS.Models
{
    public class LoginAuditLog
    {
        public int Id { get; set; }

        public string? UserName { get; set; }
        public string? UserId { get; set; }

        public DateTime TimestampUtc { get; set; }

        public bool Success { get; set; }

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public string? FailureReason { get; set; }
    }
}
namespace LDApi.RIS.Dto
{
    public class SetRoleDto
    {
        public string Username { get; set; } = "";
        public string Role { get; set; } = ""; // "Admin" ou "User"
    }

}
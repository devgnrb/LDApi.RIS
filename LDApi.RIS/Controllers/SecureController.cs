// SecureController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace LDApi.RIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecureController : ControllerBase
    {
        [HttpGet("secret-data")]
        [Authorize]
        public IActionResult GetSecretData()
        {
            // Tu peux récupérer l'identité / user via User.Claims, User.Identity, etc.
            var userName = User.Identity?.Name;
            return Ok(new { message = "Ceci est une donnée protégée", user = userName });
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LDApi.RIS.Dto;
using Microsoft.AspNetCore.Identity;
using LDApi.RIS.Models;
using System.Threading.Tasks;
using LDApi.RIS.Data;

namespace LDApi.RIS.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _db;
        private readonly IConfiguration _configuration;


        public AdminController(UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            AuthDbContext authDbContext
        )
        {
            _userManager = userManager;
            _configuration = configuration;
            _db = authDbContext;
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            var users = _userManager.Users.ToList();

            var result = new List<object>();


            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                // Recherche des roles 
                result.Add(new
                {
                    userName = user.UserName,
                    email = user.Email,
                    role = roles.FirstOrDefault() ?? "User"
                });
            }


            return Ok(result);
        }

        [HttpGet("only")]
        [Authorize(Roles = "Admin")]
        public IActionResult OnlyAdmin()
        {
            return Ok(new { message = "Accès Admin OK" });
        }

        [HttpPost("set-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetRole([FromBody] SetRoleDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);
            if (user == null) return NotFound("Utilisateur non trouvé");


            // Empêcher un admin de se retirer lui-même le rôle
            var currentUserName = User.Identity?.Name;
            if (dto.Username == currentUserName && dto.Role != "Admin")
            {
                return BadRequest("Impossible de retirer son propre rôle Admin");
            }

            if (user == null)
                return NotFound();

            // Vérification du rôle cible
            if (dto.Role != "Admin" && dto.Role != "User")
                return BadRequest("Rôle invalide");

            // Retire les rôles existants
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Ajoute le nouveau rôle
            var result = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { message = $"Role set to {dto.Role}" });
        }

        [HttpGet("login-audit")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetLoginAudit(
            int page = 1,
            int pageSize = 25,
            string? username = null,
            bool? success = null)
        {
            var query = _db.LoginAuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(username))
                query = query.Where(x => x.UserName!.Contains(username));

            if (success.HasValue)
                query = query.Where(x => x.Success == success.Value);

            var total = query.Count();

            var logs = query
                .OrderByDescending(x => x.TimestampUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.TimestampUtc,
                    x.UserName,
                    x.Success,
                    x.IpAddress,
                    x.UserAgent,
                    x.FailureReason
                })
                .ToList();

            return Ok(new
            {
                total,
                page,
                pageSize,
                logs
            });
        }


    }
}
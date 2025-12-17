// AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LDApi.RIS.Dto;
using System.Security.Cryptography;
using LDApi.RIS.Models;
using LDApi.RIS.Data;

namespace LDApi.RIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AuthDbContext _db;
        private readonly ILogger<AuthController> _logger;
        public AuthController(UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            AuthDbContext db,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _db = db;
            _logger = logger;
        }



        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new ApplicationUser { UserName = dto.Username, Email = dto.Email };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }
            return Ok(new { message = "User created" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);
            if (user == null)
            {
                await LogLoginAttempt(dto.Username, null, false, "Utilisateur inconnu");
                _logger.LogWarning("Login a échoué pour l' {User}", dto.Username);
                return Unauthorized();
            }
            if (await _userManager.IsLockedOutAsync(user))
            {
                await LogLoginAttempt(user.UserName, user.Id, false, "Compte verrouillé");
                return Unauthorized("Compte verrouillé temporairement");
            }
            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!valid)
            {
                await _userManager.AccessFailedAsync(user); // incrémente le compteur
                await LogLoginAttempt(user.UserName, user.Id, false, "Mot de passe invalide");
                _logger.LogWarning("Invalid password for {User}", user.UserName);
                return Unauthorized();
            }
            // succes avec reset du compteur
            await _userManager.ResetAccessFailedCountAsync(user);

            var token = await CreateJwtAsync(user);

            // Génération du RefreshToken
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(24);
            await _userManager.UpdateAsync(user);

            await LogLoginAttempt(user.UserName, user.Id, true, null);

            _logger.LogInformation("User {User} logged in successfully", user.UserName);


            return Ok(new
            {
                token,
                refreshToken,
                refreshTokenExpiryTime = user.RefreshTokenExpiryTime // Expiration du jeton
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.Username);
            if (user == null)
                return Unauthorized("Invalid user");

            if (user.RefreshToken != dto.RefreshToken)
                return Unauthorized("Invalid refresh token");

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized("Refresh token expired");

            var newJwt = await CreateJwtAsync(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(24);

            await _userManager.UpdateAsync(user);
            await LogLoginAttempt(dto.Username, user.Id, true, "Refresh OK");
            return Ok(new
            {
                token = newJwt,
                refreshToken = newRefreshToken,
                refreshTokenExpiryTime = user.RefreshTokenExpiryTime
            });
        }

        private async Task<string> CreateJwtAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? "")
            };

            // Ajout des rôles dans le token
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create(); // Utilisation de chiffre aléatoire avec RandomNumber pour le toker
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task LogLoginAttempt(
            string? username,
            string? userId,
            bool success,
            string? reason)
        {
            var log = new LoginAuditLog
            {
                UserName = username,
                UserId = userId,
                Success = success,
                FailureReason = reason,
                TimestampUtc = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            };

            _db.LoginAuditLogs.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}

using LDApi.RIS.Dto;
using LDApi.RIS.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LDApi.RIS.Controllers
{
    [ApiController]
    [Route("api/config")]
    public class ConfigController : ControllerBase
    {
        private readonly IMllpConfigurationService _mllpConfig;

        public ConfigController(IMllpConfigurationService mllpConfig)
        {
            _mllpConfig = mllpConfig;
        }

        [HttpPost("mllp")]
        public IActionResult UpdateMllpConfig([FromBody] MllpConfigDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Host) || dto.Port <= 0)
                return BadRequest("Configuration MLLP invalide.");

            _mllpConfig.Host = dto.Host;
            _mllpConfig.Port = dto.Port;
            return Ok(new { message = $"MLLP mis à jour : {dto.Host}:{dto.Port}" });
        }

        [HttpGet("mllp")]
        public IActionResult GetMllpConfig()
        {
            return Ok(new { host = _mllpConfig.Host, port = _mllpConfig.Port });
        }
    }


}

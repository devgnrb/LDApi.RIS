using LDApi.RIS.Services;
using LDApi.RIS.Config;
using Microsoft.AspNetCore.Mvc;


namespace LDApi.RIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly ConfigurationService _configService;

        public ConfigController(ConfigurationService configService)
        {
            _configService = configService;
        }

        [HttpGet]
        public IActionResult Get() => Ok(_configService.Config);

        [HttpPut]
        public IActionResult Update([FromBody] AppConfig config)
        {
            _configService.SaveConfig(config);
            return Ok(config);
        }
    }
}
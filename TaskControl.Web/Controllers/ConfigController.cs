using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TaskControl.Core.AppSettings;

namespace TaskControl.Web.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly AppSettings _settings;

        public ConfigController(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
        }

        [HttpGet]
        public IActionResult GetConfig()
        {
            return Ok(new
            {
                _settings.PickupWindowLimitHours,
                _settings.DeliveryWindowLimitHours,
                _settings.WeightCoefficient
            });
        }
    }
}
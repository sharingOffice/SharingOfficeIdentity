using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharingOffice.Api.Infra.Authorizations;

namespace SharingOffice.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> _logger;

        public ValuesController(ILogger<ValuesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("true");
        }

        [HttpPost, Route("[action]"), Authorize(Roles = CustomRoles.Admin)]
        public IActionResult CheckAuthorization()
        {
            return Ok("true");
        }
    }
}
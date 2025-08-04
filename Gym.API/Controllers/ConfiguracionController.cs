using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Importar para proteger el endpoint

namespace Gym.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ConfiguracionController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ConfiguracionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [Authorize]
        [HttpGet("clave-secreta")]
        public IActionResult ObtenerClaveSecreta()
        {
            var claveSecreta = _configuration["ClaveSecreta:Key"];
            if (string.IsNullOrEmpty(claveSecreta))
            {
                return NotFound("Clave secreta no encontrada.");
            }

            return Ok(new { ClaveSecreta = claveSecreta });
        }
    }
}

using Gym.BLL.Servicios.Contrato;
using Gym.DTO;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;







namespace Gym.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]

    public class EmailController : Controller
    {

        private readonly IEmailService _emailService;
        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        public IActionResult SendEmail(EmailDTO request)
        {
            _emailService.SendEmail(request);
            return Ok();
        }

        //[HttpPost]
        //public IActionResult SendEmail2(EmailDTO request)
        //{
        //    try
        //    {
        //        _emailService.SendEmail2(request);
        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Registrar la excepción

        //        return StatusCode(500, "Error al enviar el correo electrónico: " + ex.Message);
        //    }

        //}



    }
}


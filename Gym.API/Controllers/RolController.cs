using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Gym.Api.Utilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Gym.BLL.Servicios.Contrato;
using Gym.DTO;


namespace Gym.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class RolController : ControllerBase
    {

        private readonly IRolService _rolServicio;

        public RolController(IRolService rolServicio)
        {
            _rolServicio = rolServicio;
        }
        //[Authorize]
        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> Lista()
        {

            var rsp = new Response<List<RolDTO>>();

            try
            {
                rsp.status = true;
                rsp.value = await _rolServicio.Lista();


            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }
            return Ok(rsp);




        }
    }
}

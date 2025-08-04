using Gym.Api.Utilidad;
using Gym.BLL.Servicios.Contrato;
using Gym.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class DashBoardController : ControllerBase
    {

        private readonly IDashBoardService _dashboardServicio;

        public DashBoardController(IDashBoardService dashboardServicio)
        {
            _dashboardServicio = dashboardServicio;
        }

        //[Authorize]
        [HttpGet]
        [Route("Resumen")]
        public async Task<IActionResult> Resumen()
        {

            var rsp = new Response<DashBoardDTO>();

            try
            {
                rsp.status = true;
                rsp.value = await _dashboardServicio.Resumen();


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

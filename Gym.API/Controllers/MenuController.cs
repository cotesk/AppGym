using Gym.BLL.Servicios.Contrato;
using Gym.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gym.Api.Utilidad;

namespace Gym.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuServicio;

        private readonly ILogger<MenuController> _logger;

        public MenuController(IMenuService menuServicio, ILogger<MenuController> logger)
        {
            _menuServicio = menuServicio;
            _logger = logger;
        }
        //[Authorize]
        [HttpGet]
        [Route("ListaMenu")]
        public async Task<IActionResult> Lista()
        {
            //aqui devuelve una lista de categoria DTO
            var rsp = new Response<List<MenuDTO>>();

            try
            {
                rsp.status = true;
                rsp.value = await _menuServicio.ListaMenu();


            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }
            return Ok(rsp);




        }

        [Authorize]
        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> Lista(int idUsuario)
        {

            var rsp = new Response<List<MenuDTO>>();

            try
            {
                rsp.status = true;
                rsp.value = await _menuServicio.Lista(idUsuario);


            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }
            return Ok(rsp);

        }


        [Authorize]
        [HttpPost]
        [Route("AgregarPermiso")]
        public async Task<IActionResult> AgregarPermiso([FromBody] PermisoDto permiso)
        {
            var rsp = new Response<bool>();

            try
            {
                Console.WriteLine("Datos recibidos desde Angular - idRol: {0}, idMenu: {1}", permiso.IdRol, permiso.IdMenu);

                // Intentar agregar el permiso
                rsp.status = await _menuServicio.AgregarPermiso(permiso.IdRol, permiso.IdMenu);
                rsp.value = rsp.status;
            }
            catch (DbUpdateException ex)
            {
                // Capturar la excepción de Entity Framework Core
                rsp.status = false;
                rsp.msg = "Error al guardar los cambios en la base de datos.";
                _logger.LogError(ex, "Error al guardar los cambios en la base de datos al agregar un permiso.");
            }
            catch (Exception ex)
            {
                // Capturar cualquier otra excepción
                rsp.status = false;
                rsp.msg = ex.Message;
                _logger.LogError(ex, "Error inesperado al agregar un permiso.");
            }

            return Ok(rsp);
        }

        [Authorize]
        [HttpPost]
        [Route("EliminarPermiso")]
        public async Task<IActionResult> EliminarPermiso([FromBody] PermisoDto permiso)
        {
            var rsp = new Response<bool>();

            try
            {
                Console.WriteLine("Datos recibidos desde Angular para eliminar permiso - idRol: {0}, idMenu: {1}", permiso.IdRol, permiso.IdMenu);

                // Intentar eliminar el permiso
                rsp.status = await _menuServicio.EliminarPermiso(permiso.IdRol, permiso.IdMenu);
                rsp.value = rsp.status;
            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }
            return Ok(rsp);
        }


        //[HttpPost]
        //[Route("ModificarPermiso")]
        //public async Task<IActionResult> ModificarPermiso(int idRol, int idMenu, bool agregar)
        //{
        //    var rsp = new Response<bool>();

        //    try
        //    {
        //        if (agregar)
        //        {
        //            rsp.status = await _menuServicio.AgregarPermiso(idRol, idMenu);
        //        }
        //        else
        //        {
        //            rsp.status = await _menuServicio.EliminarPermiso(idRol, idMenu);
        //        }
        //        rsp.value = rsp.status;
        //    }
        //    catch (Exception ex)
        //    {
        //        rsp.status = false;
        //        rsp.msg = ex.Message;
        //    }
        //    return Ok(rsp);
        //}

        [Authorize]
        [HttpGet]
        [Route("ObtenerMenuRolesPorRol")]
        public async Task<IActionResult> ObtenerMenuRolesPorRol(int idRol)
        {
            var rsp = new Response<List<MenuDTO>>();

            try
            {
                rsp.status = true;
                rsp.value = await _menuServicio.ObtenerMenuRolesPorRol(idRol);
            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }

        //[HttpPost]
        //[Route("ActualizarPermisos")]
        //public async Task<IActionResult> ActualizarPermisos(int idRol, [FromBody] List<int> menuIds)
        //{
        //    var rsp = new Response<bool>();

        //    try
        //    {
        //        // Actualizar permisos en la capa de servicio
        //        foreach (var menuId in menuIds)
        //        {
        //            bool permisoAsignado = await _menuServicio.AgregarPermiso(idRol, menuId);
        //            if (!permisoAsignado)
        //            {
        //                bool permisoEliminado = await _menuServicio.EliminarPermiso(idRol, menuId);
        //                if (!permisoEliminado)
        //                {
        //                    // Manejar el caso en que ni el permiso fue asignado ni eliminado correctamente
        //                    rsp.status = false;
        //                    rsp.msg = "Error al actualizar permisos para el menú con ID: " + menuId;
        //                    return Ok(rsp);
        //                }
        //            }
        //        }
        //        rsp.status = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        rsp.status = false;
        //        rsp.msg = ex.Message;
        //    }

        //    return Ok(rsp);
        //}


    }
}

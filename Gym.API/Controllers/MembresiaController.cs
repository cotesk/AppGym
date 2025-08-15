using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


using Gym.BLL.Servicios.Contrato;
using Gym.DTO;
using Gym.Api.Utilidad;
using Microsoft.EntityFrameworkCore;
using Gym.Model;
using Gym.BLL.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using OfficeOpenXml;
using System.IO;
using System.Text.RegularExpressions;



namespace Gym.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class MembresiaController : Controller
    {



        private readonly IMembresiaService _membresiaServicio;
    
        private readonly DbgymContext _context;

        public MembresiaController(IMembresiaService categoriaServicio, DbgymContext context)
        {
            _membresiaServicio = categoriaServicio;
          
            _context = context;
        }

        //[Authorize]
        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> Lista()
        {
          
            var rsp = new Response<List<MembresiaDTO>>();

            try
            {
                rsp.status = true;
                rsp.value = await _membresiaServicio.Lista();


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
        [Route("Guardar")]
        public async Task<IActionResult> Guardar([FromBody] MembresiaDTO membresia)
        {

            var rsp = new Response<MembresiaDTO>();
            try
            {

                var nombreExistente = await _context.Membresia.AnyAsync(p => p.Nombre == membresia.Nombre);

                if (nombreExistente)
                {
                    rsp.status = false;
                    rsp.msg = "El nombre de la membresia ya existe.";
                    return Ok(rsp);
                }



                rsp.status = true;
                rsp.value = await _membresiaServicio.Crear(membresia);


            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }
            return Ok(rsp);


        }
        [Authorize]
        [HttpPut]
        [Route("Editar")]
        public async Task<IActionResult> Editar([FromBody] MembresiaDTO categoria)
        {

            var rsp = new Response<bool>();

            try
            {

              

                rsp.status = true;
                rsp.value = await _membresiaServicio.Editar(categoria);


            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }
            return Ok(rsp);


        }
        [Authorize]
        [HttpDelete]
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {

            var rsp = new Response<bool>();

            try
            {
                rsp.status = true;
                rsp.value = await _membresiaServicio.Eliminar(id);


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
        [Route("Asignar")]
        public async Task<IActionResult> Asignar([FromBody] AsignacionesMembresiaDTO asignacion)
        {
            var rsp = new Response<bool>();

            try
            {
                // Validar que el usuario y la membresía existan
                var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == asignacion.IdUsuario);
                var membresiaExiste = await _context.Membresia.AnyAsync(m => m.IdMembresia == asignacion.IdMembresia);

                if (!usuarioExiste || !membresiaExiste)
                {
                    rsp.status = false;
                    rsp.msg = "Usuario o membresía no encontrada.";
                    return Ok(rsp);
                }

                rsp.status = true;
                rsp.value = await _membresiaServicio.AsignarMembresia(asignacion);
                rsp.msg = "Membresia Asignada correctamente";
            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }

        //[Authorize]
        [HttpGet]
        [Route("Asignaciones/Lista")]
        public async Task<IActionResult> ListarAsignaciones()
        {
            var rsp = new Response<List<AsignacionesMembresiaDTO>>();

            try
            {
                // Consulta las asignaciones con detalles relacionados de Usuario y Membresía
                var asignaciones = await _context.AsignacionesMembresia
                    .OrderByDescending(a => a.FechaVencimiento)
                    .Include(a => a.IdUsuarioNavigation) // Carga los detalles del usuario relacionado
                    .Include(a => a.IdMembresiaNavigation) // Carga los detalles de la membresía relacionada
                    .Select(a => new AsignacionesMembresiaDTO
                    {
                        AsignacionId = a.AsignacionId,
                        IdUsuario = a.IdUsuario,
                        NombreUsuario = a.IdUsuarioNavigation != null ? a.IdUsuarioNavigation.NombreCompleto : null, // Nombre del usuario
                        IdMembresia = a.IdMembresia,
                        NombreMembresia = a.IdMembresiaNavigation != null ? a.IdMembresiaNavigation.Nombre : null, // Nombre de la membresía
                        ImagenUrl = a.IdUsuarioNavigation != null ? a.IdUsuarioNavigation.ImagenUrl : null,
                        FechaVencimiento = a.FechaVencimiento.HasValue ? a.FechaVencimiento.Value.ToString("dd/MM/yyyy hh:mm tt") : null,
                        Estado = a.Estado
                    })
                    .ToListAsync();

                rsp.status = true;
                rsp.value = asignaciones;
            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }







        [HttpGet("GetMembresiaByUsuario/{idUsuario}")]
        public async Task<IActionResult> GetMembresiaByUsuario(int idUsuario)
        {
            var asignacion = await _context.AsignacionesMembresia
                .Include(a => a.IdMembresiaNavigation) // Usa la propiedad de navegación correcta
                .Where(a => a.IdUsuario == idUsuario)
                .Select(a => new
                {
                    a.AsignacionId,
                    a.FechaAsignacion,
                    a.FechaVencimiento,
                    a.Estado,
                    Membresia = new
                    {
                        a.IdMembresiaNavigation.IdMembresia,
                        a.IdMembresiaNavigation.Nombre,
                        a.IdMembresiaNavigation.Descripcion,
                        a.IdMembresiaNavigation.DuracionDias,
                        a.IdMembresiaNavigation.Precio,
                        a.IdMembresiaNavigation.FechaRegistro,
                        a.IdMembresiaNavigation.EsActivo
                    }
                })
                .FirstOrDefaultAsync();

            if (asignacion == null)
            {
                // Devuelve null para msg sin problemas de nullability
                return Ok(new { status = false, value = new List<object>(), msg  = "No se encontró una asignación para el usuario especificado." });
            }

            return Ok(new { status = true, value = new[] { asignacion }, msg = "Se encontró una asignación para el usuario especificado." });
        }



        //[HttpPost]
        //[Route("ActualizarEstadoAsignaciones")]
        //public async Task<IActionResult> ActualizarEstadoAsignaciones()
        //{
        //    var rsp = new Response<bool>();

        //    try
        //    {
        //        // Obtener la fecha actual en la zona horaria de Colombia
        //        var zonaHorariaColombia = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        //        var fechaHoraColombia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHorariaColombia);

        //        // Obtener todas las asignaciones cuya FechaVencimiento sea menor a la fecha actual
        //        var asignacionesPendientes = await _context.AsignacionesMembresia
        //            .Where(am => am.FechaVencimiento < fechaHoraColombia && am.Estado != "Pendiente")  // Asegurarse de que no estén ya en "Pendiente"
        //            .ToListAsync();

        //        // Actualizar el estado de cada asignación a "Pendiente"
        //        foreach (var asignacion in asignacionesPendientes)
        //        {
        //            asignacion.Estado = "Pendiente";  // Cambiar el estado

        //        }

        //        // Guardar los cambios en la base de datos
        //        _context.AsignacionesMembresia.UpdateRange(asignacionesPendientes);
        //        await _context.SaveChangesAsync();

        //        rsp.status = true;
        //        rsp.value = true;
        //        rsp.msg = "Se han actualizado los estados de las asignaciones correctamente.";
        //    }
        //    catch (Exception ex)
        //    {
        //        rsp.status = false;
        //        rsp.msg = ex.Message;
        //    }

        //    return Ok(rsp);
        //}



        [HttpGet]
        [Route("FechasVencidas")]
        public async Task<IActionResult> FechasVencidas()
        {
            var rsp = new Response<List<AsignacionesMembresiaDTO>>();

            try
            {
                // Obtener la fecha actual en la zona horaria de Colombia
                var zonaHorariaColombia = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                var fechaHoraColombia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHorariaColombia);

                // Obtener las asignaciones que están vencidas y no están en estado "Pendiente"
                var asignacionesVencidas = await _context.AsignacionesMembresia
                    .Include(a => a.IdUsuarioNavigation)  // Carga los detalles del usuario relacionado
                    .Include(a => a.IdMembresiaNavigation)  // Carga los detalles de la membresía relacionada
                    .Where(am => am.FechaVencimiento < fechaHoraColombia && am.Estado != "Pendiente")
                    .ToListAsync();

                // Obtener las asignaciones que ya están en estado "Pendiente"
                var asignacionesPendientes = await _context.AsignacionesMembresia
                    .Include(a => a.IdUsuarioNavigation)  // Carga los detalles del usuario relacionado
                    .Include(a => a.IdMembresiaNavigation)  // Carga los detalles de la membresía relacionada
                    .Where(am => am.Estado == "Pendiente")
                    .ToListAsync();

                // Actualizar el estado de las asignaciones vencidas a "Pendiente"
                foreach (var asignacion in asignacionesVencidas)
                {
                    asignacion.Estado = "Pendiente";  // Cambiar el estado
                }

                // Guardar los cambios de las asignaciones vencidas
                _context.AsignacionesMembresia.UpdateRange(asignacionesVencidas);
                await _context.SaveChangesAsync();

                // Combinar las asignaciones vencidas (ahora con estado Pendiente) y las que ya están en Pendiente
                var todasAsignacionesPendientes = asignacionesVencidas.Concat(asignacionesPendientes)
                    .OrderByDescending(a => a.FechaVencimiento)  // Ordenar de mayor a menor por la fecha de vencimiento
                    .ToList();

                // Convertir las asignaciones a DTO
                var asignacionesDTO = todasAsignacionesPendientes.Select(a => new AsignacionesMembresiaDTO
                {
                    AsignacionId = a.AsignacionId,
                    IdUsuario = a.IdUsuario,
                    NombreUsuario = a.IdUsuarioNavigation != null ? a.IdUsuarioNavigation.NombreCompleto : null,
                    ImagenUrl = a.IdUsuarioNavigation != null ? a.IdUsuarioNavigation.ImagenUrl : null,
                    IdMembresia = a.IdMembresia,
                    NombreMembresia = a.IdMembresiaNavigation != null ? a.IdMembresiaNavigation.Nombre : null,
                    FechaVencimiento = a.FechaVencimiento.HasValue ? a.FechaVencimiento.Value.ToString("dd/MM/yyyy hh:mm tt") : null,
                    Estado = a.Estado
                }).ToList();

                rsp.status = true;
                rsp.value = asignacionesDTO;
            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }





        [HttpGet]
        [Route("FechasVencidasUsuarios")]
        public async Task<IActionResult> FechasVencidasUsuarios()
        {
            var rsp = new Response<List<AsignacionesMembresiaDTO>>();

            try
            {
                // Obtener la fecha actual en la zona horaria de Colombia
                var zonaHorariaColombia = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                var fechaHoraColombia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHorariaColombia);

                // Obtener las asignaciones que están vencidas y no están en estado "Pendiente"
                var asignacionesVencidas = await _context.AsignacionesMembresia
                    .Include(a => a.IdUsuarioNavigation)  // Carga los detalles del usuario relacionado
                    .Include(a => a.IdMembresiaNavigation)  // Carga los detalles de la membresía relacionada
                    .Where(am => am.FechaVencimiento < fechaHoraColombia && am.Estado != "Pendiente")
                    .ToListAsync();

                // Obtener las asignaciones que ya están en estado "Pendiente"
                var asignacionesPendientes = await _context.AsignacionesMembresia
                    .Include(a => a.IdUsuarioNavigation)  // Carga los detalles del usuario relacionado
                    .Include(a => a.IdMembresiaNavigation)  // Carga los detalles de la membresía relacionada
                    .Where(am => am.Estado == "Pendiente")
                    .ToListAsync();

                // Actualizar el estado de las asignaciones vencidas a "Pendiente"
                foreach (var asignacion in asignacionesVencidas)
                {
                    asignacion.Estado = "Pendiente";  // Cambiar el estado
                }

                // Guardar los cambios de las asignaciones vencidas
                //_context.AsignacionesMembresia.UpdateRange(asignacionesVencidas);
                //await _context.SaveChangesAsync();

                // Combinar las asignaciones vencidas (ahora con estado Pendiente) y las que ya están en Pendiente
                var todasAsignacionesPendientes = asignacionesVencidas.Concat(asignacionesPendientes)
                    .OrderByDescending(a => a.FechaVencimiento)  // Ordenar de mayor a menor por la fecha de vencimiento
                    .ToList();

                // Convertir las asignaciones a DTO
                var asignacionesDTO = todasAsignacionesPendientes.Select(a => new AsignacionesMembresiaDTO
                {
                    AsignacionId = a.AsignacionId,
                    IdUsuario = a.IdUsuario,
                    NombreUsuario = a.IdUsuarioNavigation != null ? a.IdUsuarioNavigation.NombreCompleto : null,
                    IdMembresia = a.IdMembresia,
                    TelefonoUsuario = a.IdUsuarioNavigation != null ? a.IdUsuarioNavigation.Telefono : null,
                    NombreMembresia = a.IdMembresiaNavigation != null ? a.IdMembresiaNavigation.Nombre : null,
                    FechaVencimiento = a.FechaVencimiento.HasValue ? a.FechaVencimiento.Value.ToString("dd/MM/yyyy hh:mm tt") : null,
                    Estado = a.Estado
                }).ToList();

                rsp.status = true;
                rsp.value = asignacionesDTO;
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

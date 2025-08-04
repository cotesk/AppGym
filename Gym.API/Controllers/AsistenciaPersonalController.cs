using Microsoft.AspNetCore.Mvc;
using Gym.BLL.Servicios.Contrato;
using Gym.DTO;
using Gym.Api.Utilidad;
using Microsoft.EntityFrameworkCore;
using Gym.Model;
using System.Globalization;

namespace Gym.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsistenciaPersonalController : ControllerBase
    {
        private readonly IAsistenciaPersonalService _asistenciaService;
        private readonly DbgymContext _context;

        public AsistenciaPersonalController(IAsistenciaPersonalService asistenciaService, DbgymContext context)
        {
            _asistenciaService = asistenciaService;
            _context = context;
        }

        [HttpPost]
        [Route("Registrar")]
        public async Task<IActionResult> RegistrarAsistencia(int idUsuario)
        {
            var rsp = new Response<bool>();

            try
            {
                // Obtener la asignación del usuario con la membresía relacionada
                var usuario = await _context.AsignacionesMembresia
                    .Include(u => u.IdMembresiaNavigation) // Carga la membresía
                    .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

                if (usuario == null || usuario.IdMembresiaNavigation == null)
                {
                    rsp.status = false;
                    rsp.msg = "El usuario no tiene una membresía activa.";
                    return BadRequest(rsp);
                }

                var membresia = usuario.IdMembresiaNavigation;
                var dias = membresia.DuracionDias;
                string tipoMembresia;

                // Determinar el tipo de membresía según los días de duración
                if (dias < 2)
                {
                    tipoMembresia = "diaria";
                }
                else if (dias >= 28 && dias <= 33)
                {
                    tipoMembresia = "mensual";
                }
                else
                {
                    tipoMembresia = "anual";
                }

                // Obtener la hora actual en la zona horaria de Colombia (UTC-5)
                TimeZoneInfo colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                DateTime fechaActual = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, colombiaTimeZone);



                bool pagoRealizado;

                // Ajustar el estado de pago según el tipo de membresía
                if (tipoMembresia == "diaria")
                {
                    // Pagos diarios no se marcan como realizados automáticamente
                    pagoRealizado = false;
                }
                else if (tipoMembresia == "mensual" || tipoMembresia == "anual")
                {
                    if (usuario.Estado== "Activado")
                    {
                        var fechaFin = usuario.FechaVencimiento;
                        pagoRealizado = fechaActual <= fechaFin;

                    }
                    else
                    {
                        pagoRealizado = false;
                    }
                 
                }
                else
                {
                    pagoRealizado = false; // Fallback en caso de inconsistencias
                }

                // Verificar si ya existe una asistencia para el día actual
                var asistenciaExistente = await _context.AsistenciaPersonals
                              .FirstOrDefaultAsync(a => a.IdUsuario == idUsuario &&
                               a.FechaAsistencia.HasValue && // Verifica que la fecha no sea nula
                               a.FechaAsistencia.Value.Date == fechaActual.Date);


                if (asistenciaExistente != null)
                {
                    rsp.status = false;
                    rsp.msg = "Ya se ha registrado una asistencia para el día de hoy.";
                    return BadRequest(rsp);
                }

                // Registrar la nueva asistencia
                var nuevaAsistencia = new AsistenciaPersonal
                {
                    IdUsuario = idUsuario,
                    FechaAsistencia = fechaActual,
                    PagoRealizado = pagoRealizado
                };

                _context.AsistenciaPersonals.Add(nuevaAsistencia);
                await _context.SaveChangesAsync();

                rsp.status = true;
                rsp.value = true;
                rsp.msg = "Asistencia registrada";
            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }


        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> ListarAsistencias()
        {
            var rsp = new Response<List<AsistenciaPersonalDTO>>();
            try
            {

                rsp.status = true;
                rsp.value = await _asistenciaService.ListarAsistencias();

            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }


        [Route("ListaPaginada")]
        [HttpGet]
        public ActionResult<IEnumerable<AsistenciaPersonalDTO>> GetUsuarios(int page = 1, int pageSize = 5, string searchTerm = null)
        {
            IQueryable<AsistenciaPersonalDTO> query = _context.AsistenciaPersonals
                .Select(u => new AsistenciaPersonalDTO
                {
                    IdUsuario = u.IdUsuario,
                    AsistenciaId = u.AsistenciaId,
                    NombreUsuario = u.IdUsuarioNavigation.NombreCompleto,
                    FechaAsistencia = ((DateTime)u.FechaAsistencia).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture),
                    PagoRealizado = u.PagoRealizado,
                });

            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Filtrar por PagoRealizado si es un booleano
                if (bool.TryParse(searchTerm, out bool pagoRealizado))
                {
                    query = query.Where(c => c.PagoRealizado == pagoRealizado);
                }
                // Filtrar por AsistenciaId o NombreUsuario si corresponde
                else if (int.TryParse(searchTerm, out int asistenciaId))
                {
                    query = query.Where(c => c.AsistenciaId == asistenciaId);
                }
                else if (int.TryParse(searchTerm, out int IdUsuario))
                {
                    query = query.Where(c => c.IdUsuario == IdUsuario);
                }
                // Filtrar por FechaAsistencia si es una fecha válida
                else if (DateTime.TryParseExact(searchTerm, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaBusqueda))
                {
                    // Usamos AsEnumerable() para hacer la comparación en memoria (evita problemas con la traducción a SQL)
                    query = query.AsEnumerable()
                                 .Where(c => DateTime.ParseExact(c.FechaAsistencia, "dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None).Date == fechaBusqueda.Date)
                                 .AsQueryable();
                }
                else
                {
                    query = query.Where(c => c.NombreUsuario.Contains(searchTerm));
                }
            }

            query = query.OrderByDescending(c => c.AsistenciaId);


            var totalUsuarios = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalUsuarios / pageSize);

            var usuariosPaginados = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new { data = usuariosPaginados, total = totalUsuarios, totalPages });
        }



        [HttpGet]
        [Route("Consultar/{idUsuario:int}")]
        public async Task<IActionResult> ConsultarAsistenciasPorUsuario(int idUsuario)
        {
            var rsp = new Response<List<AsistenciaPersonalDTO>>();

            try
            {
                rsp.status = true;
                rsp.value = await _asistenciaService.ConsultarAsistenciasPorUsuario(idUsuario);
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

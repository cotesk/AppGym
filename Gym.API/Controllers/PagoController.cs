using Gym.BLL.Servicios;
using Gym.BLL.Servicios.Contrato;
using Gym.DTO;
using Gym.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Gym.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagosController : ControllerBase
    {

        private readonly DbgymContext _context;
        private readonly IPagoService _pagoService;

        public PagosController(DbgymContext context, IPagoService pagoService)
        {
            _context = context;
            _pagoService = pagoService;
        }


        // Registrar un nuevo pago
        [HttpPost("Registrar")]
        public async Task<IActionResult> RegistrarPago([FromBody] PagoDTO pagoDto)
        {
            if (pagoDto == null)
            {
                return BadRequest("Los datos del pago son inválidos.");
            }

            try
            {
                var pagoRegistrado = await _pagoService.RegistrarPago(pagoDto);

                return Ok(new
                {
                    status = true,
                    message = "Pago registrado y membresía activada exitosamente",
                    data = pagoRegistrado
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }


        [HttpPost("RegistrarCalendario")]
        public async Task<IActionResult> RegistrarPagoCalendario([FromBody] PagoDTO pagoDto)
        {
            if (pagoDto == null)
            {
                return BadRequest("Los datos del pago son inválidos.");
            }

            try
            {
                var pagoRegistrado = await _pagoService.RegistrarPagoCalendario(pagoDto);

                return Ok(new
                {
                    status = true,
                    message = "Pago registrado y membresía activada exitosamente",
                    data = pagoRegistrado
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }


        //[HttpPost("RegistrarCalendario")]
        //public async Task<IActionResult> RegistrarPagoCalendario([FromBody] PagoDTO pagoDto)
        //{
        //    if (pagoDto == null)
        //    {
        //        return BadRequest("Los datos del pago son inválidos.");
        //    }

        //    try
        //    {
        //        // Verificar si el pago ya ha sido registrado
        //        var asignacion = await _context.AsignacionesMembresia
        //            .Include(am => am.IdMembresiaNavigation)
        //              .Where(am => am.IdUsuario == pagoDto.IdUsuario)
        //               .OrderByDescending(am => am.FechaAsignacion) // Ordena por fecha, la más reciente primero
        //                 .FirstOrDefaultAsync();

        //        var asistencia = await _context.AsistenciaPersonals
        //       .Where(am => am.AsistenciaId == pagoDto.IdAsistencia && am.PagoRealizado ==false)
        //        .OrderByDescending(am => am.FechaAsistencia) // Ordena por fecha, la más reciente primero
        //          .FirstOrDefaultAsync();


        //        if (asignacion == null)
        //        {
        //            return BadRequest("El usuario no tiene asignaciones de membresía no pagadas.");
        //        }


        //        var zonaHorariaColombia = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        //        var fechaHoraColombia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHorariaColombia);

        //        var fechaColombiana = fechaHoraColombia;

        //        var dias = asignacion.IdMembresiaNavigation.DuracionDias;

        //        if (dias < 2)
        //        {
        //            pagoDto.TipoPago = "diario";
        //        }
        //        else if (dias >= 28 && dias <= 33)
        //        {
        //            pagoDto.TipoPago = "mensual";
        //        }
        //        else
        //        {
        //            pagoDto.TipoPago = "anual";
        //        }




        //        // Lógica para registrar el pago
        //        var pago = new Pago
        //        {
        //            IdUsuario = pagoDto.IdUsuario,
        //            Monto = asignacion.IdMembresiaNavigation.Precio,
        //            MetodoPago = "Efectivo",
        //            TipoPago = pagoDto.TipoPago,
        //            FechaPago = fechaColombiana,  // Usar la fecha proporcionada
        //            Observaciones = pagoDto.Observaciones
        //        };

        //        _context.Pagos.Add(pago);
        //        await _context.SaveChangesAsync();


        //        asistencia.PagoRealizado = true;

        //        _context.AsistenciaPersonals.Update(asistencia);
        //        await _context.SaveChangesAsync();

        //        // Actualizar el estado de la asignación de la membresía
        //        asignacion.Estado = "Pagado";  // Cambiar el estado a 'Pagado'
        //        _context.AsignacionesMembresia.Update(asignacion);
        //        await _context.SaveChangesAsync();

        //        // Registrar el pago en el historial
        //        var historialPago = new HistorialPago
        //        {
        //            PagoId = pago.PagoId,
        //            IdUsuario = pago.IdUsuario,
        //            FechaPago = pago.FechaPago,
        //            Monto = pago.Monto,
        //            TipoPago = pago.TipoPago
        //        };

        //        _context.HistorialPagos.Add(historialPago);
        //        await _context.SaveChangesAsync();

        //        var cajaActiva = await _context.Cajas
        //    .FirstOrDefaultAsync(c => c.Estado == "Abierto");

        //        if (cajaActiva != null)
        //        {
        //            if (pago.MetodoPago == "Efectivo")
        //            {
        //                cajaActiva.Ingresos += pago.Monto;
        //            }
        //            else
        //            {
        //                cajaActiva.Transacciones += pago.Monto;
        //            }

        //            _context.Cajas.Update(cajaActiva);
        //            await _context.SaveChangesAsync();
        //        }
        //        else
        //        {
        //            throw new Exception("No hay una caja activa disponible.");
        //        }

            

        //        return Ok(new
        //        {
        //            status = true,
        //            message = "Pago registrado y membresía activada exitosamente",
        //            data = pago
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { status = false, message = ex.Message });
        //    }
        //}


        // Obtener los pagos de un usuario
        [HttpGet("ObtenerPagos/{idUsuario}")]
        public async Task<IActionResult> ObtenerPagos(int idUsuario)
        {
            try
            {
                var pagos = await _pagoService.ObtenerPagosPorUsuario(idUsuario);

                if (pagos == null || !pagos.Any())
                {
                    return NotFound(new { status = false, message = "No se encontraron pagos para este usuario." });
                }

                return Ok(new { status = true, data = pagos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }


        [HttpGet("ObtenerTodosLosPagos")]
        public async Task<IActionResult> ObtenerTodosLosPagos()
        {
            try
            {
                var pagos = await _pagoService.ObtenerTodosLosPagos();

                if (pagos == null || !pagos.Any())
                {
                    return NotFound(new { status = false, message = "No se encontraron pagos." });
                }

                return Ok(new { status = true, data = pagos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }



        [Route("ListaPaginada")]
        [HttpGet]
        public ActionResult<IEnumerable<HistorialPagoDTO>> GetUsuarios(int page = 1, int pageSize = 5, string searchTerm = null)
        {

            // Verificamos si searchTerm es un número entero y le agregamos .00 si es así
            //if (!string.IsNullOrEmpty(searchTerm) && int.TryParse(searchTerm, out int monto))
            //{
            //    searchTerm = monto.ToString("F2", CultureInfo.InvariantCulture); // Convertimos a string con .00
            //}

            IQueryable<HistorialPagoDTO> query = _context.HistorialPagos
                .Select(u => new HistorialPagoDTO
                {
                    IdUsuario = u.IdUsuario,
                    HistorialPagoId = u.HistorialPagoId,
                    PagoId = u.PagoId,
                    MontoTexto = u.Monto.ToString(),
                    NombreUsuario = u.IdUsuarioNavigation.NombreCompleto,
                    ImagenUrl = u.IdUsuarioNavigation != null ? u.IdUsuarioNavigation.ImagenUrl : null,

                    FechaPago = ((DateTime)u.FechaPago).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture),
                    TipoPago = u.TipoPago,
                });

            if (!string.IsNullOrEmpty(searchTerm))
            {
               if (int.TryParse(searchTerm, out int HistorialPagoId))
                {
                    query = query.Where(c => c.HistorialPagoId == HistorialPagoId);
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
                                 .Where(c => DateTime.ParseExact(c.FechaPago, "dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None).Date == fechaBusqueda.Date)
                                 .AsQueryable();
                }
                else
                {
                    //query = query.Where(c => c.NombreUsuario.Contains(searchTerm));

                    query = query.Where(c =>
                   c.NombreUsuario.Contains(searchTerm) ||
                   c.MontoTexto.Contains(searchTerm) ||
                   c.TipoPago.Contains(searchTerm)
               );
                }
            }

            query = query.OrderByDescending(c => c.HistorialPagoId);



            var totalUsuarios = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalUsuarios / pageSize);

            var usuariosPaginados = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new { data = usuariosPaginados, total = totalUsuarios, totalPages });
        }




        [HttpGet("BuscarEstadoCalendario/{idUsuario}")]
        public async Task<IActionResult> GetAsignacionByIdAsistencia(int idUsuario)
        {

            try
            {
                var asignacion = _context.AsignacionesMembresia
                                    .FirstOrDefault(a => a.IdUsuario == idUsuario);

                if (asignacion == null)
                {
                    return NotFound(new { status = false, message = "No se encontraron estado para este usuario." });
                }
                int idMem = asignacion.IdMembresia ?? 0;

                var membresia = _context.Membresia
                                   .FirstOrDefault(a => a.IdMembresia == idMem );

                if (membresia == null)
                {
                    return NotFound(new { status = false, message = "No se encontraron estado para este usuario." });
                }



                return Ok(new { status = true, data = membresia.Nombre });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }

        }

      


    }


}
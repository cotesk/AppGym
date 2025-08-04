using Gym.DTO;
using Gym.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gym.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntrenadorClienteController : ControllerBase
    {
        private readonly DbgymContext _context;

        public EntrenadorClienteController(DbgymContext context)
        {
            _context = context;
        }

        // POST: api/EntrenadorCliente
        [HttpPost("asignar-entrenador")]
        public async Task<IActionResult> AsignarEntrenadorACliente(AsignacionEntrenadorClienteDTO asignacionDTO)
        {
            // Verificar si el cliente existe en la base de datos
            var clienteExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == asignacionDTO.ClienteID && u.IdRolNavigation.Nombre == "Clientes");

            if (clienteExistente == null)
            {
                return NotFound(new { mensaje = "Cliente no encontrado o no es un cliente." });
            }

            // Verificar si el entrenador existe en la base de datos
            var entrenadorExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == asignacionDTO.EntrenadorID && u.IdRolNavigation.Nombre == "Entrenador");

            if (entrenadorExistente == null)
            {
                return NotFound(new { mensaje = "Entrenador no encontrado o no es un entrenador." });
            }

            // Verificar si el cliente ya tiene un entrenador asignado
            var asignacionClienteExistente = await _context.EntrenadorClientes
                .FirstOrDefaultAsync(ac => ac.ClienteId == asignacionDTO.ClienteID);

            if (asignacionClienteExistente != null)
            {
                return BadRequest(new { mensaje = "El cliente ya tiene un entrenador asignado." });
            }

            // Obtener la hora actual en la zona horaria de Colombia
            var zonaHorariaColombia = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            var fechaHoraColombia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHorariaColombia);

            // Crear la nueva asignación
            var nuevaAsignacion = new EntrenadorCliente
            {
                ClienteId = asignacionDTO.ClienteID,
                EntrenadorId = asignacionDTO.EntrenadorID,
                FechaAsignacion = fechaHoraColombia
            };

            // Agregar la nueva asignación a la base de datos
            _context.EntrenadorClientes.Add(nuevaAsignacion);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Entrenador asignado exitosamente", entrenadorId = nuevaAsignacion.EntrenadorId });
        }



        //[HttpPost("asignar-entrenador")]
        //public async Task<IActionResult> AsignarEntrenador(AsignacionEntrenadorClienteDTO asignarEntrenadorDTO)
        //{
        //    // Verificar si el entrenador existe
        //    var entrenador = await _context.Entrenadores.FirstOrDefaultAsync(e => e.EntrenadorId == asignarEntrenadorDTO.EntrenadorID);
        //    if (entrenador == null)
        //    {
        //        return NotFound(new { mensaje = "Entrenador no encontrado." });
        //    }

        //    // Verificar si el cliente existe
        //    var cliente = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == asignarEntrenadorDTO.ClienteID);
        //    if (cliente == null)
        //    {
        //        return NotFound(new { mensaje = "Cliente no encontrado." });
        //    }


        //    var zonaHorariaColombia = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        //    var fechaHoraColombia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHorariaColombia);

        //    // Crear la asignación
        //    var nuevaAsignacion = new EntrenadorCliente
        //    {
        //        ClienteId = asignarEntrenadorDTO.ClienteID,
        //        EntrenadorId = asignarEntrenadorDTO.EntrenadorID,
        //        FechaAsignacion = fechaHoraColombia
        //    };

        //    // Agregar al contexto y guardar cambios
        //    _context.EntrenadorClientes.Add(nuevaAsignacion);
        //    await _context.SaveChangesAsync();

        //    return Ok(new { mensaje = "Entrenador asignado correctamente.", asignacionID = nuevaAsignacion.AsignacionId });
        //}


        //[HttpGet("{entrenadorId}/clientes")]
        //public async Task<IActionResult> GetClientesPorEntrenador(int entrenadorId)
        //{
        //    var clientes = await _context.EntrenadorClientes
        //        .Where(ec => ec.EntrenadorId == entrenadorId)
        //        .Include(ec => ec.Cliente)
        //        .Select(ec => new
        //        {
        //            ClienteID = ec.ClienteId,
        //            NombreCliente = ec.Cliente.NombreCompleto,
        //            FechaAsignacion = ec.FechaAsignacion,
        //            FechaFinAsignacion = ec.FechaFinAsignacion
        //        })
        //        .ToListAsync();

        //    return Ok(clientes);
        //}

        //[HttpGet("{clienteId}/entrenador")]
        //public async Task<IActionResult> GetEntrenadorPorCliente(int clienteId)
        //{

        //    var clientes = await _context.EntrenadorClientes
        //                   .Where(ec => ec.ClienteId == clienteId)
        //                   .Include(ec => ec.Entrenador)
        //                   .Select(ec => new
        //                   {
        //                       ClienteID = ec.ClienteId,
        //                       NombreCliente = ec.Entrenador.NombreCompleto,
        //                       FechaAsignacion = ec.FechaAsignacion,
        //                       FechaFinAsignacion = ec.FechaFinAsignacion
        //                   })
        //                   .ToListAsync();

        //    return Ok(clientes);
        //}





        //[HttpGet("asignaciones")]
        //public async Task<IActionResult> GetTodasLasAsignaciones()
        //{
        //    var asignaciones = await _context.EntrenadorClientes
        //        .Include(ec => ec.Cliente) // Cliente relacionado
        //        .Include(ec => ec.Entrenador) // Entrenador relacionado
        //        .Select(ec => new
        //        {
        //            AsignacionID = ec.AsignacionId,
        //            ClienteID = ec.ClienteId,
        //            NombreCliente = ec.Cliente.NombreCompleto, // Accede a Cliente
        //            EntrenadorID = ec.EntrenadorId,
        //            NombreEntrenador = ec.Entrenador.NombreCompleto, // Accede a Entrenador
        //            FechaAsignacion = ec.FechaAsignacion,
        //            FechaFinAsignacion = ec.FechaFinAsignacion
        //        })
        //        .ToListAsync();

        //    return Ok(asignaciones);
        //}




        // GET: api/EntrenadorCliente/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<EntrenadorClienteDTO>> GetAsignacion(int id)
        {
            var asignacion = await _context.EntrenadorClientes
                .Include(ec => ec.Cliente)  // Incluir el cliente relacionado
                .Include(ec => ec.Entrenador) // Incluir el entrenador relacionado
                .FirstOrDefaultAsync(ec => ec.AsignacionId == id);

            if (asignacion == null)
            {
                return NotFound();
            }

            // Mapear el modelo a DTO
            var asignacionDTO = new EntrenadorClienteDTO
            {
                AsignacionId = asignacion.AsignacionId,
                ClienteId = asignacion.ClienteId,
                EntrenadorId = asignacion.EntrenadorId,
                NombreCliente = asignacion.Cliente.NombreCompleto,
                NombreEntrenador = asignacion.Entrenador.NombreCompleto,
                FechaAsignacion = asignacion.FechaAsignacion?.ToString("dd/MM/yyyy h:mm tt"),
                FechaFinAsignacion = asignacion.FechaFinAsignacion?.ToString("dd/MM/yyyy h:mm tt")
            };

            return Ok(asignacionDTO);
        }

        // GET: api/EntrenadorCliente/cliente/{clienteId}
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<EntrenadorClienteDTO>>> GetAsignacionesPorCliente(int clienteId)
        {
            var asignaciones = await _context.EntrenadorClientes
                .Include(ec => ec.Entrenador)  // Incluir los entrenadores asignados
                .Where(ec => ec.ClienteId == clienteId)
                .ToListAsync();

            if (asignaciones == null || !asignaciones.Any())
            {
                return NotFound(new { mensaje = "No hay asignaciones para este cliente." });
            }

            // Mapear las asignaciones a DTOs
            var asignacionesDTO = asignaciones.Select(asignacion => new EntrenadorClienteDTO
            {
                AsignacionId = asignacion.AsignacionId,
                ClienteId = asignacion.ClienteId,
                EntrenadorId = asignacion.EntrenadorId,

                NombreEntrenador = asignacion.Entrenador.NombreCompleto,
                FechaAsignacion = asignacion.FechaAsignacion?.ToString("dd/MM/yyyy h:mm tt"),
                FechaFinAsignacion = asignacion.FechaFinAsignacion?.ToString("dd/MM/yyyy h:mm tt")
            }).ToList();

            return Ok(asignacionesDTO);
        }

        // GET: api/EntrenadorCliente/entrenador/{entrenadorId}
        [HttpGet("entrenador/{entrenadorId}")]
        public async Task<ActionResult<IEnumerable<EntrenadorClienteDTO>>> GetAsignacionesPorEntrenador(int entrenadorId)
        {
            var asignaciones = await _context.EntrenadorClientes
                .Include(ec => ec.Cliente)  // Incluir los clientes asignados
                .Where(ec => ec.EntrenadorId == entrenadorId)
                .ToListAsync();

            if (asignaciones == null || !asignaciones.Any())
            {
                return NotFound(new { mensaje = "No hay asignaciones para este entrenador." });
            }

            // Mapear las asignaciones a DTOs
            var asignacionesDTO = asignaciones.Select(asignacion => new EntrenadorClienteDTO
            {
                AsignacionId = asignacion.AsignacionId,
                ClienteId = asignacion.ClienteId,
                EntrenadorId = asignacion.EntrenadorId,
                NombreCliente = asignacion.Cliente.NombreCompleto,

                FechaAsignacion = asignacion.FechaAsignacion?.ToString("dd/MM/yyyy h:mm tt"),
                FechaFinAsignacion = asignacion.FechaFinAsignacion?.ToString("dd/MM/yyyy h:mm tt")
            }).ToList();

            return Ok(asignacionesDTO);
        }

        // GET: api/EntrenadorCliente
        [HttpGet("traerTodo")]
        public async Task<ActionResult<IEnumerable<EntrenadorClienteDTO>>> GetTodasLasAsignaciones()
        {
            var asignaciones = await _context.EntrenadorClientes
                .Include(ec => ec.Cliente)  // Incluir el cliente relacionado
                .Include(ec => ec.Entrenador) // Incluir el entrenador relacionado
                .ToListAsync();

            if (asignaciones == null || !asignaciones.Any())
            {
                return NotFound(new { mensaje = "No hay asignaciones disponibles." });
            }

            // Mapear las asignaciones a DTOs
            var asignacionesDTO = asignaciones.Select(asignacion => new EntrenadorClienteDTO
            {
                AsignacionId = asignacion.AsignacionId,
                ClienteId = asignacion.ClienteId,
                EntrenadorId = asignacion.EntrenadorId,
                NombreCliente = asignacion.Cliente.NombreCompleto,
                NombreEntrenador = asignacion.Entrenador.NombreCompleto,
                FechaAsignacion = asignacion.FechaAsignacion?.ToString("dd/MM/yyyy h:mm tt"),
                FechaFinAsignacion = asignacion.FechaFinAsignacion?.ToString("dd/MM/yyyy h:mm tt")
            }).ToList();

            return Ok(asignacionesDTO);
        }



        // PUT: api/EntrenadorCliente/editar-asignacion/{id}
        [HttpPut("editar-asignacion/{id}")]
        public async Task<IActionResult> EditarAsignacionEntrenadorCliente(int id, [FromBody] AsignacionEntrenadorClienteDTO editarDTO)
        {
            // Verificar si la asignación existe
            var asignacionExistente = await _context.EntrenadorClientes
                .FirstOrDefaultAsync(ac => ac.AsignacionId == id);

            if (asignacionExistente == null)
            {
                return NotFound(new { mensaje = "Asignación no encontrada." });
            }

            // Verificar si el nuevo cliente existe en la base de datos
            var clienteExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == editarDTO.ClienteID && u.IdRolNavigation.Nombre == "Clientes");

            if (clienteExistente == null)
            {
                return NotFound(new { mensaje = "El nuevo cliente no existe o no tiene el rol adecuado." });
            }

            // Verificar si el nuevo entrenador existe en la base de datos
            var entrenadorExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == editarDTO.EntrenadorID && u.IdRolNavigation.Nombre == "Entrenador");

            if (entrenadorExistente == null)
            {
                return NotFound(new { mensaje = "El nuevo entrenador no existe o no tiene el rol adecuado." });
            }

            // Verificar si el cliente ya tiene un entrenador asignado
            var clienteAsignado = await _context.EntrenadorClientes
               .FirstOrDefaultAsync(ac => ac.ClienteId == editarDTO.ClienteID && ac.AsignacionId != id);

            if (clienteAsignado != null)
            {
                return BadRequest(new { mensaje = "El cliente ya está asignado a otro entrenador." });
            }


            // Verificar si ya existe una asignación idéntica (cliente y entrenador)
            var asignacionDuplicada = await _context.EntrenadorClientes
                .FirstOrDefaultAsync(ac => ac.ClienteId == editarDTO.ClienteID && ac.EntrenadorId == editarDTO.EntrenadorID);

            if (asignacionDuplicada != null && asignacionDuplicada.AsignacionId != id)
            {
                return BadRequest(new { mensaje = "Ya existe una asignación con este cliente y entrenador." });
            }

            // Obtener la hora actual en la zona horaria de Colombia
            var zonaHorariaColombia = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            var fechaHoraColombia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHorariaColombia);

            // Actualizar los datos de la asignación
            asignacionExistente.ClienteId = editarDTO.ClienteID;
            asignacionExistente.EntrenadorId = editarDTO.EntrenadorID;
            asignacionExistente.FechaAsignacion = fechaHoraColombia;

            // Guardar los cambios en la base de datos
            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { mensaje = "Asignación actualizada exitosamente." });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error al actualizar la asignación.", detalle = ex.Message });
            }
        }



        // DELETE: api/EntrenadorCliente/eliminar-asignacion/{id}
        [HttpDelete("eliminar-asignacion")]
        public async Task<IActionResult> EliminarAsignacion(int asignacionId)
        {
            // Buscar la asignación en la base de datos por su ID
            var asignacion = await _context.EntrenadorClientes.FirstOrDefaultAsync(ec => ec.AsignacionId == asignacionId);

            if (asignacion == null)
            {
                return NotFound(new { mensaje = "Asignación no encontrada." });
            }

            // Eliminar la asignación de la base de datos
            _context.EntrenadorClientes.Remove(asignacion);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Asignación eliminada exitosamente." });
        }




    }
}

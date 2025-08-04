using Gym.DTO;
using Gym.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gym.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntrenadoresController : ControllerBase
    {
        private readonly DbgymContext _context;

        public EntrenadoresController(DbgymContext context)
        {
            _context = context;
        }

        // POST: api/Entrenadores
        [HttpPost]
        public async Task<IActionResult> CrearEntrenador(CrearEntrenadorDTO crearEntrenadorDTO)
        {
            // Verificar si el usuario existe en la base de datos
            var usuarioExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == crearEntrenadorDTO.IdUsuario);

            if (usuarioExistente == null)
            {
                return NotFound(new { mensaje = "Usuario no encontrado." });
            }

            // Crear el nuevo entrenador
            var nuevoEntrenador = new Entrenadores
            {
                IdUsuario = crearEntrenadorDTO.IdUsuario,
                Especialidad = crearEntrenadorDTO.Especialidad
            };

            // Agregar el nuevo entrenador al contexto de la base de datos
            _context.Entrenadores.Add(nuevoEntrenador);
            await _context.SaveChangesAsync();

            // Retornar una respuesta con un mensaje personalizado
            return Ok(new { mensaje = "Entrenador creado", entrenadorId = nuevoEntrenador.EntrenadorId });
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<EntrenadoresDTO>> GetEntrenador(int id)
        {
            var entrenador = await _context.Entrenadores
                .Include(e => e.IdUsuarioNavigation)
                .FirstOrDefaultAsync(e => e.EntrenadorId == id);

            if (entrenador == null)
            {
                return NotFound();
            }

            // Mapear a DTO
            var entrenadorDTO = new EntrenadoresDTO
            {
                EntrenadorId = entrenador.EntrenadorId,
                IdUsuario = entrenador.IdUsuario,
                Especialidad = entrenador.Especialidad,
                UsuarioNombre = entrenador.IdUsuarioNavigation?.NombreCompleto // Aquí, puedes tomar solo lo que necesitas
            };

            return Ok(entrenadorDTO);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EntrenadoresDTO>>> GetEntrenadores()
        {
            var entrenadores = await _context.Entrenadores
                .Include(e => e.IdUsuarioNavigation)
                .ToListAsync();

            if (entrenadores == null || !entrenadores.Any())
            {
                return NotFound(new { mensaje = "No se encontraron entrenadores." });
            }

            // Mapear la lista de entrenadores a DTOs
            var entrenadoresDTO = entrenadores.Select(e => new EntrenadoresDTO
            {
                EntrenadorId = e.EntrenadorId,
                IdUsuario = e.IdUsuario,
                Especialidad = e.Especialidad,
                UsuarioNombre = e.IdUsuarioNavigation?.NombreCompleto
            }).ToList();

            return Ok(entrenadoresDTO);
        }



    }

}

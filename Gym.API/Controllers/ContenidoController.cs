using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Gym.BLL.Servicios.Contrato;

using Gym.Api.Utilidad;
using Gym.BLL.Servicios;
using static System.Net.Mime.MediaTypeNames;
using Firebase.Auth;
using Firebase.Storage;
using Gym.Model;

using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Gym.BLL.Servicios.Contrato;
using Gym.DTO;


namespace Gym.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class ContenidoController : Controller
    {

        private readonly IContenidoService _contenidoServicio;

        private readonly DbgymContext _context;

        public ContenidoController(IContenidoService contenidoServicio, DbgymContext context)
        {
            _contenidoServicio = contenidoServicio;

            _context = context;
        }



        //[Authorize]
        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> Lista()
        {
            var rsp = new Response<List<ContenidoDTO>>();

            try
            {
                rsp.status = true;
                var listaContenido = await _contenidoServicio.Lista();

                // Verificar si la lista de productos es nula antes de asignarla a rsp.value
                rsp.value = listaContenido ?? new List<ContenidoDTO>();
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
        public async Task<IActionResult> Editar([FromBody] ContenidoDTO contenido)
        {
            var rsp = new Response<bool>();

            try
            {


                rsp.status = true;
                rsp.value = await _contenidoServicio.Editar(contenido);
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
                rsp.value = await _contenidoServicio.Eliminar(id);


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
        [Route("ActualizarImagen/{id:int}")]
        public async Task<IActionResult> ActualizarImagen(int id, [FromForm] IFormFile imagen)
        {
            var rsp = new Response<string>();

            try
            {
                Console.WriteLine($"ID del producto en ActualizarImagen: {id}");

                // Verifica si la imagen se está recibiendo correctamente
                if (imagen == null)
                {
                    return BadRequest(new { Mensaje = "La imagen no se recibió correctamente." });
                }

                var contenido = await _context.Contenidos.FirstOrDefaultAsync(p => p.IdContenido == id);

                if (contenido == null)
                {
                    return NotFound(new { Mensaje = "Producto no encontrado" });
                }

                // Convierte la imagen a un arreglo de bytes
                using (var stream = new MemoryStream())
                {
                    await imagen.CopyToAsync(stream);
                    contenido.Imagenes = stream.ToArray();
                }

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();

                rsp.status = true;
                rsp.value = "Imagen actualizada correctamente";
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
        public async Task<IActionResult> Guardar([FromBody] ContenidoDTO contenido)
        {

            var rsp = new Response<ContenidoDTO>();
            try
            {

                bool nombreExistente;

                // Verifica si TipoComentarios está vacío o nulo
                if (string.IsNullOrEmpty(contenido.TipoComentarios))
                {
                    // Si TipoComentarios está vacío, se omite la condición en la consulta
                    nombreExistente = false; // O establece el valor según tus necesidades
                }
                else
                {
                    // Consulta solo si TipoComentarios no está vacío
                    nombreExistente = await _context.Contenidos.AnyAsync(p => p.TipoComentarios == contenido.TipoComentarios);
                }


                if (nombreExistente)
                {
                    rsp.status = false;
                    rsp.msg = "Ya existe un contenido con el mismo nombre";
                    return Ok(rsp);
                }

                rsp.status = true;
                rsp.value = await _contenidoServicio.Crear(contenido);


            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
                Console.WriteLine($"Error al guardar el producto: {ex}");
            }
            return Ok(rsp);
        }





    }
}

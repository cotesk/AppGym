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
                var contenido = await _context.Contenidos.FindAsync(id);
                // Eliminar la imagen asociada en Firebase Storage
                if (!string.IsNullOrEmpty(contenido.NombreImagen))
                    await EliminarDeStorage(contenido.NombreImagen);

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

                // Eliminar imagen anterior si existe
                // Eliminar imagen anterior si existe
                if (!string.IsNullOrEmpty(contenido.NombreImagen))
                {
                    try
                    {
                        // Eliminar imagen del almacenamiento
                        await EliminarDeStorage(contenido.NombreImagen);
                        Console.WriteLine("Imagen anterior eliminada del almacenamiento.");
                    }
                    catch (Exception ex)
                    {
                        // La imagen no existe en Firebase, proceder con el reemplazo
                        Console.WriteLine($"La imagen no existe en el almacenamiento: {ex.Message}. Reemplazando con la nueva imagen.");
                    }
                }

                // Extraer el nombre del archivo seleccionado
                var nombreArchivo = Path.GetFileName(imagen.FileName); // Obtiene el nombre del archivo
                                                                       // Generar un nombre único para el archivo
                var nombreArchivoUnico = $"{Guid.NewGuid()}{nombreArchivo}";

                // Convertir la imagen en byte[] y crear un archivo temporal
                using (var stream = new MemoryStream())
                {
                    // Copia el archivo recibido en el MemoryStream
                    await imagen.CopyToAsync(stream);



                    // Crear un archivo temporal en el sistema
                    var tempFilePath = Path.Combine(Path.GetTempPath(), nombreArchivoUnico);

                    // Escribir los bytes de la imagen en el archivo temporal
                    await System.IO.File.WriteAllBytesAsync(tempFilePath, stream.ToArray());

                    // Abrir el archivo temporal como un FileStream
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                    {
                        // Subir el archivo temporal a Firebase
                        var urlImagen = await SubirStorage(fileStream, nombreArchivoUnico);
                        contenido.ImagenUrl = urlImagen;  // Asignar la URL de la imagen al DTO
                        contenido.NombreImagen = nombreArchivoUnico;

                        // Eliminar el archivo temporal después de subirlo
                        if (System.IO.File.Exists(tempFilePath))
                        {
                            fileStream.Close();  // Asegúrate de cerrar el FileStream antes de eliminar el archivo
                            System.IO.File.Delete(tempFilePath); // Eliminar el archivo temporal
                        }
                    }
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

                //bool nombreExistente;
                //nombreExistente = await _context.Contenidos.AnyAsync(p => p.TipoComentarios == contenido.TipoComentarios);
                Contenido nombreExistente = null; // Declarar explícitamente el tipo y asignar null

                if (contenido.TipoContenido == "Texto")
                {
                    nombreExistente = _context.Contenidos
                                                     .FirstOrDefault(a => a.TipoComentarios == contenido.TipoComentarios);
                }


                if (nombreExistente != null)
                {
                    rsp.status = false;
                    rsp.msg = "Ya existe un contenido con el mismo nombre";
                    return Ok(rsp);
                }

                //var nombreExistente = await _context.Usuarios.AnyAsync(p => p.NombreCompleto == usuario.NombreCompleto);

                if (nombreExistente == null)
                {
                    if (contenido.TipoContenido != "Texto")
                    {
                        var guid = Guid.NewGuid().ToString();
                        contenido.NombreImagen = $"{guid}-{contenido.NombreImagen}";

                        // Convertir el arreglo de bytes a un stream de imagen
                        using (var stream = new MemoryStream(contenido.Imagenes))
                        {
                            // Subir la imagen a Firebase Storage
                            var imagenUrl = await SubirStorage(stream, contenido.NombreImagen);
                            contenido.ImagenUrl = imagenUrl;  // Asignar la URL de la imagen al DTO
                            contenido.Imagenes = null;
                        }

                    }

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




        private async Task<string> SubirStorage(Stream archivo, string nombre)
        {
            try
            {
                string email = "sofemprethy@gmail.com";
                string clave = "123456";
                string ruta = "appsistemaventa2024.appspot.com";
                string api_key = "AIzaSyD2_V_dDjLBCU6K1zsPJTHxddgNmaBK7SI";

                var auth = new FirebaseAuthProvider(new FirebaseConfig(api_key));
                var a = await auth.SignInWithEmailAndPasswordAsync(email, clave);

                var cancellation = new CancellationTokenSource();

                var task = new FirebaseStorage(
                    ruta,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                        ThrowOnCancel = true
                    })
                    .Child("Imagen_Contenido_Gym")
                    .Child(nombre)
                    .PutAsync(archivo, cancellation.Token);

                // Esperar a que termine la subida y obtener la URL de la imagen
                var downloadUrl = await task;
                return downloadUrl; // Retorna la URL de descarga de la imagen

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error subiendo imagen a Firebase: {ex.Message}");
                throw;
            }
        }






        private async Task EliminarDeStorage(string nombre)
        {
            // INGRESA AQUÍ TUS PROPIAS CREDENCIALES
            //string email = "carloscotes48@gmail.com";
            //string clave = "goten170797";
            //string ruta = "appsistemaventa.appspot.com";
            //string api_key = "AIzaSyBecbs4061LHECO9sPQru2jgkvYppceSQc";
            string email = "sofemprethy@gmail.com";
            string clave = "123456";
            string ruta = "appsistemaventa2024.appspot.com";
            string api_key = "AIzaSyD2_V_dDjLBCU6K1zsPJTHxddgNmaBK7SI";

            var auth = new FirebaseAuthProvider(new FirebaseConfig(api_key));
            var a = await auth.SignInWithEmailAndPasswordAsync(email, clave);

            var task = new FirebaseStorage(
                ruta,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                    ThrowOnCancel = true
                })
                .Child("Imagen_Contenido_Gym")
                .Child(nombre);

            await task.DeleteAsync();
        }







    }
}

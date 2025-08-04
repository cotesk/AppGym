using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Gym.BLL.Servicios.Contrato;
using Gym.DTO;
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
using System.IO;


namespace Gym.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class EmpresaController : ControllerBase
    {
        private readonly IEmpresaService _empresaServicio;

        private readonly DbgymContext _context;

        public EmpresaController(IEmpresaService empresaServicio, DbgymContext context)
        {
            _empresaServicio = empresaServicio;

            _context = context;
        }



        //[Authorize]
        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> Lista()
        {
            var rsp = new Response<List<EmpresaDTO>>();

            try
            {
                rsp.status = true;
                var listaProductos = await _empresaServicio.Lista();

                // Verificar si la lista de productos es nula antes de asignarla a rsp.value
                rsp.value = listaProductos ?? new List<EmpresaDTO>();
            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }

        [HttpGet]
        [Route("ListaCard")]
        public async Task<IActionResult> ListaCard()
        {
            var rsp = new Response<List<EmpresaDTO>>();

            try
            {
                rsp.status = true;
                var listaProductos = await _empresaServicio.Lista();

                // Proyectar la lista de productos solo con los campos necesarios
                rsp.value = listaProductos.Select(p => new EmpresaDTO
                {
                    //IdEmpresa = p.IdEmpresa,
                    Logo = p.Logo,
                    NombreEmpresa = p.NombreEmpresa,
                    Telefono = p.Telefono,
                    Facebook = p.Facebook,
                    Instagram = p.Instagram,
                    Tiktok = p.Tiktok,


                }).ToList();
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
        public async Task<IActionResult> Editar([FromBody] EmpresaDTO producto)
        {
            var rsp = new Response<bool>();

            try
            {


                rsp.status = true;
                rsp.value = await _empresaServicio.Editar(producto);
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
                var empresa = await _context.Empresas.FindAsync(id);

                // Eliminar la imagen asociada en Firebase Storage
                if (!string.IsNullOrEmpty(empresa.LogoNombre))
                    await EliminarDeStorage(empresa.LogoNombre);

                rsp.status = true;
                rsp.value = await _empresaServicio.Eliminar(id);


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
                Console.WriteLine($"ID de la empresa en ActualizarImagen: {id}");

                // Verifica si la imagen se está recibiendo correctamente
                if (imagen == null)
                {
                    return BadRequest(new { Mensaje = "La imagen no se recibió correctamente." });
                }

                var empresa = await _context.Empresas.FirstOrDefaultAsync(p => p.IdEmpresa == id);

                if (empresa == null)
                {
                    return NotFound(new { Mensaje = "Empresa no encontrada" });
                }

                // Eliminar imagen anterior si existe
                //if (!string.IsNullOrEmpty(empresa.LogoNombre))
                //{
                //    // Eliminar la imagen de Firebase si ya existe
                //    await EliminarDeStorage(empresa.LogoNombre);
                //}

                // Extraer el nombre del archivo seleccionado
                var nombreArchivo = Path.GetFileName(imagen.FileName); // Obtiene el nombre del archivo

                // Convertir la imagen en byte[] y crear un archivo temporal
                using (var stream = new MemoryStream())
                {
                    // Copia el archivo recibido en el MemoryStream
                    await imagen.CopyToAsync(stream);

                    // Guardar los bytes de la imagen en la base de datos
                    empresa.Logo = stream.ToArray(); // Guarda los bytes de la imagen en el campo Logo
                    empresa.LogoNombre = nombreArchivo; // Guarda el nombre del archivo

                    // Crear un archivo temporal en el sistema
                    var tempFilePath = Path.Combine(Path.GetTempPath(), nombreArchivo);

                    // Escribir los bytes de la imagen en el archivo temporal
                    await System.IO.File.WriteAllBytesAsync(tempFilePath, stream.ToArray());

                    // Abrir el archivo temporal como un FileStream
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                    {
                        // Subir el archivo temporal a Firebase
                        //var urlImagen = await SubirStorage(fileStream, nombreArchivo);

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
        public async Task<IActionResult> Guardar([FromBody] EmpresaDTO empresa)
        {

            var rsp = new Response<EmpresaDTO>();
            try
            {

                var nombreExistente = await _context.Empresas.AnyAsync(p => p.NombreEmpresa == empresa.NombreEmpresa);

                if (nombreExistente)
                {
                    rsp.status = false;
                    rsp.msg = "Ya existe una empresa con el mismo nombre";
                    return Ok(rsp);
                }
                // Convertir el arreglo de bytes a un stream de imagen
                //using (var stream = new MemoryStream(empresa.Logo))
                //{
                //    // Subir la imagen a Firebase Storage
                //    var urlImagen = await SubirStorage(stream, empresa.LogoNombre);

                //}

                rsp.status = true;
                rsp.value = await _empresaServicio.Crear(empresa);


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
                    .Child("Imagen_Empresa")
                    .Child(nombre)
                    .PutAsync(archivo, cancellation.Token);

                return await task;
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
                .Child("Imagen_Empresa")
                .Child(nombre);

            await task.DeleteAsync();
        }





    }
}


using Firebase.Auth;
using Firebase.Storage;
using Gym.Api.Utilidad;
using Gym.BLL.Servicios;
using Gym.BLL.Servicios.Contrato;
using Gym.DTO;
using Gym.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Gym.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioServicio;
        private readonly CryptographyUtility _cryptoUtility;
        private readonly IEmailService _emailServicio;
        private readonly DbgymContext _context;
        private readonly IConfiguration _configuration;


        public UsuarioController(IUsuarioService usuarioServicio, IEmailService emailServicio, DbgymContext context, IConfiguration configuration)
        {
            _usuarioServicio = usuarioServicio;
            _cryptoUtility = new CryptographyUtility(configuration);
            _emailServicio = emailServicio;
            _context = context;
            _configuration = configuration;

        }

        [Authorize]
        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> Lista()
        {

            var rsp = new Response<List<UsuarioDTO>>();

            try
            {
                rsp.status = true;
                rsp.value = await _usuarioServicio.Lista();


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
        [Route("ListaUsuarios")]
        public async Task<IActionResult> ListaUsuarios()
        {
            var rsp = new Response<List<UsuarioDTO>>();

            try
            {
                rsp.status = true;
                var listaUsuarios = await _usuarioServicio.Lista();

                // Mapear la lista de usuarios a UsuarioDTO excluyendo el campo ImageData
                rsp.value = listaUsuarios.Select(u => new UsuarioDTO
                {
                    IdUsuario = u.IdUsuario,
                    NombreCompleto = u.NombreCompleto,
                    Correo = u.Correo,
                    IdRol = u.IdRol,
                    RolDescripcion = u.RolDescripcion,
                    EsActivo = u.EsActivo,
                    Clave = u.Clave,
                    RefreshToken = u.RefreshToken,
                    RefreshTokenExpiry = u.RefreshTokenExpiry,
                    ImagenUrl = u.ImagenUrl,
                    NombreImagen = u.NombreImagen
                }).ToList();
            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }


        [HttpGet]
        [Route("imagen/{id}")]
        public IActionResult ObtenerImagenProducto(int id)
        {
            var producto = _context.Usuarios.Find(id);
            if (producto == null) return NotFound();
            return Ok(new { ImagenUrl = producto.ImagenUrl });
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetClaveSecreta()
        {
            // Obtener la clave secreta desde la configuración
            var claveSecreta = _configuration["ClaveSecreta:Key"];

            if (string.IsNullOrEmpty(claveSecreta))
            {
                return NotFound("Clave secreta no encontrada en la configuración.");
            }

            // Devolver en formato JSON
            return Ok(new { ClaveSecreta = claveSecreta });
        }


        [Authorize]
        [Route("ListaPaginada")]
        [HttpGet]
        public ActionResult<IEnumerable<UsuarioDTO>> GetUsuarios(int page = 1, int pageSize = 5, string searchTerm = null)
        {
            IQueryable<UsuarioDTO> query = _context.Usuarios
                .Where(u => u.IdRol != 4) // Excluir usuarios con IdRol igual a 4
                .Select(u => new UsuarioDTO
                {
                    IdUsuario = u.IdUsuario,
                    NombreCompleto = u.NombreCompleto,
                    Correo = u.Correo,
                    IdRol = u.IdRol,
                    RolDescripcion = u.IdRolNavigation.Nombre, // Obtener la descripción del rol desde la navegación
                    Clave = u.Clave,
                    EsActivo = u.EsActivo.HasValue ? (u.EsActivo.Value ? 1 : 0) : (int?)null,
                    //ImageData = u.ImageData,
                    ImagenUrl = u.ImagenUrl,
                    NombreImagen = u.NombreImagen,
                    RefreshToken = u.RefreshToken,
                    RefreshTokenExpiry = u.RefreshTokenExpiry
                });

            if (int.TryParse(searchTerm, out int isActive))
            {
                query = query.Where(c => c.EsActivo == isActive);
            }
            else if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c =>
                    c.NombreCompleto.Contains(searchTerm) ||
                    c.RolDescripcion.Contains(searchTerm) ||
                    c.Correo.Contains(searchTerm)
                );
            }


            // Ordenar alfabéticamente por nombre
            query = query.OrderBy(c => c.NombreCompleto);

            var totalUsuarios = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalUsuarios / pageSize);

            var usuariosPaginados = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new { data = usuariosPaginados, total = totalUsuarios, totalPages });
        }


        [Authorize]
        [Route("ListaPaginadaCliente")]
        [HttpGet]
        public ActionResult<IEnumerable<UsuarioDTO>> GetClientes(int page = 1, int pageSize = 5, string searchTerm = null)
        {
            IQueryable<UsuarioDTO> query = _context.Usuarios
                .Where(u => u.IdRol == 3) 
                .Select(u => new UsuarioDTO
                {
                    IdUsuario = u.IdUsuario,
                    NombreCompleto = u.NombreCompleto,
                    Correo = u.Correo,
                    IdRol = u.IdRol,
                    RolDescripcion = u.IdRolNavigation.Nombre, // Obtener la descripción del rol desde la navegación
                    Clave = u.Clave,
                    EsActivo = u.EsActivo.HasValue ? (u.EsActivo.Value ? 1 : 0) : (int?)null,
                    //ImageData = u.ImageData,
                    ImagenUrl = u.ImagenUrl,
                    NombreImagen = u.NombreImagen,
                    RefreshToken = u.RefreshToken,
                    RefreshTokenExpiry = u.RefreshTokenExpiry
                });

            // Convertir searchTerm a bool si es posible
            if (int.TryParse(searchTerm, out int isActive))
            {
                query = query.Where(c => c.EsActivo == isActive);
            }


            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c =>
                    c.NombreCompleto.Contains(searchTerm) ||
                    c.RolDescripcion.Contains(searchTerm) ||
                    c.Correo.Contains(searchTerm)
                );
            }

            // Ordenar alfabéticamente por nombre
            query = query.OrderBy(c => c.NombreCompleto);

            var totalUsuarios = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalUsuarios / pageSize);

            var usuariosPaginados = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new { data = usuariosPaginados, total = totalUsuarios, totalPages });
        }

        // [Authorize]
        [HttpGet]
        [Route("ObtenerUsuarioPorId/{idUsuario:int}")]
        public async Task<IActionResult> ObtenerUsuarioPorId(int idUsuario)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }
        [HttpGet]
        [Route("ObtenerUsuarioPorcorreo/{correo}")]
        public async Task<IActionResult> ObtenerUsuarioPorcorreo(string correo)
        {
            //var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);

            var usuario = await _context.Usuarios
       .Where(u => u.Correo == correo)
       .Select(u => new
       {
           u.Correo,
           u.EsActivo,
           u.IdRolNavigation.Nombre
       })
       .FirstOrDefaultAsync();



            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }

        //[HttpPost]
        //[Route("IniciarSesion")]
        //public async Task<IActionResult> IniciarSesion([FromBody] LoginDTO login)
        //{

        //    var rsp = new Response<SesionDTO>();

        //    try
        //    {
        //        rsp.status = true;
        //        rsp.value = await _usuarioServicio.ValidadarCredenciales(login.Correo,login.Clave);


        //    }
        //    catch (Exception ex)
        //    {
        //        rsp.status = false;
        //        rsp.msg = ex.Message;
        //    }
        //    return Ok(rsp);


        //}
        private UsuarioDTO ConvertirSesionAUsuario(SesionDTO sesion)
        {
            return new UsuarioDTO
            {
                IdUsuario = sesion.IdUsuario,
                NombreCompleto = sesion.NombreCompleto,
                Correo = sesion.Correo,
                RolDescripcion = sesion.RolDescripcion,
                ImagenUrl = sesion.ImagenUrl,
                NombreImagen = sesion.NombreImagen,
                //ImageData = sesion.ImageData,
                Clave = sesion.Clave,

            };
        }


        [HttpPost]
        [Route("IniciarSesion")]
        public async Task<IActionResult> IniciarSesion([FromBody] LoginDTO login)
        {
            try
            {
                // Intentar validar las credenciales
                var usuario = await _usuarioServicio.ValidadarCredenciales(login.Correo, login.Clave);

                if (usuario == null)
                {
                    return Unauthorized(new { mensaje = "Credenciales inválidas." });
                }

                // Si las credenciales son válidas, establece el status en true
                var status = true;
                var value = usuario;
                var claims = new[]
                {
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreCompleto)
        };

                var jwtSettings = _configuration.GetSection("JwtSettings");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.GetValue<string>("Key")));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(60), // Ajusta el tiempo de expiración del token según tus necesidades
                    signingCredentials: creds);

                // Convertir la sesión a un objeto UsuarioDTO
                var usuarioDto = ConvertirSesionAUsuario(usuario);
                // Actualiza el RefreshToken y RefreshTokenExpiry para el usuario
                await ActualizarRefreshToken(usuarioDto);

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                // Devuelve el token, el usuario y el status como parte de la respuesta
                return Ok(new { token = tokenString, refreshToken = usuarioDto.RefreshToken, status = status, value = value });
            }
            catch (ArgumentException ex)
            {
                // Manejar excepciones específicas de ArgumentException
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (TaskCanceledException ex)
            {
                // Manejar las excepciones relacionadas con la tarea cancelada
                return StatusCode(StatusCodes.Status408RequestTimeout, new { mensaje = "La solicitud se canceló." });
            }
            catch (Exception ex)
            {
                // Manejar cualquier otra excepción
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error inesperado: " + ex.Message });
            }
        }



        private async Task ActualizarRefreshToken(UsuarioDTO usuario)
        {
            // Generar un refresh token único
            var refreshToken = Guid.NewGuid().ToString();

            // Duración del refresh token: 1 día
            var refreshTokenExpiry = DateTime.Now.AddDays(1);
            //var refreshTokenExpiry = DateTime.Now.AddHours(2); // Expira en 2 horas
            // var refreshTokenExpiry = DateTime.Now.AddMinutes(15);
            try
            {
                // Buscar el usuario por su correo electrónico en la base de datos
                var usuarioDb = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == usuario.Correo);

                if (usuarioDb != null)
                {
                    // Actualizar los campos RefreshToken y RefreshTokenExpiry del usuario encontrado
                    usuarioDb.RefreshToken = refreshToken;
                    usuarioDb.RefreshTokenExpiry = refreshTokenExpiry;

                    // Guardar los cambios en la base de datos
                    await _context.SaveChangesAsync();

                    // Actualizar el RefreshToken y RefreshTokenExpiry del objeto UsuarioDTO
                    usuario.RefreshToken = refreshToken;
                    usuario.RefreshTokenExpiry = refreshTokenExpiry;
                }
                else
                {
                    // El usuario no se encontró en la base de datos
                    // Puedes manejar este caso de acuerdo a tus necesidades, por ejemplo, lanzando una excepción o registrando un mensaje de error
                    throw new Exception("Usuario no encontrado en la base de datos.");
                }
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que pueda ocurrir al actualizar el refresh token
                // Puedes registrar el error, lanzar una excepción, etc.
                throw new Exception("Error al actualizar el refresh token: " + ex.Message);
            }
        }


        [HttpPost]
        [Route("RenovarToken")]
        public async Task<IActionResult> RenovarToken([FromBody] RefreshTokenDTO refreshTokenDto)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.RefreshToken == refreshTokenDto.RefreshToken);


            if (usuario == null)
            {
                return Unauthorized();
            }

            // Verifica si el refreshTokenExpiry ha expirado
            if (usuario.RefreshTokenExpiry <= DateTime.Now)
            {
                // Si el refreshTokenExpiry ha expirado, aun así genera un nuevo token de acceso y actualiza el refreshTokenExpiry
                /* usuario.RefreshTokenExpiry = DateTime.Now.AddMinutes(15);*/ // Ajusta la expiración del refreshToken según tus necesidades
                usuario.RefreshTokenExpiry = DateTime.Now.AddDays(1);
                await _context.SaveChangesAsync();
            }

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
        new Claim(ClaimTypes.Name, usuario.NombreCompleto)
    };

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.GetValue<string>("Key")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                /* expires: DateTime.Now.AddMinutes(15),*/ // Ajusta la expiración del token según tus necesidades
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Actualiza el refreshTokenExpiry en la base de datos
            //usuario.RefreshTokenExpiry = DateTime.Now.AddMinutes(2); // Ajusta la expiración del refreshToken según tus necesidades
            //await _context.SaveChangesAsync();


            return Ok(new { token = tokenString });
        }

        [Authorize]
        [HttpPost]
        [Route("Guardar")]
        public async Task<IActionResult> Guardar([FromBody] UsuarioDTO usuario)
        {

            var rsp = new Response<UsuarioDTO>();

            try
            {

                var nombreExistente = await _context.Usuarios.AnyAsync(p => p.NombreCompleto == usuario.NombreCompleto);

                if (nombreExistente)
                {
                    rsp.status = false;
                    rsp.msg = "Ya existe un producto con el mismo nombre";
                    return Ok(rsp);
                }


                // Convertir el arreglo de bytes a un stream de imagen
                using (var stream = new MemoryStream(usuario.ImageData))
                {
                    var guid = Guid.NewGuid().ToString();
                    usuario.NombreImagen = $"{guid}-{usuario.NombreImagen}";
                    // Subir la imagen a Firebase Storage
                    var imagenUrl = await SubirStorage(stream, usuario.NombreImagen);
                    usuario.ImagenUrl = imagenUrl;  // Asignar la URL de la imagen al DTO
                    usuario.ImageData = null;
                }



                rsp.status = true;
                rsp.value = await _usuarioServicio.Crear(usuario);


            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }
            return Ok(rsp);


        }


        [HttpPost]
        [Route("GuardarNuevo")]
        public async Task<IActionResult> GuardarNuevo([FromBody] UsuarioCreateDTO usuarioCreateDTO)
        {
            var rsp = new Response<UsuarioDTO>();

            try
            {


                // Convertir el arreglo de bytes a un stream de imagen
                using (var stream = new MemoryStream(usuarioCreateDTO.ImageData))
                {
                    var guid = Guid.NewGuid().ToString();
                    usuarioCreateDTO.NombreImagen = $"{guid}-{usuarioCreateDTO.NombreImagen}";
                    // Subir la imagen a Firebase Storage
                    var imagenUrl = await SubirStorage(stream, usuarioCreateDTO.NombreImagen);
                    usuarioCreateDTO.ImagenUrl = imagenUrl;  // Asignar la URL de la imagen al DTO
                    usuarioCreateDTO.ImageData = null;
                }

                // Map UsuarioCreateDTO to UsuarioDTO
                var usuarioDTO = new UsuarioDTO
                {
                    NombreCompleto = usuarioCreateDTO.NombreCompleto,
                    Correo = usuarioCreateDTO.Correo,
                    IdRol = 4,
                    Clave = usuarioCreateDTO.Clave,
                    EsActivo = 0,
                    RolDescripcion = "Clientes",
                    //ImageData = usuarioCreateDTO.ImageData,
                    ImagenUrl = usuarioCreateDTO.ImagenUrl,
                    NombreImagen = usuarioCreateDTO.NombreImagen,
                };




                // Call service method to create user
                var usuarioCreado = await _usuarioServicio.Crear(usuarioDTO);

                // Return the result
                rsp.status = true;
                rsp.value = usuarioCreado;
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
        [Route("EditarUsuario/{id}")]
        public async Task<IActionResult> EditarUsuario([FromBody] UsuarioDTO usuario, int id)
        {
            var rsp = new Response<bool>();

            if (usuario == null || id != usuario.IdUsuario)
            {
                return BadRequest("Datos no válidos");
            }

            try
            {
                var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);

                if (usuarioExistente == null)
                {
                    return NotFound("Usuario no encontrado");
                }
                var nombreExistente = await _context.Usuarios.AnyAsync(p => p.NombreCompleto == usuario.NombreCompleto && p.IdUsuario != usuario.IdUsuario);

                if (nombreExistente)
                {
                    rsp.status = false;
                    rsp.msg = "El nombre del usuario ya existe.";
                    return Ok(rsp);
                }
                var correoExistente = await _context.Usuarios.AnyAsync(p => p.Correo == usuario.Correo && p.IdUsuario != usuario.IdUsuario);

                if (correoExistente)
                {
                    rsp.status = false;
                    rsp.msg = "El nombre del correo ya existe.";
                    return Ok(rsp);
                }
                // Actualiza los campos del usuario existente con los valores del DTO
                //usuarioExistente.NombreCompleto = "Nuevo Nombre";
                //usuarioExistente.Correo = "nuevo@correo.com";

                usuarioExistente.NombreCompleto = usuario.NombreCompleto;
                usuarioExistente.Correo = usuario.Correo;
                usuarioExistente.Clave = usuario.Clave;
                //usuarioExistente.ImageData = usuario.ImageData;
                // Otras propiedades según sea necesario

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();

                return Ok(new { status = true, value = true, msg = (string)null });
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return StatusCode(StatusCodes.Status500InternalServerError, new { status = false, msg = ex.Message });
            }

        }
        [Authorize]
        [HttpPut]
        [Route("Editar")]
        public async Task<IActionResult> Editar([FromBody] UsuarioDTO usuario, int id)
        {
            Console.WriteLine($"Usuario antes de actualizar: {JsonConvert.SerializeObject(usuario)}");

            var rsp = new Response<bool>();


            try
            {
                rsp.status = true;
                rsp.value = await _usuarioServicio.Editar(usuario);


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


                var usuario = await _context.Usuarios.FindAsync(id);
                // Eliminar la imagen asociada en Firebase Storage
                if (!string.IsNullOrEmpty(usuario.NombreImagen))
                    await EliminarDeStorage(usuario.NombreImagen);


                rsp.status = true;
                rsp.value = await _usuarioServicio.Eliminar(id);


            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }
            return Ok(rsp);


        }

    
        private async Task<byte[]> DownloadImage(string imageUrl)
        {
            using var httpClient = new HttpClient();
            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
            return imageBytes;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }



        public class UsuarioInputModel
        {
            public string NombreCompleto { get; set; }
            public string Correo { get; set; }

        }

        private string GenerateRandomPassword()
        {
            // Generar una contraseña aleatoria, por ejemplo:
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            string password = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return password;
        }




        //[HttpPost]
        //[Route("ActualizarImagen/{id:int}")]
        //public async Task<IActionResult> ActualizarImagen(int id, [FromForm] IFormFile imagen)
        //{
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        await imagen.CopyToAsync(memoryStream);
        //        var imageData = memoryStream.ToArray();

        //        var rsp = new Response<string>();

        //    try
        //    {
        //        Console.WriteLine($"ID del producto en ActualizarImagen: {id}");

        //        // Verifica si la imagen se está recibiendo correctamente
        //        if (imagen == null)
        //        {
        //            return BadRequest(new { Mensaje = "La imagen no se recibió correctamente." });
        //        }

        //        var usuario = await _context.Usuarios.FirstOrDefaultAsync(p => p.IdUsuario == id);

        //        if (usuario == null)
        //        {
        //            return NotFound(new { Mensaje = "Producto no encontrado" });
        //        }

        //        // Convierte la imagen a un arreglo de bytes
        //        using (var stream = new MemoryStream())
        //        {
        //            await imagen.CopyToAsync(stream);
        //            usuario.ImageData = stream.ToArray();
        //        }

        //        // Guarda los cambios en la base de datos
        //        await _context.SaveChangesAsync();

        //        rsp.status = true;
        //        rsp.value = "Imagen actualizada correctamente";
        //    }
        //    catch (Exception ex)
        //    {
        //        rsp.status = false;
        //        rsp.msg = ex.Message;
        //    }


        //    return Ok(rsp);


        //    }
        //    return Ok();
        //}
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

                var contenido = await _context.Usuarios.FirstOrDefaultAsync(p => p.IdUsuario == id);

                if (contenido == null)
                {
                    return NotFound(new { Mensaje = "Producto no encontrado" });
                }

                // Intentar eliminar la imagen anterior si existe
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
                rsp.value = contenido.ImagenUrl;
                //rsp.value = "Imagen actualizada correctamente";
            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }


        //[Authorize]
        [HttpPost("RecuperarContraseña")]
        public async Task<IActionResult> RecuperarContraseña([FromBody] RecuperarContrasenaDTO solicitud)
        {
            var rsp = new Response<string>();

            try
            {
                // Buscar usuarios por correo electrónico
                var usuarios = await _context.Usuarios.Where(u => u.Correo == solicitud.Correo).ToListAsync();

                if (usuarios == null || usuarios.Count == 0)
                {
                    rsp.status = false;
                    rsp.msg = "No se encontro el correo electrónico para recuperar la contraseña.";
                    return Ok(rsp);
                }

                // Obtén la información de la empresa
                var empresa = _context.Empresas.FirstOrDefault();

                string nombre, direccion, correoEmoresa, nit, telefono, propietario;
                string logoUrl;

                var url = empresa != null ? await ObtenerUrlDeStorage(empresa.LogoNombre) : "Sin imagen";

                if (empresa != null)
                {
                    // Obtén los datos de la empresa
                    nombre = empresa.NombreEmpresa;
                    direccion = empresa.Direccion;
                    correoEmoresa = empresa.Correo;
                    nit = empresa.Nit;
                    telefono = empresa.Telefono;
                    propietario = empresa.Propietario;
                    logoUrl = url;

                }
                else
                {
                    // Datos por defecto
                    nombre = "Tu Empresa";
                    direccion = "Sin Dirección";
                    correoEmoresa = "Sin Correo";
                    logoUrl = url;
                    nit = "Sin NIT";
                    telefono = "Sin Teléfono";
                    propietario = "Sin Propietario";
                }

                // URL del GIF de alivio
                string gifUrl = "https://media.giphy.com/media/uWqiZM2vzncysGBon5/giphy.gif";

                // Envía un correo electrónico con la contraseña a cada usuario encontrado
                foreach (var usuario in usuarios)
                {
                    var mensajeHtml = $@"
<!DOCTYPE html>
<html lang='es'>
  <body style='font-family: Segoe UI, Roboto, Arial, sans-serif; background-color: #f4f4f9; color: #2f3640; text-align: center; padding: 40px 20px;'>
    <div style='max-width: 520px; margin: auto; background-color: #fff; padding: 24px; border-radius: 10px; box-shadow: 0 4px 12px rgba(0,0,0,0.05);'>

   
      <div style='margin-bottom: 20px;'>
        <img src='{logoUrl}' alt='Logo de {nombre}' style='max-width: 160px; height: auto; border-radius: 8px;' />
      </div>

   
      <h2 style='font-size: 20px; font-weight: 600; margin-bottom: 12px;'>Recuperación de Contraseña</h2>

    
      <p style='margin: 0 0 12px;'>Hola, <strong>{usuario.NombreCompleto}</strong>,</p>
      <p style='margin: 0 0 24px;'>Tu contraseña es:</p>
      <p style='font-size: 24px; font-weight: bold; color: #e63946; margin: 0;'>{usuario.Clave}</p>

     
      <img src='{gifUrl}' alt='GIF de alivio' style='max-width: 280px; margin: 24px auto; display: block;' />

    
      <p style='font-size: 13px; color: #888; margin-top: 24px;'>Si no solicitaste este correo, simplemente ignóralo.</p>

    
      <div style='margin-top: 30px; font-size: 11px; color: #aaa;'>
        <p>{nombre} · {direccion}</p>
        <p>Tel: {telefono} · Correo: {correoEmoresa}</p>
        <p>NIT: {nit}</p>
      </div>
    </div>
  </body>
</html>";

                    await _emailServicio.EnviarCorreoElectronico(
                        usuario.Correo,
                        "Recuperación de Contraseña",
                        mensajeHtml,
                        true // Asegurando que el contenido es HTML
                    );
                }

                rsp.status = true;
                rsp.value = "Se ha enviado la contraseña a tu correo electrónico.";
            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }


        [HttpPost("SolicitarRestablecimientoContrasena")]
        public async Task<IActionResult> SolicitarRestablecimientoContrasena([FromBody] RecuperarContrasenaDTO solicitud)
        {
            var rsp = new Response<string>();

            try
            {
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == solicitud.Correo);

                if (usuario == null)
                {
                    rsp.status = false;
                    rsp.msg = "No se encontró un usuario con ese correo electrónico.";
                    return Ok(rsp);
                }

                // Generar token de restablecimiento de contraseña
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim("idUsuario", usuario.IdUsuario.ToString()) }),
                    Expires = DateTime.UtcNow.AddMinutes(10),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var resetToken = tokenHandler.WriteToken(token);
                //var resetToken = _cryptoUtility.EncryptToken(tokenHandler.WriteToken(token));
                var correo = solicitud.Correo;

                //cuando suba esto a un servidor, cambiar la direcion ejemplo $"http://www.ejemplo.com/restablecer-contrasena?correo={correo}&token={resetToken}"
                // Crear URL de restablecimiento de contraseña
                //var resetUrl = $"http://localhost:4200/restablecer-contrasena?correo={correo}&token={resetToken}";
                var resetUrl = $"https://appgym-c350c.web.app/restablecer-contrasena?correo={correo}&token={resetToken}";

                // Obtén la información de la empresa
                var empresa = _context.Empresas.FirstOrDefault();

                string nombre, direccion, correoEmoresa, nit, telefono, propietario;
                string logoUrl;

                var url = empresa != null ? await ObtenerUrlDeStorage(empresa.LogoNombre) : "Sin imagen";

                if (empresa != null)
                {
                    // Obtén los datos de la empresa
                    nombre = empresa.NombreEmpresa;
                    direccion = empresa.Direccion;
                    correoEmoresa = empresa.Correo;
                    nit = empresa.Nit;
                    telefono = empresa.Telefono;
                    propietario = empresa.Propietario;
                    logoUrl = url;

                }
                else
                {
                    // Datos por defecto
                    nombre = "Tu Empresa";
                    direccion = "Sin Dirección";
                    correoEmoresa = "Sin Correo";
                    logoUrl = url;
                    nit = "Sin NIT";
                    telefono = "Sin Teléfono";
                    propietario = "Sin Propietario";
                }

                var mensajeHtml = $@"
<!DOCTYPE html>
<html lang='es'>
  <body style='font-family: Segoe UI, Roboto, Arial, sans-serif; background-color: #f4f4f9; color: #2f3640; text-align: center; padding: 40px 20px;'>
    <div style='max-width: 520px; margin: auto; background-color: #fff; padding: 24px; border-radius: 10px; box-shadow: 0 4px 12px rgba(0,0,0,0.05);'>

    
      <div style='margin-bottom: 20px;'>
        <img src='{logoUrl}' alt='Logo de {nombre}' style='max-width: 160px; height: auto; border-radius: 8px;' />
      </div>

   
      <h2 style='font-size: 20px; font-weight: 600; margin-bottom: 12px;'>Restablecer Contraseña</h2>

    
      <p style='margin: 0 0 12px;'>Hola, <strong>{usuario.NombreCompleto}</strong>,</p>
      <p style='margin: 0 0 24px;'>Haz clic en el siguiente botón para restablecer tu contraseña:</p>

     
      <a href='{resetUrl}' style='display: inline-block; padding: 12px 24px; background-color: #28a745; color: #fff; text-decoration: none; border-radius: 6px; font-weight: 500;'>Restablecer Contraseña</a>

    
      <p style='font-size: 13px; color: #888; margin-top: 24px;'>Si no solicitaste este cambio, simplemente ignora este mensaje.</p>

     
      <div style='margin-top: 30px; font-size: 11px; color: #aaa;'>
        <p>{nombre} · {direccion}</p>
        <p>Tel: {telefono} · Correo: {correoEmoresa}</p>
        <p>NIT: {nit}</p>
      </div>
    </div>
  </body>
</html>";

                await _emailServicio.EnviarCorreoElectronico(
                    usuario.Correo,
                    "Restablecimiento de Contraseña",
                    mensajeHtml,
                    true
                );

                rsp.status = true;
                rsp.value = "Se ha enviado un enlace para restablecer la contraseña a tu correo electrónico.";
            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }


        [HttpPost("RestablecerContrasena")]
        public async Task<IActionResult> RestablecerContrasena([FromBody] RestablecerContrasenaDTO model)
        {
            var rsp = new Response<string>();

            try
            {
                //esto es para el token si decodificacion 
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"]);
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(model.Token, new TokenValidationParameters

                //var decryptedToken = _cryptoUtility.DecryptToken(model.Token);
                //var tokenHandler = new JwtSecurityTokenHandler();
                //var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"]);

                //SecurityToken validatedToken;
                //var principal = tokenHandler.ValidateToken(decryptedToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out validatedToken);

                var userId = int.Parse(principal.FindFirst("idUsuario").Value);
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == userId);

                if (usuario == null)
                {
                    rsp.status = false;
                    rsp.msg = "Usuario no encontrado.";
                    return Ok(rsp);
                }

                // Actualizar la contraseña del usuario
                usuario.Clave = model.NuevaContrasena; // Asegúrate de hashear la contraseña antes de guardarla
                await _context.SaveChangesAsync();

                rsp.status = true;
                rsp.value = "Contraseña restablecida exitosamente.";
            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }



        [HttpPost("ActivarUsuario")]
        public async Task<IActionResult> ActivarUsuario([FromBody] RecuperarContrasenaDTO solicitud)
        {
            var rsp = new Response<string>();

            try
            {

                // Obtener la zona horaria de Colombia
                TimeZoneInfo zonaColombia = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

                // Obtener la hora actual en Colombia
                DateTime horaColombia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaColombia);


                // Buscar usuarios por correo electrónico
                var usuarios = await _context.Usuarios.Where(u => u.Correo == solicitud.Correo).ToListAsync();
                var usuarioz = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == solicitud.Correo);



                if (usuarios == null || usuarios.Count == 0)
                {
                    rsp.status = false;
                    rsp.msg = "No se encontró un usuario con ese correo electrónico";
                    return Ok(rsp);
                }

                if (usuarioz.EsActivo == true)
                {
                    rsp.status = false;
                    rsp.msg = "El usuario ya está activado.";
                    return Ok(rsp);
                }


                // 1. Generar un código único que no esté en uso
                string codigo;
                bool existe;

                do
                {
                    codigo = new Random().Next(1000, 9999).ToString();

                    existe = await _context.CodigoActivacions.AnyAsync(c =>
                      c.Codigo == codigo &&
                      c.Usado == false &&
                      c.FechaExpiracion > horaColombia);
                }
                while (existe);


                // 2. Crear y guardar el código
                var codigoActivacion = new CodigoActivacion
                {
                    IdUsuario = usuarioz.IdUsuario,
                    Codigo = codigo,
                    FechaGeneracion = horaColombia,
                    FechaExpiracion = horaColombia.AddMinutes(2),
                    Usado = false
                };

                _context.CodigoActivacions.Add(codigoActivacion);
                await _context.SaveChangesAsync();



                // URL del GIF de alivio
                string gifUrl = "https://media.giphy.com/media/uWqiZM2vzncysGBon5/giphy.gif";


                // Generar token 
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim("idUsuario", usuarioz.IdUsuario.ToString()) }),
                    Expires = horaColombia.AddMinutes(2),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var resetToken = tokenHandler.WriteToken(token);
                //var resetToken = _cryptoUtility.EncryptToken(tokenHandler.WriteToken(token));
                var correo = solicitud.Correo;

                //cuando suba esto a un servidor, cambiar la direcion ejemplo $"http://www.ejemplo.com/restablecer-contrasena?correo={correo}&token={resetToken}"
                // Crear URL de restablecimiento de contraseña
                //var resetUrl = $"http://localhost:4200/activar-cuenta?correo={correo}&token={resetToken}";
                var resetUrl = $"https://appgym-c350c.web.app/activar-cuenta?correo={correo}&token={resetToken}";


                // Obtén la información de la empresa
                var empresa = _context.Empresas.FirstOrDefault();

                string nombre, direccion, correoEmoresa, nit, telefono, propietario;
                string logoUrl;

                var url = empresa != null ? await ObtenerUrlDeStorage(empresa.LogoNombre) : "Sin imagen";

                if (empresa != null)
                {
                    // Obtén los datos de la empresa
                    nombre = empresa.NombreEmpresa;
                    direccion = empresa.Direccion;
                    correoEmoresa = empresa.Correo;
                    nit = empresa.Nit;
                    telefono = empresa.Telefono;
                    propietario = empresa.Propietario;
                    logoUrl = url;

                }
                else
                {
                    // Datos por defecto
                    nombre = "Tu Empresa";
                    direccion = "Sin Dirección";
                    correoEmoresa = "Sin Correo";
                    logoUrl = url;
                    nit = "Sin NIT";
                    telefono = "Sin Teléfono";
                    propietario = "Sin Propietario";
                }

                // Envía un correo electrónico con la contraseña a cada usuario encontrado
                foreach (var usuario in usuarios)
                {
                    var mensajeHtml = $@"
<!DOCTYPE html>
<html lang='es'>
  <body style='font-family: Segoe UI, Roboto, Arial, sans-serif; background-color: #f5f6fa; color: #2f3640; text-align: center; padding: 40px 20px;'>
    <div style='max-width: 480px; margin: auto; background-color: #ffffff; padding: 24px; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.05);'>

     
      <div style='margin-bottom: 24px;'>
        <img src='{logoUrl}' alt='Logo de {nombre}' style='max-width: 160px; height: auto; border-radius: 8px;' />
      </div>

    
      <h2 style='font-size: 20px; font-weight: 600; margin-bottom: 8px;'>Activación de Cuenta</h2>
      <p style='margin: 0 0 16px;'>Hola, <strong>{usuario.NombreCompleto}</strong></p>

   
      <p style='margin-bottom: 4px;'>Tu código de activación es:</p>
      <p style='font-size: 36px; font-weight: bold; color: #0056d2; margin: 0 0 20px;'>{codigo}</p>

     
      <a href='{resetUrl}' style='display: inline-block; padding: 10px 20px; background-color: #0056d2; color: #fff; text-decoration: none; border-radius: 4px; font-size: 14px;'>Ingresar Código</a>

    
      <p style='font-size: 12px; color: #888; margin-top: 24px;'>Este código expirará en 2 minutos.</p>
      <p style='font-size: 12px; color: #aaa;'>Si no solicitaste esta activación, puedes ignorar este mensaje.</p>

    
      <div style='margin-top: 30px; font-size: 11px; color: #bbb;'>
        <p>{nombre} · {direccion}</p>
        <p>NIT: {nit} · Tel: {telefono}</p>
        <p>Correo: {correoEmoresa}</p>
      </div>
    </div>
  </body>
</html>";




                    await _emailServicio.EnviarCorreoElectronico(
                        usuario.Correo,
                        "Activacion de cuenta",
                        mensajeHtml,
                        true // Asegurando que el contenido es HTML
                    );
                }

                rsp.status = true;
                rsp.value = "Se ha enviado el codigo de activacion a tu correo electrónico.";
            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }




        [HttpPost("Activacion")]
        public async Task<IActionResult> Activacion([FromBody] ActivacionDTO model)
        {
            var rsp = new Response<string>();

            try
            {

                // Obtener la zona horaria de Colombia
                TimeZoneInfo zonaColombia = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

                // Obtener la hora actual en Colombia
                DateTime horaColombia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaColombia);


                //esto es para el token si decodificacion 
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"]);
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(model.Token, new TokenValidationParameters

                //var decryptedToken = _cryptoUtility.DecryptToken(model.Token);
                //var tokenHandler = new JwtSecurityTokenHandler();
                //var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"]);

                //SecurityToken validatedToken;
                //var principal = tokenHandler.ValidateToken(decryptedToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out validatedToken);

                var userId = int.Parse(principal.FindFirst("idUsuario").Value);
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == userId);

                if (usuario == null)
                {
                    rsp.status = false;
                    rsp.msg = "Usuario no encontrado.";
                    return Ok(rsp);
                }

                var codigoValido = await _context.CodigoActivacions
                .Where(c => c.IdUsuario == userId && c.Codigo == model.Codigo && c.Usado == false)
                .OrderByDescending(c => c.FechaGeneracion)
                .FirstOrDefaultAsync();

                if (codigoValido == null || codigoValido.FechaExpiracion < horaColombia)
                    return BadRequest("Código inválido o expirado");

                // Marcar como usado
                codigoValido.Usado = true;

                // Actualizar la contraseña del usuario
                usuario.EsActivo = true; // Asegúrate de hashear la contraseña antes de guardarla
                await _context.SaveChangesAsync();

                rsp.status = true;
                rsp.value = "Usuario activo exitosamente.";
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
        [Route("exportar-usuarios")]
        public IActionResult ExportarUsuarios()
        {
            var usuarios = _context.Usuarios.ToList();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Establecer el contexto de licencia

            using (var package = new ExcelPackage())
            {

                var worksheet = package.Workbook.Worksheets.Add("Usuarios");

                // Encabezados de la tabla
                worksheet.Cells[1, 1].Value = "Nombre Completo";
                worksheet.Cells[1, 2].Value = "Correo";
                worksheet.Cells[1, 3].Value = "ID Rol";
                worksheet.Cells[1, 4].Value = "Clave";
                worksheet.Cells[1, 5].Value = "Es Activo";
                worksheet.Cells[1, 6].Value = "Fecha Registro";
                worksheet.Cells[1, 7].Value = "Refresh Token";
                worksheet.Cells[1, 8].Value = "Refresh Token Expiry";
                worksheet.Cells[1, 9].Value = "Imagen Nombre";
                worksheet.Cells[1, 10].Value = "Url Imagen";

                // Rellenar los datos de los usuarios
                int row = 2;
                foreach (var usuario in usuarios)
                {
                    worksheet.Cells[row, 1].Value = usuario.NombreCompleto;
                    worksheet.Cells[row, 2].Value = usuario.Correo;
                    worksheet.Cells[row, 3].Value = usuario.IdRol;
                    worksheet.Cells[row, 4].Value = usuario.Clave; // Ten cuidado con esto si deseas proteger las contraseñas
                    worksheet.Cells[row, 5].Value = usuario.EsActivo;
                    worksheet.Cells[row, 6].Value = usuario.FechaRegistro?.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 7].Value = usuario.RefreshToken;
                    worksheet.Cells[row, 8].Value = usuario.RefreshTokenExpiry?.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 9].Value = usuario.NombreImagen;
                    worksheet.Cells[row, 10].Value = usuario.ImagenUrl;
                    row++;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"Usuarios-{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("importar-usuarios")]
        public IActionResult ImportarUsuarios(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No se ha subido ningún archivo.");
            }

            using (var stream = new MemoryStream())
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Establecer el contexto de licencia
                file.CopyTo(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // Obtén la primera hoja
                    var usuarios = new List<Usuario>();

                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++) // Comienza en la segunda fila
                    {
                        var usuario = new Usuario
                        {
                            NombreCompleto = worksheet.Cells[row, 1].Value?.ToString(),
                            Correo = worksheet.Cells[row, 2].Value?.ToString(),
                            IdRol = Convert.ToInt32(worksheet.Cells[row, 3].Value),
                            Clave = worksheet.Cells[row, 4].Value?.ToString(),
                            EsActivo = Convert.ToBoolean(worksheet.Cells[row, 5].Value),
                            FechaRegistro = DateTime.TryParse(worksheet.Cells[row, 6].Value?.ToString(), out DateTime fecha) ? (DateTime?)fecha : null,
                            //ImageData = DecodeBase64Image(worksheet.Cells[row, 7].Value?.ToString()),
                            RefreshToken = worksheet.Cells[row, 7].Value?.ToString(),
                            RefreshTokenExpiry = DateTime.TryParse(worksheet.Cells[row, 8].Value?.ToString(), out DateTime expiry) ? (DateTime?)expiry : null,
                            NombreImagen = worksheet.Cells[row, 9].Value?.ToString(),
                            ImagenUrl = worksheet.Cells[row, 10].Value?.ToString(),
                        };

                        // Verifica si el usuario ya existe por su Correo
                        var existingUsuario = _context.Usuarios.FirstOrDefault(u => u.Correo == usuario.Correo);
                        if (existingUsuario != null)
                        {
                            // Si el usuario existe, actualiza sus propiedades
                            existingUsuario.NombreCompleto = usuario.NombreCompleto;
                            existingUsuario.IdRol = usuario.IdRol;
                            existingUsuario.Clave = usuario.Clave; // Aquí podrías agregar lógica para no sobrescribir la clave si no lo deseas
                            existingUsuario.EsActivo = usuario.EsActivo;
                            existingUsuario.FechaRegistro = usuario.FechaRegistro;
                            //existingUsuario.ImageData = usuario.ImageData;
                            existingUsuario.RefreshToken = usuario.RefreshToken;
                            existingUsuario.RefreshTokenExpiry = usuario.RefreshTokenExpiry;
                            existingUsuario.NombreImagen = usuario.NombreImagen;
                            existingUsuario.ImagenUrl = usuario.ImagenUrl;
                        }
                        else
                        {
                            // Si no existe, añade el nuevo usuario
                            usuarios.Add(usuario);
                        }
                    }

                    if (usuarios.Any())
                    {
                        _context.Usuarios.AddRange(usuarios);
                    }

                    _context.SaveChanges();
                }
            }

            return Ok("Usuarios importados correctamente.");
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
                    .Child("Imagen_Usuario_Gym")
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
                .Child("Imagen_Usuario_Gym")
                .Child(nombre);

            await task.DeleteAsync();
        }

        private async Task<string> ObtenerUrlDeStorage(string nombre)
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
                .Child("Imagen_Empresa_Gym")
                .Child(nombre);

            // Obtener la URL de descarga sin el token de cancelación
            var downloadURL = await task.GetDownloadUrlAsync();

            return downloadURL;
        }



        // Función para decodificar Base64
        private byte[] DecodeBase64Image(string base64Image)
        {
            if (string.IsNullOrWhiteSpace(base64Image))
                return null;

            base64Image = CleanBase64String(base64Image);

            int mod4 = base64Image.Length % 4;
            if (mod4 > 0)
            {
                base64Image += new string('=', 4 - mod4);
            }

            try
            {
                return Convert.FromBase64String(base64Image);
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Error al decodificar Base64: {ex.Message}");
                return null;
            }
        }

        // Limpiar caracteres no válidos de Base64
        private string CleanBase64String(string base64String)
        {
            return base64String.Replace(" ", "")
                               .Replace("\n", "")
                               .Replace("\r", "")
                               .Trim();
        }



    }
}

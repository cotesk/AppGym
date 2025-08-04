using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Gym.BLL.Servicios.Contrato;
using Gym.DTO;
using Gym.Api.Utilidad;
using Microsoft.EntityFrameworkCore;
using Gym.Model;
using System;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Gym.BLL.Servicios;
using Microsoft.AspNetCore.Cors;
using Microsoft.SqlServer.Server;

using OfficeOpenXml;
using System.IO;
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
        //[Authorize]
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

        //[Authorize]
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
                    Telefono = u.Telefono,
                    Direccion = u.Direccion,
                    Cedula = u.Cedula,
                    RolDescripcion = u.RolDescripcion,
                    EsActivo = u.EsActivo,
                    Clave = u.Clave,
                    RefreshToken = u.RefreshToken,
                    RefreshTokenExpiry = u.RefreshTokenExpiry
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
        [Route("ListaClientes")]
        public async Task<IActionResult> ListaClientes()
        {
            var rsp = new Response<List<UsuarioDTO>>();

            try
            {
                rsp.status = true;
                var listaUsuarios = await _usuarioServicio.Lista();

                // Mapear la lista de usuarios a UsuarioDTO excluyendo el campo ImageData
                rsp.value = listaUsuarios
                     .Where(u => u.RolDescripcion == "Clientes")
                    .Select(u => new UsuarioDTO
                {
                    IdUsuario = u.IdUsuario,
                    NombreCompleto = u.NombreCompleto,
                    Correo = u.Correo,
                    IdRol = u.IdRol,
                    Telefono = u.Telefono,
                    Direccion = u.Direccion,
                    Cedula = u.Cedula,
                    //ImageData= u.ImageData,
                    RolDescripcion = u.RolDescripcion,
                    EsActivo = u.EsActivo,
                    Clave = u.Clave,
                    RefreshToken = u.RefreshToken,
                    RefreshTokenExpiry = u.RefreshTokenExpiry
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
            return Ok(new { imageData = producto.ImageData });
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


        //[Authorize]
        [Route("ListaPaginada")]
        [HttpGet]
        public ActionResult<IEnumerable<UsuarioDTO>> GetUsuarios(int page = 1, int pageSize = 5, string searchTerm = null)
        {
            IQueryable<UsuarioDTO> query = _context.Usuarios
                .Where(u => u.IdRol != 3) // Excluir usuarios con IdRol igual a 3 que son clientes
                .Select(u => new UsuarioDTO
                {
                    IdUsuario = u.IdUsuario,
                    NombreCompleto = u.NombreCompleto,
                    Correo = u.Correo,
                    IdRol = u.IdRol,
                    Telefono = u.Telefono,
                    Direccion = u.Direccion,
                    Cedula = u.Cedula,
                    RolDescripcion = u.IdRolNavigation.Nombre, // Obtener la descripción del rol desde la navegación
                    Clave = u.Clave,
                    EsActivo = u.EsActivo.HasValue ? (u.EsActivo.Value ? 1 : 0) : (int?)null,
                    ImageData = u.ImageData,
                    RefreshToken = u.RefreshToken,
                    RefreshTokenExpiry = u.RefreshTokenExpiry
                });

            if (int.TryParse(searchTerm, out int isActive))
            {
                query = query.Where(c =>
                c.Cedula.Contains(searchTerm) ||
                c.EsActivo == isActive
                
                );
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
        [HttpPost]
        [Route("ActualizarImagen/{id:int}")]
       
        public async Task<IActionResult> ActualizarImagen(int id, [FromForm] IFormFile imagen)
        {
            try
            {
                Console.WriteLine($"ID del usuario en ActualizarImagen: {id}");

                // Verifica si la imagen se está recibiendo correctamente
                if (imagen == null || imagen.Length == 0)
                {
                    return BadRequest(new { Mensaje = "La imagen no se recibió correctamente." });
                }

                var usuario = await _context.Usuarios.FirstOrDefaultAsync(p => p.IdUsuario == id);

                if (usuario == null)
                {
                    return NotFound(new { Mensaje = "Usuario no encontrado" });
                }

                // Convierte la imagen a un arreglo de bytes
                using (var memoryStream = new MemoryStream())
                {
                    await imagen.CopyToAsync(memoryStream);
                    usuario.ImageData = memoryStream.ToArray();
                }

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();

                var rsp = new Response<string>
                {
                    status = true,
                    value = "Imagen actualizada correctamente"
                };

                return Ok(rsp);
            }
            catch (Exception ex)
            {
                var rsp = new Response<string>
                {
                    status = false,
                    msg = ex.Message
                };

                return StatusCode(StatusCodes.Status500InternalServerError, rsp);
            }
        }

        //[Authorize]
        [Route("ListaPaginadaCliente")]
        [HttpGet]
        public ActionResult<IEnumerable<UsuarioDTO>> GetClientes(int page = 1, int pageSize = 5, string searchTerm = null)
        {
            IQueryable<UsuarioDTO> query = _context.Usuarios
                .Where(u => u.IdRol != 1 && u.IdRol != 2)
                .Select(u => new UsuarioDTO
                {
                    IdUsuario = u.IdUsuario,
                    NombreCompleto = u.NombreCompleto,
                    Correo = u.Correo,
                    IdRol = u.IdRol,
                    RolDescripcion = u.IdRolNavigation.Nombre, // Obtener la descripción del rol desde la navegación
                    Clave = u.Clave,
                    Telefono = u.Telefono,
                    Direccion = u.Direccion,
                    Cedula = u.Cedula,
                    EsActivo = u.EsActivo.HasValue ? (u.EsActivo.Value ? 1 : 0) : (int?)null,
                    ImageData = u.ImageData,
                    RefreshToken = u.RefreshToken,
                    RefreshTokenExpiry = u.RefreshTokenExpiry
                });

            // Convertir searchTerm a bool si es posible
            if (int.TryParse(searchTerm, out int isActive))
            {
                query = query.Where(c =>
                c.Cedula.Contains(searchTerm) ||
                c.EsActivo == isActive
                
                );
            }


            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c =>
                    c.NombreCompleto.Contains(searchTerm) ||
                    c.RolDescripcion.Contains(searchTerm) ||
                     c.Cedula.Contains(searchTerm) ||
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
        [Route("ObtenerUsuarioPoCedula/{cedula}")]
        public async Task<IActionResult> ObtenerUsuarioPorCedula(string cedula)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Cedula == cedula);

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

    
        private UsuarioDTO ConvertirSesionAUsuario(SesionDTO sesion)
        {
            return new UsuarioDTO
            {
                IdUsuario = sesion.IdUsuario,
                NombreCompleto = sesion.NombreCompleto,
                Correo = sesion.Correo,
                RolDescripcion = sesion.RolDescripcion,
                ImageData = sesion.ImageData,
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
                // Map UsuarioCreateDTO to UsuarioDTO
                var usuarioDTO = new UsuarioDTO
                {
                    NombreCompleto = usuarioCreateDTO.NombreCompleto,
                    Correo = usuarioCreateDTO.Correo,
                    IdRol = 4,
                    Telefono = usuarioCreateDTO.Telefono,
                    Cedula = usuarioCreateDTO.Cedula,
                    Direccion = usuarioCreateDTO.Direccion,
                    Clave = usuarioCreateDTO.Clave,
                    EsActivo = 0,
                    RolDescripcion = "Clientes",
                    ImageData = usuarioCreateDTO.ImageData,

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
                //usuarioExistente.Telefono = usuario.Telefono;
                //usuarioExistente.Direccion = usuario.Direccion;
                //usuarioExistente.Cedula = usuario.Cedula;
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
        //[Authorize]
        [HttpPut]
        [Route("Editar")]
        public async Task<IActionResult> Editar([FromBody] UsuarioDTO usuario)
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

                // URL del GIF de alivio
                string gifUrl = "https://media.giphy.com/media/uWqiZM2vzncysGBon5/giphy.gif";

                // Envía un correo electrónico con la contraseña a cada usuario encontrado
                foreach (var usuario in usuarios)
                {
                    var mensajeHtml = $@"
            <html>
                <body style='font-family: Arial, sans-serif; background-color: #f4f4f9; color: #333; text-align: center; padding: 20px;'>
                    <div style='max-width: 600px; margin: auto; background: white; padding: 20px; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);'>
                        <h2 style='color: #333;'>Recuperación de Contraseña</h2>
                        <p>Hola, <strong>{usuario.NombreCompleto}</strong></p>
                        <p>Tu contraseña es:</p>
                        <p style='font-size: 24px; font-weight: bold; color: #e63946;'>{usuario.Clave}</p>
                        <img src='{gifUrl}' alt='Aliviado' style='width: 100%; max-width: 300px; margin-top: 20px;'/>
                        <p style='margin-top: 20px;'>Si no solicitaste este correo, por favor ignóralo.</p>
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
                var resetUrl = $"https://appsistemaventa2024.web.app/restablecer-contrasena?correo={correo}&token={resetToken}";


                var mensajeHtml = $@"
    <html>
        <body style='font-family: Arial, sans-serif; background-color: #f4f4f9; color: #333; text-align: center; padding: 20px;'>
            <div style='max-width: 600px; margin: auto; background: white; padding: 20px; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);'>
                <h2 style='color: #333;'>Restablecer Contraseña</h2>
                <p>Hola, <strong>{usuario.NombreCompleto}</strong></p>
                <p>Para restablecer tu contraseña, haz clic en el siguiente enlace:</p>
                <a href='{resetUrl}' style='display: inline-block; padding: 10px 20px; margin: 20px 0; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px;'>Restablecer Contraseña</a>
                <p>Si no solicitaste este cambio, por favor ignora este correo.</p>
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
                // Buscar usuarios por correo electrónico
                var usuarios = await _context.Usuarios.Where(u => u.Correo == solicitud.Correo).ToListAsync();
                var usuarioz = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == solicitud.Correo);



                if (usuarios == null || usuarios.Count == 0)
                {
                    rsp.status = false;
                    rsp.msg = "No se encontró un usuario con ese correo electrónico";
                    return Ok(rsp);
                }

                // URL del GIF de alivio
                string gifUrl = "https://media.giphy.com/media/uWqiZM2vzncysGBon5/giphy.gif";


                // Generar token 
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim("idUsuario", usuarioz.IdUsuario.ToString()) }),
                    Expires = DateTime.UtcNow.AddMinutes(10),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var resetToken = tokenHandler.WriteToken(token);
                //var resetToken = _cryptoUtility.EncryptToken(tokenHandler.WriteToken(token));
                var correo = solicitud.Correo;

                //cuando suba esto a un servidor, cambiar la direcion ejemplo $"http://www.ejemplo.com/restablecer-contrasena?correo={correo}&token={resetToken}"
                // Crear URL de restablecimiento de contraseña
                //var resetUrl = $"http://localhost:4200/activar-cuenta?correo={correo}&token={resetToken}";
                var resetUrl = $"https://appsistemaventa2024.web.app/activar-cuenta?correo={correo}&token={resetToken}";

                // Envía un correo electrónico con la contraseña a cada usuario encontrado
                foreach (var usuario in usuarios)
                {
                    var mensajeHtml = $@"
           <html>
        <body style='font-family: Arial, sans-serif; background-color: #f4f4f9; color: #333; text-align: center; padding: 20px;'>
            <div style='max-width: 600px; margin: auto; background: white; padding: 20px; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);'>
                <h2 style='color: #333;'>Activar Usuario</h2>
                <p>Hola, <strong>{usuario.NombreCompleto}</strong></p>
                <p>Para activar el usuario, haz clic en el siguiente enlace:</p>
                <a href='{resetUrl}' style='display: inline-block; padding: 10px 20px; margin: 20px 0; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px;'>Activar Cuenta</a>
                <p>Si no solicitaste esta activacion, por favor ignora este correo.</p>
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
                rsp.value = "Se ha enviado la contraseña a tu correo electrónico.";
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
                worksheet.Cells[1, 7].Value = "Image Data"; // Aquí podrías usar una representación más legible si es necesario
                worksheet.Cells[1, 8].Value = "Refresh Token";
                worksheet.Cells[1, 9].Value = "Refresh Token Expiry";
                worksheet.Cells[1, 10].Value = "Telefono";
                worksheet.Cells[1, 11].Value = "Direccion";
                worksheet.Cells[1, 12].Value = "Cedula";

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
                    worksheet.Cells[row, 7].Value = Convert.ToBase64String(usuario.ImageData ?? Array.Empty<byte>());
                    worksheet.Cells[row, 8].Value = usuario.RefreshToken;
                    worksheet.Cells[row, 9].Value = usuario.RefreshTokenExpiry?.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 10].Value = usuario.Telefono;
                    worksheet.Cells[row, 11].Value = usuario.Direccion;
                    worksheet.Cells[row, 12].Value = usuario.Cedula;
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
                            ImageData = DecodeBase64Image(worksheet.Cells[row, 7].Value?.ToString()),
                            RefreshToken = worksheet.Cells[row, 8].Value?.ToString(),
                            RefreshTokenExpiry = DateTime.TryParse(worksheet.Cells[row, 9].Value?.ToString(), out DateTime expiry) ? (DateTime?)expiry : null,
                            Telefono = worksheet.Cells[row,10].Value?.ToString(),
                            Direccion = worksheet.Cells[row,11].Value?.ToString(),
                            Cedula = worksheet.Cells[row,12].Value?.ToString(),  
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
                            existingUsuario.ImageData = usuario.ImageData;
                            existingUsuario.RefreshToken = usuario.RefreshToken;
                            existingUsuario.RefreshTokenExpiry = usuario.RefreshTokenExpiry;
                            existingUsuario.Telefono = usuario.Telefono;
                            existingUsuario.Direccion = usuario.Direccion;
                            existingUsuario.Cedula = usuario.Cedula;
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

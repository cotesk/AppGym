using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Gym.BLL.Servicios.Contrato;
using Gym.DAL.Repositorios.Contrato;
using Gym.DTO;
using Gym.Model;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;

namespace Gym.BLL.Servicios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _usuarioRepositorio;
        private readonly IMapper _mapper;
        private readonly SmtpClient _smtpClient;
        private readonly string _fromEmail;

        public UsuarioService(IGenericRepository<Usuario> usuarioRepositorio, IMapper mapper)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _mapper = mapper;

        }

        public async Task<List<UsuarioDTO>> Lista()
        {

            try
            {

                var queryUsuario = await _usuarioRepositorio.Consultar();
                var listaUsuarios = queryUsuario.Include(rol => rol.IdRolNavigation).ToList();

                return _mapper.Map<List<UsuarioDTO>>(listaUsuarios);
            }
            catch
            {
                throw;
            }


        }

        public async Task<SesionDTO> ValidadarCredenciales(string correo, string clave)
        {
            try
            {
                // Obtener el usuario por correo
                var usuarioEncontrado = await _usuarioRepositorio.Obtener(p => p.Correo == correo);

                // Verificar si el usuario existe
                if (usuarioEncontrado == null)
                {
                    throw new ArgumentException("El usuario no existe.");
                }

                // Verificar si el usuario está activo
                if (!usuarioEncontrado.EsActivo.HasValue || !usuarioEncontrado.EsActivo.Value)
                {
                    throw new ArgumentException("Este usuario está inactivo.");
                }

                // Validar las credenciales del usuario
                var queryUsuario = await _usuarioRepositorio.Consultar(u =>
                    u.Correo == correo &&
                    u.Clave == clave
                );

                // Verificar si las credenciales son correctas
                var usuario = queryUsuario.FirstOrDefault();
                if (usuario == null)
                {
                    throw new ArgumentException("Clave incorrecta.");
                }

                // Devolver el usuario con su rol incluido
                Usuario devolverUsuario = queryUsuario.Include(rol => rol.IdRolNavigation).First();
                return _mapper.Map<SesionDTO>(devolverUsuario);
            }
            catch (TaskCanceledException ex)
            {
                // Manejar las excepciones relacionadas con la tarea cancelada
                throw new Exception($"Error validando credenciales: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // Manejar cualquier otra excepción
                throw new Exception($"Error inesperado: {ex.Message}", ex);
            }
        }


        public async Task<UsuarioDTO> Crear(UsuarioDTO modelo)
        {
            try
            {


                var usuarioExistente = await _usuarioRepositorio.Obtener(u => u.NombreCompleto == modelo.NombreCompleto);
                if (usuarioExistente != null)
                {
                    throw new ArgumentException("El nombre del usuario ya existe.");
                }

                var usuarioExistentes = await _usuarioRepositorio.Obtener(u => u.Correo == modelo.Correo);
                if (usuarioExistentes != null)
                {
                    throw new ArgumentException("El nombre del correo ya existe.");
                }

                var usuarioCreado = await _usuarioRepositorio.Crear(_mapper.Map<Usuario>(modelo));


                if (usuarioCreado.IdUsuario == 0)
                {
                    throw new TaskCanceledException("No se pudo crear el usuario");



                }
                var query = await _usuarioRepositorio.Consultar(u => u.IdUsuario == usuarioCreado.IdUsuario);
                usuarioCreado = query.Include(rol => rol.IdRolNavigation).First();

                return _mapper.Map<UsuarioDTO>(usuarioCreado);

            }
            catch
            {
                throw;
            }

        }




        public async Task<bool> Editar(UsuarioDTO modelo)
        {
            try
            {




                var usuarioExistente = await _usuarioRepositorio.Obtener(u => u.NombreCompleto == modelo.NombreCompleto && u.IdUsuario != modelo.IdUsuario);
                if (usuarioExistente != null)
                {
                    throw new ArgumentException("El nombre del usuario ya existe.");
                }

                var usuarioExistentes = await _usuarioRepositorio.Obtener(u => u.Correo == modelo.Correo && u.IdUsuario != modelo.IdUsuario);
                if (usuarioExistentes != null)
                {
                    throw new ArgumentException("El nombre del correo ya existe.");
                }

                var usuarioModelo = _mapper.Map<Usuario>(modelo);
                var usuarioEncontrado = await _usuarioRepositorio.Obtener(u => u.IdUsuario == usuarioModelo.IdUsuario);
                if (usuarioEncontrado == null)
                    throw new TaskCanceledException("No se pudo encontrar el usuario");
                //si el usuario existe procede a este paso de editar 
                usuarioEncontrado.NombreCompleto = usuarioModelo.NombreCompleto;
                usuarioEncontrado.Correo = usuarioModelo.Correo;
                usuarioEncontrado.IdRol = usuarioModelo.IdRol;
                usuarioEncontrado.Clave = usuarioModelo.Clave;
                usuarioEncontrado.EsActivo = usuarioModelo.EsActivo;
                usuarioEncontrado.Cedula = usuarioModelo.Cedula;
                usuarioEncontrado.Direccion = usuarioModelo.Direccion;
                usuarioEncontrado.Telefono = usuarioModelo.Telefono;

                bool respuesta = await _usuarioRepositorio.Editar(usuarioEncontrado);


                if (!respuesta)
                    throw new TaskCanceledException("No se pudo editar el usuario");

                return respuesta;

            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> EditarUsuario(UsuarioDTO modelo)
        {
            try
            {

                var usuarioModelo = _mapper.Map<Usuario>(modelo);
                var usuarioEncontrado = await _usuarioRepositorio.Obtener(u => u.IdUsuario == usuarioModelo.IdUsuario);
                if (usuarioEncontrado == null)
                    throw new TaskCanceledException("No se pudo encontrar el usuario");
                //si el usuario existe procede a este paso de editar 
                usuarioEncontrado.NombreCompleto = usuarioModelo.NombreCompleto;
                usuarioEncontrado.Correo = usuarioModelo.Correo;
                usuarioEncontrado.Clave = usuarioModelo.Clave;
                usuarioEncontrado.Cedula = usuarioModelo.Cedula;
                usuarioEncontrado.Direccion = usuarioModelo.Direccion;
                usuarioEncontrado.Telefono = usuarioModelo.Telefono;
                //usuarioEncontrado.ImageData = usuarioModelo.ImageData;
                bool respuesta = await _usuarioRepositorio.Editar(usuarioEncontrado);


                if (!respuesta)
                    throw new TaskCanceledException("No se pudo editar el usuario");

                return respuesta;

            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            try
            {
                var usuarioEncontrado = await _usuarioRepositorio.Obtener(u => u.IdUsuario == id);


                if (usuarioEncontrado == null)
                    throw new TaskCanceledException("No se pudo encontrar el usuario");

                bool respuesta = await _usuarioRepositorio.Eliminar(usuarioEncontrado);


                if (!respuesta)
                    throw new TaskCanceledException("No se pudo eliminar el ususario");

                return respuesta;
            }
            catch
            {
                throw;
            }
        }


        //public UsuarioService(IConfiguration config)
        //{
        //    var host = config["Email:Host"];
        //    var port = int.Parse(config["Email:Port"]);
        //    _fromEmail = config["Email:UserName"];
        //    var password = config["Email:PassWord"];

        //    if (string.IsNullOrEmpty(_fromEmail) || string.IsNullOrEmpty(host) || string.IsNullOrEmpty(password))
        //    {
        //        throw new ArgumentNullException("Email configuration is missing");
        //    }

        //    _smtpClient = new SmtpClient(host, port)
        //    {
        //        Credentials = new NetworkCredential(_fromEmail, password),
        //        EnableSsl = true
        //    };
        //}


        //public async Task EnviarCorreoElectronico(string destinatario, string asunto, string cuerpo)
        //{
        //    var mailMessage = new MailMessage(_fromEmail, destinatario, asunto, cuerpo);
        //    await _smtpClient.SendMailAsync(mailMessage);
        //}




    }
}

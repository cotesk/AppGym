using AutoMapper;
using Gym.BLL.Servicios.Contrato;
using Gym.DAL.Repositorios.Contrato;
using Gym.DTO;
using Gym.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.BLL.Servicios
{
    public class MembresiaService : IMembresiaService
    {

        private readonly IGenericRepository<Membresia> _membresiaRepositorio;
        private readonly IGenericRepository<AsistenciaPersonal> _asignacionesRepositorio;
        private readonly DbgymContext _context;
        private readonly IMapper _mapper;

        public MembresiaService(IGenericRepository<Membresia> membresiaRepositorio, IGenericRepository<AsistenciaPersonal> asignacionesRepositorio, DbgymContext context, IMapper mapper)
        {
            _membresiaRepositorio = membresiaRepositorio;
            _asignacionesRepositorio = asignacionesRepositorio;
            _context = context;
            _mapper = mapper;
        }

        public async Task<MembresiaDTO> Crear(MembresiaDTO modelo)
        {
            try
            {



                var productoCreado = await _membresiaRepositorio.Crear(_mapper.Map<Membresia>(modelo));
                if (productoCreado.IdMembresia == 0)
                    throw new TaskCanceledException("No se pudo crear la categoria");
                return _mapper.Map<MembresiaDTO>(productoCreado);

            }
            catch
            {
                throw;
            }
        }


        public async Task<bool> Editar(MembresiaDTO modelo)
        {
            try
            {
              




                var membresiaExistente = await _membresiaRepositorio.Obtener(u => u.Nombre == modelo.Nombre && u.IdMembresia != modelo.IdMembresia);
            
                if (membresiaExistente != null)
                {
                    throw new ArgumentException("Ya existe una membresia con este nombre.");
                }

                var membresiaModelo = _mapper.Map<Membresia>(modelo);

                // Obtener la membresía existente por ID
                var membresiaEncontrado = await _membresiaRepositorio.Obtener(u => u.IdMembresia == membresiaModelo.IdMembresia);
                if (membresiaEncontrado == null)
                {
                    throw new TaskCanceledException("No se pudo encontrar la membresía.");
                }

                // Actualizar las propiedades del producto existente
                membresiaEncontrado.Nombre = membresiaModelo.Nombre;
                membresiaEncontrado.Descripcion = membresiaModelo.Descripcion;
                membresiaEncontrado.DuracionDias = membresiaModelo.DuracionDias;
                membresiaEncontrado.Precio = membresiaModelo.Precio;
                membresiaEncontrado.EsActivo = membresiaModelo.EsActivo;
      


                bool respuesta = await _membresiaRepositorio.Editar(membresiaEncontrado);


                if (!respuesta)
                    throw new TaskCanceledException("No se pudo editar la membresia");

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


                var membresiaEncontrado = await _membresiaRepositorio.Obtener(p => p.IdMembresia == id);


                if (membresiaEncontrado == null)
                    throw new TaskCanceledException("No se pudo encontrar el producto");

                bool respuesta = await _membresiaRepositorio.Eliminar(membresiaEncontrado);


                if (!respuesta)
                    throw new TaskCanceledException("No se pudo eliminar el producto");

                return respuesta;

            }
            catch (TaskCanceledException ex)
            {

                throw new Exception("Error al encontrar o eliminar el producto", ex);
            }
            catch (Exception ex)
            {
                // Manejar otras excepciones no anticipadas
                throw new Exception("Error desconocido al eliminar el producto", ex);
            }
        }

        public async Task<List<MembresiaDTO>> Lista()
        {
            try
            {
                var queryMembresia = await _membresiaRepositorio.Consultar();

                return _mapper.Map<List<MembresiaDTO>>(queryMembresia.ToList());
            }
            catch
            {
                throw;
            }


        }


        public async Task<bool> AsignarMembresia(AsignacionesMembresiaDTO asignacion)
        {
          

            // Validar si el usuario ya tiene una membresía activa
            var membresiaActiva = await _context.AsignacionesMembresia
                .FirstOrDefaultAsync(m => m.IdUsuario == asignacion.IdUsuario && m.Estado == "Activado");

            if (membresiaActiva != null)
            {
                // No permitir asignar una nueva membresía si hay una activa
                throw new InvalidOperationException("El usuario ya tiene una membresía activa.");
            }

            // Validar si el usuario ya tiene una membresía pendiente
            var membresiaPendiente = await _context.AsignacionesMembresia
                .FirstOrDefaultAsync(m => m.IdUsuario == asignacion.IdUsuario && m.Estado == "Pendiente");

           
            // Calcular la fecha de vencimiento ajustada
            DateTime? fechaVencimiento = null;
            if (!string.IsNullOrEmpty(asignacion.FechaVencimiento))
            {
                // Convertir la fecha proporcionada a DateTime
                var fechaBase = DateTime.Parse(asignacion.FechaVencimiento);

                // Asegurar que la fecha esté en la zona horaria de Colombia
                var zonaHorariaColombia = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                var fechaEnColombia = TimeZoneInfo.ConvertTime(fechaBase, zonaHorariaColombia);

                // Ajustar la hora a las 11:00 PM en Colombia
                fechaEnColombia = fechaEnColombia.Date.AddHours(23);

              
                fechaVencimiento = fechaEnColombia;
            }


            if (membresiaPendiente != null)
            {
                // Actualizar la membresía pendiente existente
                membresiaPendiente.IdMembresia = asignacion.IdMembresia;
                membresiaPendiente.FechaVencimiento = fechaVencimiento;
                membresiaPendiente.Estado = asignacion.Estado;

                _context.AsignacionesMembresia.Update(membresiaPendiente);
            }
            else
            {
                // Crear una nueva asignación de membresía
                var nuevaAsignacion = new AsignacionesMembresia
                {
                    IdUsuario = asignacion.IdUsuario,
                    IdMembresia = asignacion.IdMembresia,
                    FechaVencimiento = fechaVencimiento,
                    Estado = asignacion.Estado
                };

                _context.AsignacionesMembresia.Add(nuevaAsignacion);
            }

            return await _context.SaveChangesAsync() > 0;
        }







    }
}

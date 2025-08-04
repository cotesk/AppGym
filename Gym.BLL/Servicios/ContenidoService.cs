using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


using AutoMapper;
using Azure;
using Gym.DAL.Repositorios.Contrato;
using Gym.DTO;
using Gym.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Gym.BLL.Servicios.Contrato;
using Gym.DAL.Repositorios.Contrato;
using Gym.DTO;
using Gym.Model;


namespace Gym.BLL.Servicios
{
    public class ContenidoService : IContenidoService
    {


        private readonly IGenericRepository<Contenido> _contenidoRepositorio;
        private readonly DbgymContext _context;
        private readonly IMapper _mapper;

        public ContenidoService(IGenericRepository<Contenido> contenidoRepositorio, DbgymContext context, IMapper mapper)
        {
            _contenidoRepositorio = contenidoRepositorio;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ContenidoDTO> Crear(ContenidoDTO modelo)
        {
            try
            {

                var productoCreado = await _contenidoRepositorio.Crear(_mapper.Map<Contenido>(modelo));
                if (productoCreado.IdContenido == 0)
                    throw new TaskCanceledException("No se pudo crear la categoria");
                return _mapper.Map<ContenidoDTO>(productoCreado);

            }
            catch
            {
                throw;
            }
        }


        public async Task<bool> Editar(ContenidoDTO modelo)
        {
            try
            {
                var contenidoaModelo = _mapper.Map<Contenido>(modelo);

                // Obtener el producto existente
                var contenidoEncontrado = await _contenidoRepositorio.Obtener(u => u.IdContenido == contenidoaModelo.IdContenido);
                if (contenidoEncontrado == null)
                    throw new TaskCanceledException("No se pudo encontrar el producto");

                // Actualizar las propiedades del producto existente
                contenidoEncontrado.Comentarios = contenidoaModelo.Comentarios;
                contenidoEncontrado.TipoComentarios = contenidoaModelo.TipoComentarios;
                contenidoEncontrado.TipoContenido = contenidoaModelo.TipoContenido;





                bool respuesta = await _contenidoRepositorio.Editar(contenidoEncontrado);


                if (!respuesta)
                    throw new TaskCanceledException("No se pudo editar el producto");

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


                var productoEncontrado = await _contenidoRepositorio.Obtener(p => p.IdContenido == id);


                if (productoEncontrado == null)
                    throw new TaskCanceledException("No se pudo encontrar el producto");

                bool respuesta = await _contenidoRepositorio.Eliminar(productoEncontrado);


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

        public async Task<List<ContenidoDTO>> Lista()
        {
            try
            {
                var queryEmpresa = await _contenidoRepositorio.Consultar();

                return _mapper.Map<List<ContenidoDTO>>(queryEmpresa.ToList());
            }
            catch
            {
                throw;
            }


        }

    }
}

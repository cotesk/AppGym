using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


using AutoMapper;
using Azure;
using Gym.BLL.Servicios.Contrato;
using Gym.DAL.Repositorios.Contrato;
using Gym.DTO;
using Gym.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Gym.BLL.Servicios.Contrato;
using Gym.DAL.Repositorios.Contrato;



namespace Gym.BLL.Servicios
{
    public class EmpresaService : IEmpresaService
    {


        private readonly IGenericRepository<Empresa> _empresaRepositorio;
        private readonly DbgymContext _context;
        private readonly IMapper _mapper;

        public EmpresaService(IGenericRepository<Empresa> empresaRepositorio, IMapper mapper, DbgymContext context)
        {
            _empresaRepositorio = empresaRepositorio;
            _mapper = mapper;
            _context = context;
        }

        public async Task<EmpresaDTO> Crear(EmpresaDTO modelo)
        {
            try
            {

                var productoCreado = await _empresaRepositorio.Crear(_mapper.Map<Empresa>(modelo));
                if (productoCreado.IdEmpresa == 0)
                    throw new TaskCanceledException("No se pudo crear la categoria");
                return _mapper.Map<EmpresaDTO>(productoCreado);

            }
            catch
            {
                throw;
            }
        }


        public async Task<bool> Editar(EmpresaDTO modelo)
        {
            try
            {
                var empresaModelo = _mapper.Map<Empresa>(modelo);

                // Obtener el producto existente
                var empresaEncontrado = await _empresaRepositorio.Obtener(u => u.IdEmpresa == empresaModelo.IdEmpresa);
                if (empresaEncontrado == null)
                    throw new TaskCanceledException("No se pudo encontrar el producto");

                // Actualizar las propiedades del producto existente
                empresaEncontrado.NombreEmpresa = empresaModelo.NombreEmpresa;
                empresaEncontrado.Direccion = empresaModelo.Direccion;
                empresaEncontrado.Telefono = empresaModelo.Telefono;
                empresaEncontrado.Propietario = empresaModelo.Propietario;
                empresaEncontrado.Correo = empresaModelo.Correo;
                empresaEncontrado.Nit = empresaModelo.Nit;
                empresaEncontrado.Facebook = empresaModelo.Facebook;
                empresaEncontrado.Tiktok = empresaModelo.Tiktok;
                empresaEncontrado.Instagram = empresaModelo.Instagram;



                bool respuesta = await _empresaRepositorio.Editar(empresaEncontrado);


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


                var productoEncontrado = await _empresaRepositorio.Obtener(p => p.IdEmpresa == id);


                if (productoEncontrado == null)
                    throw new TaskCanceledException("No se pudo encontrar el producto");

                bool respuesta = await _empresaRepositorio.Eliminar(productoEncontrado);


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

        public async Task<List<EmpresaDTO>> Lista()
        {
            try
            {
                var queryEmpresa = await _empresaRepositorio.Consultar();

                return _mapper.Map<List<EmpresaDTO>>(queryEmpresa.ToList());
            }
            catch
            {
                throw;
            }


        }




    }


}


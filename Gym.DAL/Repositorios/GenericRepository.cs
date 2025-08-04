using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gym.DAL.Repositorios.Contrato;
using Gym.DAL.Repositorios;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Gym.Model;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.EntityFrameworkCore.Storage;

namespace Gym.DAL.Repositorios
{
    public class GenericRepository<TModelo> : IGenericRepository<TModelo> where TModelo : class
    {

        private readonly DbgymContext _dbcontext;

        public GenericRepository(DbgymContext dbContext)
        {
            _dbcontext = dbContext;
        }


        public async Task<TModelo> Obtener(Expression<Func<TModelo, bool>> filtro)
        {
            try
            {

                TModelo modelo = await _dbcontext.Set<TModelo>().FirstOrDefaultAsync(filtro);
                return modelo;
            }
            catch
            {
                throw;

            }
        }
        public async Task<TModelo> Crear(TModelo modelo)
        {
            try
            {
                _dbcontext.Set<TModelo>().Add(modelo);
                await _dbcontext.SaveChangesAsync();
                return modelo;
            }
            catch
            {
                throw;

            }
        }

        public async Task<bool> Editar(TModelo modelo)
        {
            try
            {
                _dbcontext.Set<TModelo>().Update(modelo);
                await _dbcontext.SaveChangesAsync();
                return true;

            }
            catch
            {
                throw;

            }
        }
        public async Task<bool> Eliminar(TModelo modelo)
        {
            try
            {
                _dbcontext.Set<TModelo>().Remove(modelo);
                await _dbcontext.SaveChangesAsync();
                return true;

            }
            catch
            {
                throw;

            }
        }
        public async Task<IQueryable<TModelo>> Consultar(Expression<Func<TModelo, bool>> filtro = null)
        {
            try
            {
                //original
                //IQueryable<TModelo> queryModelo = filtro == null ? _dbcontext.Set<TModelo>() : _dbcontext.Set<TModelo>().Where(filtro);
                //return queryModelo;
                IQueryable<TModelo> queryModelo = filtro == null ? _dbcontext.Set<TModelo>() : _dbcontext.Set<TModelo>().Where(filtro);
                return await Task.FromResult(queryModelo); // Corrección aquí

            }
            catch
            {
                throw;

            }
        }

        public async Task<TModelo> ObtenerPorId(int id)
        {
            try
            {
                // Realiza la búsqueda del modelo por su ID en la base de datos
                var modelo = await _dbcontext.Set<TModelo>().FindAsync(id);

                // Verifica si el modelo fue encontrado
                if (modelo != null)
                {
                    // Retorna el modelo encontrado
                    return modelo;
                }
                else
                {
                    // Retorna null si el modelo no fue encontrado
                    return null;
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Error inesperado al buscar.", ex);
                throw;
            }
        }


        //public async Task<TModelo> ObtenerPorNombre(string nombre)
        //{
        //    try
        //    {
        //        // Construir la expresión para buscar por nombre
        //        Expression<Func<TModelo, bool>> filtro = ConstructNombreFiltro(nombre);

        //        // Utilizar el método Obtener con el filtro de nombre
        //        return await Obtener(filtro);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error al obtener el modelo por nombre.", ex);
        //    }
        //}

        //// Método privado para construir la expresión de filtro por nombre
        //private Expression<Func<TModelo, bool>> ConstructNombreFiltro(string nombre)
        //{
        //    ParameterExpression parameter = Expression.Parameter(typeof(TModelo), "x");
        //    MemberExpression member = Expression.PropertyOrField(parameter, "Nombre"); // Ajusta "Nombre" al nombre de la propiedad en tu modelo
        //    Expression body = Expression.Equal(member, Expression.Constant(nombre));
        //    return Expression.Lambda<Func<TModelo, bool>>(body, parameter);
        //}


    }
}

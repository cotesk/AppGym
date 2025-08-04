using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Gym.Model;
using Microsoft.EntityFrameworkCore.Storage;

namespace Gym.DAL.Repositorios.Contrato
{
    public interface IGenericRepository<TModel> where TModel : class
    {

        Task<TModel> Obtener(Expression<Func<TModel, bool>> filtro);
        Task<TModel> Crear(TModel modelo);
        Task<bool> Editar(TModel modelo);
        Task<bool> Eliminar(TModel modelo);
        Task<IQueryable<TModel>> Consultar(Expression<Func<TModel, bool>> filtro = null);
        Task<TModel> ObtenerPorId(int id);
        //Task<TModel> ObtenerPorNombre(string nombre);


    }
}

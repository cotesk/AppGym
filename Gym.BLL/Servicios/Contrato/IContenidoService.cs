using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Gym.DTO;
using Microsoft.AspNetCore.Http;

using Gym.Model;

namespace Gym.BLL.Servicios.Contrato
{
    public interface IContenidoService
    {
        Task<List<ContenidoDTO>> Lista();

        Task<ContenidoDTO> Crear(ContenidoDTO modelo);
        Task<bool> Editar(ContenidoDTO modelo);
        Task<bool> Eliminar(int id);



    }
}

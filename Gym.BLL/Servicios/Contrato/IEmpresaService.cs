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
    public interface IEmpresaService
    {

        Task<List<EmpresaDTO>> Lista();

        Task<EmpresaDTO> Crear(EmpresaDTO modelo);
        Task<bool> Editar(EmpresaDTO modelo);
        Task<bool> Eliminar(int id);




    }
}

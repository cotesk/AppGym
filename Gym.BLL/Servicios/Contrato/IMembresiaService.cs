using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gym.DTO;
using Gym.Model;

namespace Gym.BLL.Servicios.Contrato
{
    public interface IMembresiaService
    {


        Task<List<MembresiaDTO>> Lista();
        Task<MembresiaDTO> Crear(MembresiaDTO modelo);
        Task<bool> Editar(MembresiaDTO modelo);
        Task<bool> Eliminar(int id);
        Task<bool> AsignarMembresia(AsignacionesMembresiaDTO asignacion);
       
    }
}

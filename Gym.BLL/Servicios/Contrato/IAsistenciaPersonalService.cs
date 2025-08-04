using Gym.DTO;
using Gym.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.BLL.Servicios.Contrato
{
    public interface IAsistenciaPersonalService
    {
      
        Task<List<AsistenciaPersonalDTO>> ListarAsistencias();
        Task<List<AsistenciaPersonalDTO>> ConsultarAsistenciasPorUsuario(int idUsuario);
    }


}

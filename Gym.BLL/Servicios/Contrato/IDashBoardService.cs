using Gym.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.BLL.Servicios.Contrato
{
    public interface IDashBoardService
    {

        Task<DashBoardDTO> Resumen();

    }
}

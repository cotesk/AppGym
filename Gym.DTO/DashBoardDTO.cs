using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.DTO
{
    public class DashBoardDTO
    {

  
        public string? TotalIngresos { get; set; }

        public int TotalCliente { get; set; }
        public int TotalEntrenadores { get; set; }

        public int TotalUsuarios { get; set; }



        //public decimal TotalGananciasProductos { get; set; }
        public List<PagosSemanaDTO> PagoUltimaSemana { get; set; }
        public List<PagosDoceMesesDTO> PagosDoceMeses { get; set; }

        public List<ClienteAsistenciasDTO> ClientesConMasAsistencias { get; set; }
        public List<ClienteAsistenciasDTO> TotalClientesConMasAsistencias { get; set; }

    }
}

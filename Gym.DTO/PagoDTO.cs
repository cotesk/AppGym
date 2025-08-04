using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.DTO
{
    public class PagoDTO
    {

        public int PagoId { get; set; }

        public int IdUsuario { get; set; }

        public int IdAsistencia { get; set; }

        public string? MontoTexto { get; set; }

        public string? MetodoPago { get; set; }

        public string? TipoPago { get; set; }

        public string? FechaPago { get; set; }

        public string? Observaciones { get; set; }


    }
}

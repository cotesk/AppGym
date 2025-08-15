using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.DTO
{
    public class AsistenciaPersonalDTO
    {


        public int AsistenciaId { get; set; }

        public int? IdUsuario { get; set; }

        public string? NombreUsuario { get; set; }

        public string? FechaAsistencia { get; set; }


        public bool PagoRealizado { get; set; }

        public string? ImagenUrl { get; set; }

    }
}

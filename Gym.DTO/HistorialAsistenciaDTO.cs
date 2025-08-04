using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.DTO
{
    public class HistorialAsistenciaDTO
    {
        public int HistorialId { get; set; }

        public int? IdUsuario { get; set; }

        public string? FechaAsistencia { get; set; }

        public string? Estado { get; set; }

        public string? Comentarios { get; set; }



    }
}

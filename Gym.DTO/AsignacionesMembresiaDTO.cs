using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.DTO
{
    public class AsignacionesMembresiaDTO
    {

        public int AsignacionId { get; set; }

        public int? IdUsuario { get; set; }

        public int? IdMembresia { get; set; }

  
        public string? NombreUsuario { get; set; }

        public string? TelefonoUsuario { get; set; }
        public string? NombreMembresia { get; set; }

        public string? FechaVencimiento { get; set; }

        public string? Estado { get; set; }
        public string? ImagenUrl { get; set; }

    }
}

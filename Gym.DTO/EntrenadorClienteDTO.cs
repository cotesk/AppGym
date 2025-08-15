using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.DTO
{
    public class EntrenadorClienteDTO
    {


        public int AsignacionId { get; set; }

        public int? ClienteId { get; set; }

        public int? EntrenadorId { get; set; }

        public string? NombreEntrenador { get; set; }

        public string? NombreCliente { get; set; }

        public string? ImagenUrlEntrenador { get; set; }

        public string? ImagenUrlCliente { get; set; }

        public string? FechaAsignacion { get; set; }

        public string? FechaFinAsignacion { get; set; }



    }
}

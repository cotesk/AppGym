using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.DTO
{
    public class MembresiaDTO
    {

        public int IdMembresia { get; set; }

        public string Nombre { get; set; } = null!;

        public string? Descripcion { get; set; }

        public int DuracionDias { get; set; }

        public string PrecioTexto { get; set; }



        public int? EsActivo { get; set; }



    }
}

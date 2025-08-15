using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.DTO
{
    public class ActivacionDTO
    {

        public string Correo { get; set; }
        public string Token { get; set; }

        //public int? IdUsuario { get; set; }

        public string? Codigo { get; set; }

    }
}

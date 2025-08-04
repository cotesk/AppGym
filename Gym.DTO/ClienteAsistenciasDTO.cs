using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.DTO
{
    public class ClienteAsistenciasDTO
    {
        public int? IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public byte[]? ImagenData { get; set; } // Datos de la imagen en base64
        public int TotalAsistencias { get; set; }
    }

}

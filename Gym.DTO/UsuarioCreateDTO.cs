using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.DTO
{
    public class UsuarioCreateDTO
    {

        public string NombreCompleto { get; set; }
        public string Correo { get; set; }

        public string Clave { get; set; }

        public string? Telefono { get; set; }

        public string? Cedula { get; set; }

        public string? Direccion { get; set; }

        public byte[]? ImageData { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiry { get; set; }



    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.DTO
{
    public class EmpresaDTO
    {

        public int IdEmpresa { get; set; }

        public string NombreEmpresa { get; set; } = null!;

        public string Direccion { get; set; } = null!;

        public string Telefono { get; set; } = null!;

        public string Propietario { get; set; } = null!;

        public byte[]? Logo { get; set; }

        public string? Correo { get; set; }
        public string? Nit { get; set; }

        public string? Facebook { get; set; }

        public string? Instagram { get; set; }

        public string? Tiktok { get; set; }

        public string? LogoNombre { get; set; }



    }
}

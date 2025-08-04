using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.DTO
{
    public class EmailDTO
    {

        public string Para { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        //public string UrlArchivo { get; set; } = string.Empty;
        //prueba
        public byte[] Adjunto { get; set; } // Agregar propiedad para el archivo adjunto
        public string NombreAdjunto { get; set; } // Agregar propiedad para el nombre del archivo adjunto

        public string TipoMimeAdjunto { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.DTO
{
    public class ContenidoDTO
    {

        public int IdContenido { get; set; }

        public string? Comentarios { get; set; }

        public string? TipoComentarios { get; set; }

        public string? TipoContenido { get; set; }

        public byte[]? Imagenes { get; set; }

        public string? ImagenUrl { get; set; }


        public string? NombreImagen { get; set; }

    }
}

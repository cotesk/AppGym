using System;
using System.Collections.Generic;

namespace Gym.Model;

public partial class Contenido
{
    public int IdContenido { get; set; }

    public string? Comentarios { get; set; }

    public string? TipoComentarios { get; set; }

    public string? TipoContenido { get; set; }

    public byte[]? Imagenes { get; set; }
}

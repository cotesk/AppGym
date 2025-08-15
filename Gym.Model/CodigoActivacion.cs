using System;
using System.Collections.Generic;

namespace Gym.Model;

public partial class CodigoActivacion
{
    public int IdCodigoActivacion { get; set; }

    public int? IdUsuario { get; set; }

    public string? Codigo { get; set; }

    public DateTime? FechaGeneracion { get; set; }

    public DateTime? FechaExpiracion { get; set; }

    public bool? Usado { get; set; }
}

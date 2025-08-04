using System;
using System.Collections.Generic;

namespace Gym.Model;

public partial class Membresia
{
    public int IdMembresia { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int DuracionDias { get; set; }

    public decimal Precio { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? EsActivo { get; set; }

    public virtual ICollection<AsignacionesMembresia> AsignacionesMembresia { get; set; } = new List<AsignacionesMembresia>();
}

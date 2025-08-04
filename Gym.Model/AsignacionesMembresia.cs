using System;
using System.Collections.Generic;

namespace Gym.Model;

public partial class AsignacionesMembresia
{
    public int AsignacionId { get; set; }

    public int? IdUsuario { get; set; }

    public int? IdMembresia { get; set; }

    public DateTime? FechaAsignacion { get; set; }

    public DateTime? FechaVencimiento { get; set; }

    public string? Estado { get; set; }

    public virtual Membresia? IdMembresiaNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}

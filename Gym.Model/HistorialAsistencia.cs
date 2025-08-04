using System;
using System.Collections.Generic;

namespace Gym.Model;

public partial class HistorialAsistencia
{
    public int HistorialId { get; set; }

    public int? IdUsuario { get; set; }

    public DateTime? FechaAsistencia { get; set; }

    public string? Estado { get; set; }

    public string? Comentarios { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}

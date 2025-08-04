using System;
using System.Collections.Generic;

namespace Gym.Model;

public partial class Entrenadores
{
    public int EntrenadorId { get; set; }

    public int? IdUsuario { get; set; }

    public string? Especialidad { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}

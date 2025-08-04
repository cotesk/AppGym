using System;
using System.Collections.Generic;

namespace Gym.Model;

public partial class Menu
{
    public int IdMenu { get; set; }

    public string? Nombre { get; set; }

    public string? Icono { get; set; }

    public string? Url { get; set; }

    public int? IdMenuPadre { get; set; }

    public virtual ICollection<MenuRol> MenuRols { get; set; } = new List<MenuRol>();
}

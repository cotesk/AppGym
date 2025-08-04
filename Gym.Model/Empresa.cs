using System;
using System.Collections.Generic;

namespace Gym.Model;

public partial class Empresa
{
    public int IdEmpresa { get; set; }

    public string NombreEmpresa { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string Propietario { get; set; } = null!;

    public byte[]? Logo { get; set; }

    public string? LogoNombre { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public string? Correo { get; set; }

    public string? Nit { get; set; }

    public string? Facebook { get; set; }

    public string? Instagram { get; set; }

    public string? Tiktok { get; set; }
}

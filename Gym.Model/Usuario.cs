using System;
using System.Collections.Generic;

namespace Gym.Model;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string? NombreCompleto { get; set; }

    public string? Correo { get; set; }

    public int? IdRol { get; set; }

    public string? Clave { get; set; }

    public string? Telefono { get; set; }

    public string? Cedula { get; set; }

    public string? Direccion { get; set; }

    public bool? EsActivo { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public byte[]? ImageData { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiry { get; set; }

    public virtual ICollection<AsignacionesMembresia> AsignacionesMembresia { get; set; } = new List<AsignacionesMembresia>();

    public virtual ICollection<AsignacionesPlanEntrenamiento> AsignacionesPlanEntrenamientos { get; set; } = new List<AsignacionesPlanEntrenamiento>();

    public virtual ICollection<AsistenciaPersonal> AsistenciaPersonals { get; set; } = new List<AsistenciaPersonal>();

    public virtual ICollection<Caja> Cajas { get; set; } = new List<Caja>();

    public virtual ICollection<EntrenadorCliente> EntrenadorClienteClientes { get; set; } = new List<EntrenadorCliente>();

    public virtual ICollection<EntrenadorCliente> EntrenadorClienteEntrenadors { get; set; } = new List<EntrenadorCliente>();

    public virtual ICollection<Entrenadores> Entrenadores { get; set; } = new List<Entrenadores>();

    public virtual ICollection<HistorialAsistencia> HistorialAsistencia { get; set; } = new List<HistorialAsistencia>();

    public virtual ICollection<HistorialPago> HistorialPagos { get; set; } = new List<HistorialPago>();

    public virtual Rol? IdRolNavigation { get; set; }

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}

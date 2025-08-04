using System;
using System.Collections.Generic;

namespace Gym.Model;

public partial class Caja
{
    public int IdCaja { get; set; }

    public DateTime? FechaApertura { get; set; }

    public DateTime? FechaCierre { get; set; }

    public decimal? SaldoInicial { get; set; }

    public decimal? SaldoFinal { get; set; }

    public decimal? Ingresos { get; set; }

    public decimal? Devoluciones { get; set; }

    public decimal? Prestamos { get; set; }

    public decimal? Gastos { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public DateTime? UltimaActualizacion { get; set; }

    public string? MetodoPago { get; set; }

    public int? IdUsuario { get; set; }

    public string? NombreUsuario { get; set; }

    public string? ComentariosGastos { get; set; }

    public string? ComentariosDevoluciones { get; set; }

    public string? Comentarios { get; set; }

    public decimal? Transacciones { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}

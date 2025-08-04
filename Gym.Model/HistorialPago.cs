using System;
using System.Collections.Generic;

namespace Gym.Model;

public partial class HistorialPago
{
    public int HistorialPagoId { get; set; }

    public int? PagoId { get; set; }

    public int? IdUsuario { get; set; }

    public DateTime? FechaPago { get; set; }

    public decimal? Monto { get; set; }

    public string? TipoPago { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual Pago? Pago { get; set; }
}

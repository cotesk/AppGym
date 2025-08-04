using System;
using System.Collections.Generic;

namespace Gym.Model;

public partial class Pago
{
    public int PagoId { get; set; }

    public int IdUsuario { get; set; }

    public decimal? Monto { get; set; }

    public string? MetodoPago { get; set; }

    public string? TipoPago { get; set; }

    public DateTime? FechaPago { get; set; }

    public string? Observaciones { get; set; }

    public virtual ICollection<HistorialPago> HistorialPagos { get; set; } = new List<HistorialPago>();

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace Gym.Model;

public partial class PlanesDeEntrenamiento
{
    public int PlanId { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public int? Duracion { get; set; }

    public string? TipoPlan { get; set; }

    public virtual ICollection<AsignacionesPlanEntrenamiento> AsignacionesPlanEntrenamientos { get; set; } = new List<AsignacionesPlanEntrenamiento>();
}

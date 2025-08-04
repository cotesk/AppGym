using System;
using System.Collections.Generic;

namespace Gym.Model;

public partial class AsignacionesPlanEntrenamiento
{
    public int AsignacionId { get; set; }

    public int? ClienteId { get; set; }

    public int? PlanId { get; set; }

    public DateTime? FechaAsignacion { get; set; }

    public DateTime? FechaFinAsignacion { get; set; }

    public virtual Usuario? Cliente { get; set; }

    public virtual PlanesDeEntrenamiento? Plan { get; set; }
}

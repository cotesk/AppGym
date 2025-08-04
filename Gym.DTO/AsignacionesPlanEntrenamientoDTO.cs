using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.DTO
{
    public class AsignacionesPlanEntrenamientoDTO
    {

        public int AsignacionId { get; set; }

        public int? ClienteId { get; set; }

        public int? PlanId { get; set; }

        public string? FechaAsignacion { get; set; }

        public string? FechaFinAsignacion { get; set; }

    }
}

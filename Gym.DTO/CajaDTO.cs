using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.DTO
{
    public class CajaDTO
    {

        public int IdCaja { get; set; }

        public DateTime? FechaApertura { get; set; }

        public DateTime? FechaCierre { get; set; }

        public string? SaldoInicialTexto { get; set; }

        public string? SaldoFinalTexto { get; set; }

        public string? IngresosTexto { get; set; }

        public string? DevolucionesTexto { get; set; }

        public string? PrestamosTexto { get; set; }

        public string? GastosTexto { get; set; }
        public string? TransaccionesTexto { get; set; }

        public string? Estado { get; set; }

        public string? Comentarios { get; set; }

        public DateTime? FechaRegistro { get; set; }

        public DateTime? UltimaActualizacion { get; set; }

        public string? MetodoPago { get; set; }


        public int? IdUsuario { get; set; }

        public string? NombreUsuario { get; set; }


        public string? ComentariosGastos { get; set; }

        public string? ComentariosDevoluciones { get; set; }




    }
}

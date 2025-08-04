using Gym.DTO;
using Gym.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.BLL.Servicios.Contrato
{
    public interface IPagoService
    {

        Task<(Pago Pago, string Mensaje)> RegistrarPago(PagoDTO pagoDto);
        Task<(Pago Pago, string Mensaje)> RegistrarPagoCalendario(PagoDTO pagoDto);

        Task<List<HistorialPagoDTO>> ObtenerPagosPorUsuario(int idUsuario); // Ajustado para que sea List<HistorialPago>
        Task<IEnumerable<HistorialPagoDTO>> ObtenerTodosLosPagos();

    }
}

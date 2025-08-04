using Gym.DTO;
using Gym.Model;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gym.BLL.Servicios.Contrato
{
    public interface ICajaService
    {

        Task<List<CajaDTO>> Lista();
        Task<CajaDTO> ObtenerCajaPorId(int idCaja);
        Task<CajaDTO> Crear(CajaDTO modelo);
        Task<bool> Editar(CajaDTO modelo);
        Task<bool> Eliminar(int id);
        Task<bool> CambiarEstado(int id);
        Task<Caja> ObtenerCajaPorUsuario(int idUsuario);
        //Task<Caja> ObtenerCajaPoridCaja(int idCaja);

        Task<bool> EditarIngreso(CajaDTO modelo);
        Task<bool> EditarGastos(CajaDTO modelo);
        Task<bool> EditarDevoluciones(CajaDTO modelo);
        Task<bool> EditarDevolucionesGasto(CajaDTO modelo);
        Task<bool> Prestamo(int idCaja, decimal prestamo, string comentarios, string estado);
        Task<bool> PagoDelPrestamo(int idCaja, decimal prestamo, string comentarios, string estado);

        Task<bool> Gastos(int idCaja, decimal gastos, string comentarios, string estado);
        Task<bool> PagoDevolucion(int idCaja, decimal gastos, string comentarios, string estado);

        Task<bool> Gasto(int idCaja, string numeroDocumentoCompra, string comentariosGastos);
        Task<bool> Devoluciones(int idCaja, string numeroDocumento, string comentariosDevoluciones, string estado);
        Task<bool> DevolucionesCotizaciones(int idCaja, string numeroDocumento, string comentariosDevoluciones, string estado);
    }
}

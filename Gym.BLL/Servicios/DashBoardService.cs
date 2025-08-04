using AutoMapper;
using Gym.BLL.Servicios.Contrato;
using Gym.DAL.Repositorios.Contrato;
using Gym.DTO;
using Gym.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.IO.RecyclableMemoryStreamManager;

namespace Gym.BLL.Servicios
{
    public class DashBoardService : IDashBoardService
    {


       
        private readonly IGenericRepository<Pago> _pagoRepositorio;
        private readonly IGenericRepository<Usuario> _usuarioRepositorio;
        private readonly IGenericRepository<AsistenciaPersonal> _asistenciaRepositorio;

        private readonly IMapper _mapper;

        public DashBoardService(IGenericRepository<Pago> pagoRepositorio, IGenericRepository<Usuario> usuarioRepositorio, IGenericRepository<AsistenciaPersonal> asistenciaRepositorio, IMapper mapper)
        {
            _pagoRepositorio = pagoRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _asistenciaRepositorio = asistenciaRepositorio;
            _mapper = mapper;
        }

        private async Task<int> TotalPagoAno()
        {
            int total = 0;
            IQueryable<Pago> _compraQuery = await _pagoRepositorio.Consultar();

            if (_compraQuery.Any())
            {
                var tablaCompra = retornarPagoAnio(_compraQuery, -365); // Últimos aproximadamente 365 días
                total = tablaCompra.Count(); // Contar todas las ventas no anuladas en el último año
            }

            return total;
        }

        private async Task<string> TotalIngresoUltimoMes()
        {
            decimal resultado = 0;
            IQueryable<Pago> _ventaQuery = await _pagoRepositorio.Consultar();
            if (_ventaQuery.Count() > 0)
            {
                var tablaVenta = retornarPagos(_ventaQuery, -31);

                // Excluir ingresos de las ventas anuladas
                resultado = tablaVenta
                 
                  .Select(v => v.Monto)
                  .Sum(v => v.Value);
            }

            return Convert.ToString(resultado, new CultureInfo("es-CO"));
        }

        private IQueryable<Pago> retornarPagoAnio(IQueryable<Pago> tablacompra, int restarCantidadDias)
        {
            DateTime fechaLimite = DateTime.Now.AddDays(restarCantidadDias); // Restar aproximadamente un año

            return tablacompra.Where(v => v.FechaPago >= fechaLimite);
        }



        private async Task<int> TotalUsuario()
        {

            IQueryable<Usuario> _productoQuery = await _usuarioRepositorio.Consultar();
            //int total = _productoQuery.Count();
            int total = _productoQuery.Where(b => b.EsActivo == true).Count();
            return total;

        }
        private async Task<int> TotalCliente()
        {

            IQueryable<Usuario> _productoQuery = await _usuarioRepositorio.Consultar();
            /* int total = _productoQuery.Count();*/
            int total = _productoQuery.Where(b => b.EsActivo == true && b.IdRolNavigation.Nombre=="Clientes").Count();
            return total;

        }

        private async Task<int> TotalEntrenadores()
        {

            IQueryable<Usuario> _productoQuery = await _usuarioRepositorio.Consultar();
            /* int total = _productoQuery.Count();*/
            int total = _productoQuery.Where(b => b.EsActivo == true && b.IdRolNavigation.Nombre == "Entrenador").Count();
            return total;

        }

        private IQueryable<Pago> retornarPagos(IQueryable<Pago> tablaVenta, int restarCantidadDias)
        {

            DateTime? ultimaFecha = tablaVenta.OrderByDescending(v => v.FechaPago).Select(v => v.FechaPago).First();
            ultimaFecha = ultimaFecha.Value.AddDays(restarCantidadDias);

            return tablaVenta.Where(v => v.FechaPago.Value.Date >= ultimaFecha.Value.Date);


        }


        private async Task<Dictionary<string, int>> PagosUltimoMes()
        {
            Dictionary<string, int> resultado = new Dictionary<string, int>();
            IQueryable<Pago> _ventaQuery = await _pagoRepositorio.Consultar();

            if (_ventaQuery.Any())
            {
                var tablaVenta = retornarPagos(_ventaQuery, -31);

                resultado = tablaVenta
                  
                    .GroupBy(v => v.FechaPago.Value.Date)
                    .OrderBy(g => g.Key)
                    .Select(dv => new { fecha = dv.Key.ToString("dd/MM/yyyy"), total = dv.Count() })
                    .ToDictionary(r => r.fecha, r => r.total);
            }

            return resultado;
        }
        private async Task<Dictionary<string, int>> PagosUltimosDoceMeses()
        {
            Dictionary<string, int> resultado = new Dictionary<string, int>();
            IQueryable<Pago> _ventaQuery = await _pagoRepositorio.Consultar();

            if (_ventaQuery.Any())
            {
                var tablaVenta = retornarPagos(_ventaQuery, -365); // Últimos aproximadamente 365 días

                resultado = tablaVenta
                  
                    .GroupBy(v => new { Year = v.FechaPago.Value.Year, Month = v.FechaPago.Value.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .Select(dv => new
                    {
                        fecha = $"{dv.Key.Month}/{dv.Key.Year}", // Formato mes/año
                        total = dv.Count()
                    })
                    .ToDictionary(r => r.fecha, r => r.total);
            }

            return resultado;
        }

        private async Task<List<ClienteAsistenciasDTO>> ObtenerClientesConMasAsistencias()
        {
            // Obtener todas las asistencias y agrupar por usuario
            var asistenciasQuery = await _asistenciaRepositorio.Consultar();
            var usuariosQuery = await _usuarioRepositorio.Consultar();

            if (asistenciasQuery == null || usuariosQuery == null)
            {
                return new List<ClienteAsistenciasDTO>();
            }

            var topClientes = asistenciasQuery
                .Where(a => a.IdUsuario.HasValue && a.PagoRealizado == true) // Filtrar solo asistencias con usuario válido
                .GroupBy(a => a.IdUsuario)
                .Select(g => new
                {
                    IdUsuario = g.Key,
                    TotalAsistencias = g.Count()
                })
                .OrderByDescending(x => x.TotalAsistencias)
                .Take(3) // Los 3 clientes con más asistencias
                .ToList();

            // Mapear usuarios y asistencias
            var clientesConMasAsistencias = topClientes
                .Join(
                    usuariosQuery, // Relacionar con usuarios
                    asistencia => asistencia.IdUsuario,
                    usuario => usuario.IdUsuario,
                    (asistencia, usuario) => new ClienteAsistenciasDTO
                    {
                        IdUsuario = usuario.IdUsuario,
                        Nombre = usuario.NombreCompleto,
                        //Apellido = usuario.Apellido,
                        Email = usuario.Correo,
                        ImagenData = usuario.ImageData,
                        TotalAsistencias = asistencia.TotalAsistencias
                    }
                )
                .ToList();

            return clientesConMasAsistencias;
        }

        private async Task<List<ClienteAsistenciasDTO>> ObtenerTotalClientesConMasAsistencias()
        {
            // Obtener todas las asistencias y agrupar por usuario
            var asistenciasQuery = await _asistenciaRepositorio.Consultar();
            var usuariosQuery = await _usuarioRepositorio.Consultar();

            if (asistenciasQuery == null || usuariosQuery == null)
            {
                return new List<ClienteAsistenciasDTO>();
            }

            var topClientes = asistenciasQuery
                .Where(a => a.IdUsuario.HasValue && a.PagoRealizado ==true) // Filtrar solo asistencias con usuario válido
                .GroupBy(a => a.IdUsuario)
                .Select(g => new
                {
                    IdUsuario = g.Key,
                    TotalAsistencias = g.Count()
                })
                .OrderByDescending(x => x.TotalAsistencias)
                .Take(90000) // Los 90000 clientes con más asistencias
                .ToList();

            // Mapear usuarios y asistencias
            var clientesConMasAsistencias = topClientes
                .Join(
                    usuariosQuery, // Relacionar con usuarios
                    asistencia => asistencia.IdUsuario,
                    usuario => usuario.IdUsuario,
                    (asistencia, usuario) => new ClienteAsistenciasDTO
                    {
                        IdUsuario = usuario.IdUsuario,
                        Nombre = usuario.NombreCompleto,
                        //Apellido = usuario.Apellido,
                        Email = usuario.Correo,
                        ImagenData = usuario.ImageData,
                        TotalAsistencias = asistencia.TotalAsistencias
                    }
                )
                .ToList();

            return clientesConMasAsistencias;
        }






        public async Task<DashBoardDTO> Resumen()
        {

            DashBoardDTO vmDashBoard = new DashBoardDTO();

            try
            {


                vmDashBoard.TotalIngresos = await TotalIngresoUltimoMes();
                vmDashBoard.TotalCliente = await TotalCliente();
                vmDashBoard.TotalEntrenadores = await TotalEntrenadores();
                vmDashBoard.TotalUsuarios = await TotalUsuario();



                // Agregar los 3 clientes con más asistencias al resumen
                var clientesConMasAsistencias = await ObtenerClientesConMasAsistencias();
                vmDashBoard.ClientesConMasAsistencias = clientesConMasAsistencias;


                var TotalclientesConMasAsistencias = await ObtenerTotalClientesConMasAsistencias();
                vmDashBoard.TotalClientesConMasAsistencias = TotalclientesConMasAsistencias;

                List<PagosSemanaDTO> listaVentaSemana = new List<PagosSemanaDTO>();
                List<PagosDoceMesesDTO> listaVentaDoceMeses = new List<PagosDoceMesesDTO>();
   

              

                foreach (KeyValuePair<string, int> item in await PagosUltimoMes())
                {

                    listaVentaSemana.Add(new PagosSemanaDTO()
                    {
                        Fecha = item.Key,
                        Total = item.Value




                    });
                    vmDashBoard.PagoUltimaSemana = listaVentaSemana;
                }
                foreach (KeyValuePair<string, int> item in await PagosUltimosDoceMeses())
                {

                    listaVentaDoceMeses.Add(new PagosDoceMesesDTO()
                    {
                        Fecha = item.Key,
                        Total = item.Value




                    });
                    vmDashBoard.PagosDoceMeses = listaVentaDoceMeses;
                }

            

                return vmDashBoard;
            }
            catch
            {

                throw;
            }
        }

       
    }
}

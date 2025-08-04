using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Gym.BLL.Servicios.Contrato;
using Gym.DAL.Repositorios.Contrato;
using Gym.DTO;
using Gym.Model;

namespace Gym.BLL.Servicios
{
    public class CajaService : ICajaService
    {
        private readonly IGenericRepository<Caja> _cajaRepositorio;
        private readonly IMapper _mapper;
        private readonly DbgymContext _context;

        public CajaService(IGenericRepository<Caja> cajaRepositorio, IMapper mapper, DbgymContext context)
        {
            _cajaRepositorio = cajaRepositorio;
            _mapper = mapper;
            _context = context;
        }

        public async Task<CajaDTO> Crear(CajaDTO modelo)
        {
            try
            {
                // Obtener la zona horaria local (por ejemplo, para Colombia: "SA Pacific Standard Time")
                TimeZoneInfo zonaHorariaLocal = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

                // Convertir la fecha actual de UTC a la zona horaria local
                DateTime fechaAperturaLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, zonaHorariaLocal);

                // Establecer la fecha de apertura en la zona horaria local
                modelo.FechaApertura = fechaAperturaLocal;
                modelo.FechaRegistro = fechaAperturaLocal;

                // Establecer el estado como "Abierto"
                modelo.Estado = "Abierto";

                var cajaCreado = await _cajaRepositorio.Crear(_mapper.Map<Caja>(modelo));
                if (cajaCreado.IdCaja == 0)
                    throw new TaskCanceledException("No se pudo crear la caja");
                return _mapper.Map<CajaDTO>(cajaCreado);

            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> EditarIngreso(CajaDTO modelo)
        {
            try
            {
                var cajaModelo = _mapper.Map<Caja>(modelo);
                var cajaEncontrada = await _cajaRepositorio.Obtener(u => u.IdCaja == cajaModelo.IdCaja);
                if (cajaEncontrada == null)
                    throw new TaskCanceledException("No se pudo encontrar la caja");

                Console.WriteLine("caja antes de guardar: " + cajaEncontrada);

                // Actualizar las propiedades de la caja existente
                // Verificar si el método de pago es "Efectivo"
                if (cajaModelo.MetodoPago == "Efectivo")
                {
                    // Actualizar las propiedades de la caja existente
                    cajaEncontrada.Ingresos += cajaModelo.Ingresos;
                    cajaEncontrada.Estado = cajaModelo.Estado;
                    cajaEncontrada.IdUsuario = cajaModelo.IdUsuario;
                    cajaEncontrada.MetodoPago = cajaModelo.MetodoPago;

                    // Editar la caja
                    bool respuesta = await _cajaRepositorio.Editar(cajaEncontrada);

                    if (!respuesta)
                        throw new TaskCanceledException("No se pudo editar la caja");

                    return respuesta;
                }
                else
                {

                    cajaEncontrada.Transacciones += cajaModelo.Transacciones;
                    cajaEncontrada.Estado = cajaModelo.Estado;
                    cajaEncontrada.IdUsuario = cajaModelo.IdUsuario;
                    cajaEncontrada.MetodoPago = cajaModelo.MetodoPago;

                    // Editar la caja
                    bool respuesta = await _cajaRepositorio.Editar(cajaEncontrada);

                    if (!respuesta)
                        throw new TaskCanceledException("No se pudo editar la caja");

                    return respuesta;

                }

            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> EditarGastos(CajaDTO modelo)
        {
            try
            {
                var cajaModelo = _mapper.Map<Caja>(modelo);
                var cajaEncontrada = await _cajaRepositorio.Obtener(u => u.IdCaja == cajaModelo.IdCaja);
                if (cajaEncontrada == null)
                    throw new TaskCanceledException("No se pudo encontrar la caja");

                Console.WriteLine("caja antes de guardar: " + cajaEncontrada);

                // Actualizar las propiedades de la caja existente
                if (cajaModelo.MetodoPago == "Efectivo")
                {
                    cajaEncontrada.Gastos += cajaModelo.Gastos;

                    cajaEncontrada.Estado = cajaModelo.Estado;
                    cajaEncontrada.IdUsuario = cajaModelo.IdUsuario;



                    bool respuesta = await _cajaRepositorio.Editar(cajaEncontrada);


                    if (!respuesta)
                        throw new TaskCanceledException("No se pudo editar la caja");

                    return respuesta;

                }
                else
                {

                    cajaEncontrada.Transacciones -= cajaModelo.Transacciones;

                    cajaEncontrada.Estado = cajaModelo.Estado;
                    cajaEncontrada.IdUsuario = cajaModelo.IdUsuario;



                    bool respuesta = await _cajaRepositorio.Editar(cajaEncontrada);


                    if (!respuesta)
                        throw new TaskCanceledException("No se pudo editar la caja");

                    return respuesta;

                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> EditarDevoluciones(CajaDTO modelo)
        {
            try
            {
                var cajaModelo = _mapper.Map<Caja>(modelo);
                var cajaEncontrada = await _cajaRepositorio.Obtener(u => u.IdCaja == cajaModelo.IdCaja);
                if (cajaEncontrada == null)
                    throw new TaskCanceledException("No se pudo encontrar la caja");

                Console.WriteLine("caja antes de guardar: " + cajaEncontrada);

                // Actualizar las propiedades de la caja existente
                if (cajaModelo.MetodoPago == "Efectivo")
                {

                    cajaEncontrada.Devoluciones += cajaModelo.Devoluciones;
                    //cajaEncontrada.Ingresos -= cajaModelo.Ingresos;

                    cajaEncontrada.IdUsuario = cajaModelo.IdUsuario;



                    bool respuesta = await _cajaRepositorio.Editar(cajaEncontrada);


                    if (!respuesta)
                        throw new TaskCanceledException("No se pudo editar la caja");

                    return respuesta;
                }
                else
                {

                    //cajaEncontrada.Devoluciones += cajaModelo.Devoluciones;
                    //cajaEncontrada.Ingresos -= cajaModelo.Ingresos;
                    //cajaEncontrada.Transacciones -= cajaModelo.Transacciones;
                    cajaEncontrada.Devoluciones += cajaModelo.Devoluciones;
                    cajaEncontrada.IdUsuario = cajaModelo.IdUsuario;



                    bool respuesta = await _cajaRepositorio.Editar(cajaEncontrada);


                    if (!respuesta)
                        throw new TaskCanceledException("No se pudo editar la caja");

                    return respuesta;

                }



            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> EditarDevolucionesGasto(CajaDTO modelo)
        {
            try
            {
                var cajaModelo = _mapper.Map<Caja>(modelo);
                var cajaEncontrada = await _cajaRepositorio.Obtener(u => u.IdCaja == cajaModelo.IdCaja);
                if (cajaEncontrada == null)
                    throw new TaskCanceledException("No se pudo encontrar la caja");

                Console.WriteLine("caja antes de guardar: " + cajaEncontrada);

                // Actualizar las propiedades de la caja existente


                if (cajaModelo.MetodoPago == "Efectivo")
                {

                    cajaEncontrada.Gastos -= cajaModelo.Gastos;


                    cajaEncontrada.IdUsuario = cajaModelo.IdUsuario;



                    bool respuesta = await _cajaRepositorio.Editar(cajaEncontrada);


                    if (!respuesta)
                        throw new TaskCanceledException("No se pudo editar la caja");

                    return respuesta;

                }
                else
                {

                    cajaEncontrada.Transacciones += cajaModelo.Transacciones;


                    cajaEncontrada.IdUsuario = cajaModelo.IdUsuario;



                    bool respuesta = await _cajaRepositorio.Editar(cajaEncontrada);


                    if (!respuesta)
                        throw new TaskCanceledException("No se pudo editar la caja");

                    return respuesta;


                }



            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Editar(CajaDTO modelo)
        {
            try
            {
                var cajaModelo = _mapper.Map<Caja>(modelo);
                var cajaEncontrada = await _cajaRepositorio.Obtener(u => u.IdCaja == cajaModelo.IdCaja);
                if (cajaEncontrada == null)
                    throw new TaskCanceledException("No se pudo encontrar la caja");

                Console.WriteLine("caja antes de guardar: " + cajaEncontrada);

                // Actualizar las propiedades de la caja existente
                cajaEncontrada.FechaApertura = cajaModelo.FechaApertura;
                cajaEncontrada.FechaCierre = DateTime.Now; // Establecer la fecha de cierre como la fecha actual
                cajaEncontrada.SaldoInicial = cajaModelo.SaldoInicial;
                cajaEncontrada.SaldoFinal = cajaModelo.SaldoFinal;
                cajaEncontrada.Ingresos = cajaModelo.Ingresos;
                cajaEncontrada.Devoluciones = cajaModelo.Devoluciones;
                cajaEncontrada.Prestamos = cajaModelo.Prestamos;
                cajaEncontrada.Gastos = cajaModelo.Gastos;
                cajaEncontrada.Estado = "Cerrado";
                cajaEncontrada.IdUsuario = cajaModelo.IdUsuario;
                cajaEncontrada.Comentarios = cajaModelo.Comentarios;
                cajaEncontrada.FechaRegistro = DateTime.Now;
                cajaEncontrada.UltimaActualizacion = DateTime.Now; // Actualizar la fecha de última actualización
                cajaEncontrada.MetodoPago = cajaModelo.MetodoPago;

                bool respuesta = await _cajaRepositorio.Editar(cajaEncontrada);


                if (!respuesta)
                    throw new TaskCanceledException("No se pudo editar la caja");

                return respuesta;

            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            try
            {
                var cajaEncontrado = await _cajaRepositorio.Obtener(p => p.IdCaja == id);


                if (cajaEncontrado == null)
                    throw new TaskCanceledException("No se pudo encontrar la caja");

                bool respuesta = await _cajaRepositorio.Eliminar(cajaEncontrado);


                if (!respuesta)
                    throw new TaskCanceledException("No se pudo eliminar la caja");

                return respuesta;

            }
            catch
            {
                throw;
            }
        }

        public async Task<List<CajaDTO>> Lista()
        {
            try
            {

                var ListaCaja = await _cajaRepositorio.Consultar();
                var listaProducto = ListaCaja.Include(cat => cat.IdUsuarioNavigation).ToList();
                return _mapper.Map<List<CajaDTO>>(ListaCaja.ToList());
            }
            catch
            {
                throw;
            }
        }

        public async Task<CajaDTO> ObtenerCajaPorId(int idCaja)
        {
            try
            {
                var caja = await _cajaRepositorio.Obtener(c => c.IdCaja == idCaja);
                return _mapper.Map<CajaDTO>(caja);
            }
            catch (Exception ex)
            {
                // Manejo de errores, por ejemplo, loguear el error
                throw new Exception("Error al obtener la caja por ID", ex);
            }
        }


        public async Task<bool> CambiarEstado(int id)
        {
            try
            {
                // Encuentra la caja por su ID
                var caja = await _context.Cajas.FindAsync(id);

                if (caja == null)
                {
                    return false; // La caja no existe
                }
                else
                {

                    // Calcula el saldo final como la suma del saldo inicial y los ingresos
                    caja.SaldoFinal = caja.SaldoInicial + caja.Ingresos;
                    var suma = caja.SaldoFinal - caja.Prestamos;
                    caja.SaldoFinal = suma;
                    var gasto = caja.SaldoFinal - caja.Gastos;
                    caja.SaldoFinal = gasto;
                    var devo = caja.SaldoFinal - caja.Devoluciones;
                    caja.SaldoFinal = devo;

                    // Obtener la zona horaria local (por ejemplo, para Colombia: "SA Pacific Standard Time")
                    TimeZoneInfo zonaHorariaLocal = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

                    // Convertir la fecha actual de UTC a la zona horaria local
                    DateTime fechaAperturaLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, zonaHorariaLocal);

                    // Establecer la fecha de apertura en la zona horaria local             
                    // Cambia el estado de la caja
                    caja.FechaCierre = fechaAperturaLocal;
                    caja.Estado = "Cerrada";

                    // Guarda los cambios en la base de datos
                    await _context.SaveChangesAsync();

                    return true; // El estado de la caja se cambió correctamente

                }



            }
            catch (Exception)
            {
                // Manejo de errores, por ejemplo, loguear el error
                return false; // No se pudo cambiar el estado de la caja
            }
        }

        public async Task<Caja> ObtenerCajaPorUsuario(int idUsuario)
        {
            try
            {

                // Implementa aquí la lógica para obtener la caja por usuario
                var caja = await _context.Set<Caja>()
                    .Where(c => c.IdUsuario == idUsuario)  // Filtrar por idUsuario
                    .FirstOrDefaultAsync(c => c.Estado == "Abierto");  // Asegurarse de que la caja esté abierta


                return caja;
            }
            catch (Exception ex)
            {
                // Manejo de errores
                throw new Exception("Error al obtener la caja por usuario", ex);
            }
        }



        public async Task<Caja> ObtenerCajaPoridCaja(int idCaja)
        {
            try
            {

                // Implementa aquí la lógica para obtener la caja por usuario
                var caja = await _context.Set<Caja>()
                    .Where(c => c.IdCaja == idCaja)  // Filtrar por idUsuario
                    .FirstOrDefaultAsync(c => c.Estado == "Abierto");  // Asegurarse de que la caja esté abierta




                return caja;
            }
            catch (Exception ex)
            {
                // Manejo de errores
                throw new Exception("Error al obtener la caja por usuario", ex);
            }
        }


        public async Task<bool> Gasto(int idCaja, string numeroDocumentoCompra, string comentariosGastos)
        {
            try
            {
                // Encuentra la caja por su ID
                var caja = await _context.Cajas.FindAsync(idCaja);

                if (caja == null)
                    return false; // La caja no existe



                // Concatena el nuevo comentario con los comentarios anteriores, si los hay
                caja.ComentariosGastos += ConcatenarComentarioIdCompra(caja.ComentariosGastos, comentariosGastos, numeroDocumentoCompra);

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();

                return true; // El préstamo se agregó correctamente a la caja
            }
            catch (Exception)
            {
                // Manejo de errores, por ejemplo, loguear el error
                return false; // No se pudo agregar el préstamo a la caja
            }
        }
        public async Task<bool> Devoluciones(int idCaja, string numeroDocumento, string comentariosDevoluciones, string estado)
        {
            try
            {
                // Encuentra la caja por su ID
                var caja = await _context.Cajas.FindAsync(idCaja);

                if (caja == null)
                    return false; // La caja no existe

                // Agrega el préstamo al saldo total


                // Concatena el nuevo comentario con los comentarios anteriores, si los hay
                caja.ComentariosDevoluciones += ConcatenarComentarioIdVenta(caja.ComentariosDevoluciones, comentariosDevoluciones, numeroDocumento, estado);

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();

                return true; // El préstamo se agregó correctamente a la caja
            }
            catch (Exception)
            {
                // Manejo de errores, por ejemplo, loguear el error
                return false; // No se pudo agregar el préstamo a la caja
            }
        }

        public async Task<bool> DevolucionesCotizaciones(int idCaja, string numeroDocumento, string comentariosDevoluciones, string estado)
        {
            try
            {
                // Encuentra la caja por su ID
                var caja = await _context.Cajas.FindAsync(idCaja);

                if (caja == null)
                    return false; // La caja no existe

                // Agrega el préstamo al saldo total


                // Concatena el nuevo comentario con los comentarios anteriores, si los hay
                caja.ComentariosDevoluciones += ConcatenarComentarioIdCompra(caja.ComentariosDevoluciones, comentariosDevoluciones, numeroDocumento);

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();

                return true; // El préstamo se agregó correctamente a la caja
            }
            catch (Exception)
            {
                // Manejo de errores, por ejemplo, loguear el error
                return false; // No se pudo agregar el préstamo a la caja
            }
        }
        public async Task<bool> Prestamo(int idCaja, decimal prestamo, string comentarios, string estado)
        {
            try
            {
                // Encuentra la caja por su ID
                var caja = await _context.Cajas.FindAsync(idCaja);

                if (caja == null)
                    return false; // La caja no existe

                // Agrega el préstamo al saldo total
                caja.Prestamos += prestamo;

                // Concatena el nuevo comentario con los comentarios anteriores, si los h
                caja.Comentarios += ConcatenarComentario(caja.Comentarios, comentarios, prestamo, estado);

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();

                return true; // El préstamo se agregó correctamente a la caja
            }
            catch (Exception)
            {
                // Manejo de errores, por ejemplo, loguear el error
                return false; // No se pudo agregar el préstamo a la caja
            }
        }
        public async Task<bool> PagoDelPrestamo(int idCaja, decimal prestamo, string comentarios, string estado)
        {
            try
            {
                // Encuentra la caja por su ID
                var caja = await _context.Cajas.FindAsync(idCaja);

                if (caja == null)
                    return false; // La caja no existe

                // Agrega el préstamo al saldo total
                caja.Prestamos -= prestamo;

                // Concatena el nuevo comentario con los comentarios anteriores, si los hay
                caja.Comentarios += ConcatenarComentario(caja.Comentarios, comentarios, prestamo, estado);

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();

                return true; // El préstamo se agregó correctamente a la caja
            }
            catch (Exception)
            {
                // Manejo de errores, por ejemplo, loguear el error
                return false; // No se pudo agregar el préstamo a la caja
            }
        }



        public async Task<bool> Gastos(int idCaja, decimal gastos, string comentarios, string estado)
        {
            try
            {
                // Encuentra la caja por su ID
                var caja = await _context.Cajas.FindAsync(idCaja);

                if (caja == null)
                    return false; // La caja no existe

                // Agrega el préstamo al saldo total
                caja.Gastos += gastos;

                // Concatena el nuevo comentario con los comentarios anteriores, si los h
                caja.ComentariosGastos += ConcatenarComentarioGastos(caja.ComentariosGastos, comentarios, gastos, estado);

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();

                return true; // El préstamo se agregó correctamente a la caja
            }
            catch (Exception)
            {
                // Manejo de errores, por ejemplo, loguear el error
                return false; // No se pudo agregar el préstamo a la caja
            }
        }
        public async Task<bool> PagoDevolucion(int idCaja, decimal gastos, string comentarios, string estado)
        {
            try
            {
                // Encuentra la caja por su ID
                var caja = await _context.Cajas.FindAsync(idCaja);

                if (caja == null)
                    return false; // La caja no existe

                // Agrega el préstamo al saldo total
                caja.Gastos -= gastos;

                // Concatena el nuevo comentario con los comentarios anteriores, si los hay
                caja.ComentariosDevoluciones += ConcatenarComentarioGastos(caja.ComentariosDevoluciones, comentarios, gastos, estado);

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();

                return true; // El préstamo se agregó correctamente a la caja
            }
            catch (Exception)
            {
                // Manejo de errores, por ejemplo, loguear el error
                return false; // No se pudo agregar el préstamo a la caja
            }
        }

        //private string ConcatenarComentario(string comentarioExistente, string nuevoComentario, decimal precio)
        //{
        //    // Formatea el precio con separador de miles y sin decimales
        //    string precioFormateado = precio.ToString("N0").Replace(",", ".");


        //    // Verifica si ya hay un comentario existente
        //    if (string.IsNullOrEmpty(comentarioExistente))
        //    {
        //        // Si no hay comentarios existentes, simplemente devuelve el nuevo comentario
        //        return $"{nuevoComentario} (Cantidad prestada: {precioFormateado} $)";
        //    }
        //    else
        //    {
        //        // Si hay comentarios existentes, concatena el nuevo comentario al existente junto con el precio del préstamo formateado
        //        return $" \n {nuevoComentario} (Cantidad prestada: {precioFormateado} $)";
        //    }
        //}

        private string ConcatenarComentario(string comentarioExistente, string nuevoComentario, decimal precio, string estado)
        {
            // Formatea el precio con separador de miles y sin decimales
            string precioFormateado = precio.ToString("N0").Replace(",", ".");

            // Verifica si ya hay un comentario existente
            if (string.IsNullOrEmpty(comentarioExistente))
            {
                if (estado == "prestamo")
                {
                    // Si no hay comentarios existentes, simplemente devuelve el nuevo comentario
                    return $"{nuevoComentario} (Cantidad prestada: {precioFormateado})";
                }
                else
                {
                    // Si no hay comentarios existentes, simplemente devuelve el nuevo comentario
                    return $"{nuevoComentario} (Cantidad Pagada: {precioFormateado})";
                }

            }
            else
            {

                if (estado == "prestamo")
                {
                    // Si hay comentarios existentes, concatena el nuevo comentario al existente junto con el precio del préstamo formateado
                    return $" \n {nuevoComentario} ((Cantidad prestada: {precioFormateado})";
                }
                else
                {
                    // Si hay comentarios existentes, concatena el nuevo comentario al existente junto con el precio del préstamo formateado
                    return $" \n {nuevoComentario} (Cantidad Pagada: {precioFormateado})";
                }
            }
        }

        private string ConcatenarComentarioGastos(string comentarioExistente, string nuevoComentario, decimal precio, string estado)
        {
            // Formatea el precio con separador de miles y sin decimales
            string precioFormateado = precio.ToString("N0").Replace(",", ".");

            // Verifica si ya hay un comentario existente
            if (string.IsNullOrEmpty(comentarioExistente))
            {
                if (estado == "gasto")
                {
                    // Si no hay comentarios existentes, simplemente devuelve el nuevo comentario
                    return $"{nuevoComentario} (Cantidad Pagada: {precioFormateado})";
                }
                else
                {
                    // Si no hay comentarios existentes, simplemente devuelve el nuevo comentario
                    return $"{nuevoComentario} (Cantidad Devuelta: {precioFormateado})";
                }

            }
            else
            {

                if (estado == "gasto")
                {
                    // Si hay comentarios existentes, concatena el nuevo comentario al existente junto con el precio del préstamo formateado
                    return $" \n {nuevoComentario} ((Cantidad Pagada: {precioFormateado})";
                }
                else
                {
                    // Si hay comentarios existentes, concatena el nuevo comentario al existente junto con el precio del préstamo formateado
                    return $" \n {nuevoComentario} (Cantidad Devuelta: {precioFormateado})";
                }
            }
        }


        private string ConcatenarComentarioIdVenta(string comentarioExistente, string nuevoComentario, string numeroDocumento, string estado)
        {
            // Formatea el precio con separador de miles y sin decimales
            string precioFormateado = numeroDocumento.ToString();

            // Verifica si ya hay un comentario existente
            if (string.IsNullOrEmpty(comentarioExistente))
            {
                if (estado == "Venta")
                {
                    // Si no hay comentarios existentes, simplemente devuelve el nuevo comentario
                    return $"{nuevoComentario} (# Venta: {precioFormateado})";
                }
                else
                {
                    // Si no hay comentarios existentes, simplemente devuelve el nuevo comentario
                    return $"{nuevoComentario} (# Cotizacion: {precioFormateado})";
                }

            }
            else
            {

                if (estado == "Venta")
                {
                    // Si hay comentarios existentes, concatena el nuevo comentario al existente junto con el precio del préstamo formateado
                    return $" \n {nuevoComentario} (# Venta: {precioFormateado})";
                }
                else
                {
                    // Si hay comentarios existentes, concatena el nuevo comentario al existente junto con el precio del préstamo formateado
                    return $" \n {nuevoComentario} (# Cotizacion: {precioFormateado})";
                }
            }
        }

        private string ConcatenarComentarioIdCompra(string comentarioExistente, string nuevoComentario, string numeroDocumentoCompra)
        {
            // Formatea el precio con separador de miles y sin decimales
            string precioFormateado = numeroDocumentoCompra.ToString();


            // Verifica si ya hay un comentario existente
            if (string.IsNullOrEmpty(comentarioExistente))
            {

                // Si no hay comentarios existentes, simplemente devuelve el nuevo comentario
                return $"{nuevoComentario} (# Compra: {precioFormateado})";


            }
            else
            {

                // Si hay comentarios existentes, concatena el nuevo comentario al existente junto con el precio del préstamo formateado
                return $" \n {nuevoComentario} (# compra: {precioFormateado})";

            }
        }


    }
}

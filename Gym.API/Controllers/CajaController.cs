using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


using Gym.BLL.Servicios.Contrato;
using Gym.DTO;
using Gym.Api.Utilidad;
using Microsoft.EntityFrameworkCore;
using Gym.Model;
using Gym.BLL.Servicios;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;


namespace Gym.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class CajaController : ControllerBase
    {

        private readonly ICajaService _cajaServicio;
        private readonly DbgymContext _context;
        public CajaController(ICajaService cajaServicio, DbgymContext context)
        {
            _cajaServicio = cajaServicio;
            _context = context;
        }
        //[Authorize]
        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> Lista()
        {
            //aqui devuelve una lista de categoria DTO
            var rsp = new Response<List<CajaDTO>>();

            try
            {
                rsp.status = true;
                rsp.value = await _cajaServicio.Lista();


            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }
            return Ok(rsp);




        }
        [Authorize]
        [HttpGet]
        [Route("ListaSoloHoy")]
        public async Task<IActionResult> ListaSoloHoy()
        {
            var rsp = new Response<List<CajaDTO>>();

            try
            {
                // Obtiene la lista de cajas desde el servicio
                //var todasLasCajas = await _cajaServicio.Lista();

                //// Filtra las cajas para obtener solo las de la fecha actual
                //var fechaActual = DateTime.Today; // Obtiene la fecha actual sin hora
                //rsp.value = todasLasCajas
                //    .Where(caja => caja.FechaRegistro.HasValue && caja.FechaRegistro.Value.Date == fechaActual) // Verifica si tiene un valor y compara solo la parte de la fecha
                //    .ToList();

                //rsp.status = true;

                // Obtiene la lista de cajas desde el servicio
                var todasLasCajas = await _cajaServicio.Lista();

                // Filtra las cajas para obtener solo las que tienen esActivo en true
                rsp.value = todasLasCajas
                    .Where(caja => caja.Estado == "Abierto") // Filtra solo las cajas activas
                    .ToList();

                rsp.status = true;

            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }

            return Ok(rsp);
        }

        [Authorize]
        [Route("ListaPaginada")]
        [HttpGet]
        public ActionResult<IEnumerable<CajaDTO>> GetCaja(int page = 1, int pageSize = 5, string searchTerm = null)
        {
            IQueryable<CajaDTO> query = _context.Cajas
                .Select(c => new CajaDTO
                {
                    IdUsuario = c.IdUsuario,
                    NombreUsuario = c.NombreUsuario,
                    IdCaja = c.IdCaja,
                    FechaApertura = c.FechaApertura,
                    FechaCierre = c.FechaCierre,
                    SaldoInicialTexto = c.SaldoInicial != null ? c.SaldoInicial.ToString() : string.Empty,
                    SaldoFinalTexto = c.SaldoFinal != null ? c.SaldoFinal.ToString() : string.Empty,
                    IngresosTexto = c.Ingresos != null ? c.Ingresos.ToString() : string.Empty,
                    DevolucionesTexto = c.Devoluciones != null ? c.Devoluciones.ToString() : string.Empty,
                    PrestamosTexto = c.Prestamos != null ? c.Prestamos.ToString() : string.Empty,
                    GastosTexto = c.Gastos != null ? c.Gastos.ToString() : string.Empty,
                    TransaccionesTexto = c.Transacciones != null ? c.Transacciones.ToString() : string.Empty,
                    Estado = c.Estado,
                    Comentarios = c.Comentarios,
                    FechaRegistro = c.FechaRegistro,
                    UltimaActualizacion = c.UltimaActualizacion,
                    MetodoPago = c.MetodoPago,
                    ComentariosGastos = c.ComentariosGastos,
                    ComentariosDevoluciones = c.ComentariosDevoluciones
                });

            if (int.TryParse(searchTerm, out int isActive))
            {
                query = query.Where(c => c.IdCaja == isActive);
            }
            else if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c =>
                    c.NombreUsuario.Contains(searchTerm) ||
                    c.Estado.Contains(searchTerm)
                );
            }

            query = query.OrderByDescending(c => c.IdCaja);


            var totalCategorias = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalCategorias / pageSize);

            var categoriasPaginadas = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new { data = categoriasPaginadas, total = totalCategorias, totalPages });
        }


        [Authorize]
        [HttpGet("{idCaja}")]
        public async Task<ActionResult<CajaDTO>> ObtenerCajaPorId(int idCaja)
        {
            try
            {
                var caja = await _cajaServicio.ObtenerCajaPorId(idCaja);
                if (caja == null)
                {
                    // Si no se encuentra la caja, devolvemos un código de estado 404 (Not Found)
                    return NotFound();
                }
                return caja;
            }
            catch (Exception ex)
            {
                // Si ocurre un error durante el proceso, devolvemos un código de estado 500 (Internal Server Error)
                return StatusCode(500, "Error interno del servidor: " + ex.Message);
            }
        }
        //[Authorize]
        [HttpPost]
        [Route("Guardar")]
        public async Task<IActionResult> Guardar([FromBody] CajaDTO caja)
        {

            var rsp = new Response<CajaDTO>();
            try
            {

                var cajaExistente = await _context.Cajas.AnyAsync(p => p.IdCaja == caja.IdCaja);

                if (cajaExistente)
                {
                    rsp.status = false;
                    rsp.msg = "Ya existe una caja con el mismo nombre";
                    return Ok(rsp);
                }

                //rsp.status = true;
                //rsp.value = await _productoServicio.Crear(producto);


                rsp.status = true;
                rsp.value = await _cajaServicio.Crear(caja);


            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }
            return Ok(rsp);


        }
        [Authorize]
        [HttpPut]
        [Route("Editar")]
        public async Task<IActionResult> Editar([FromBody] CajaDTO caja)
        {

            var rsp = new Response<bool>();

            try
            {
                rsp.status = true;
                rsp.value = await _cajaServicio.Editar(caja);


            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }
            return Ok(rsp);


        }
        [Authorize]
        [HttpPut]
        [Route("EditarIngreso")]
        public async Task<IActionResult> EditarIngreso([FromBody] CajaDTO caja)
        {

            var rsp = new Response<bool>();

            try
            {
                rsp.status = true;
                rsp.value = await _cajaServicio.EditarIngreso(caja);


            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }
            return Ok(rsp);


        }
        [Authorize]
        [HttpPut]
        [Route("EditarGastos")]
        public async Task<IActionResult> EditarGastos([FromBody] CajaDTO caja)
        {

            var rsp = new Response<bool>();

            try
            {
                rsp.status = true;
                rsp.value = await _cajaServicio.EditarGastos(caja);


            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }
            return Ok(rsp);


        }
        [Authorize]
        [HttpPut]
        [Route("EditarDevoluciones")]
        public async Task<IActionResult> EditarDevoluciones([FromBody] CajaDTO caja)
        {

            var rsp = new Response<bool>();

            try
            {
                rsp.status = true;
                rsp.value = await _cajaServicio.EditarDevoluciones(caja);


            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }
            return Ok(rsp);


        }
        [Authorize]
        [HttpPut]
        [Route("EditarDevolucionesGasto")]
        public async Task<IActionResult> EditarDevolucionesGasto([FromBody] CajaDTO caja)
        {

            var rsp = new Response<bool>();

            try
            {
                rsp.status = true;
                rsp.value = await _cajaServicio.EditarDevolucionesGasto(caja);


            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }
            return Ok(rsp);


        }
        [Authorize]
        [HttpDelete]
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {

            var rsp = new Response<bool>();

            try
            {
                rsp.status = true;
                rsp.value = await _cajaServicio.Eliminar(id);


            }
            catch (Exception ex)
            {
                rsp.status = false;
                rsp.msg = ex.Message;
            }
            return Ok(rsp);


        }


        [Authorize]
        [HttpPut("CambiarEstado/{id}")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            try
            {

                var result = await _cajaServicio.CambiarEstado(id);

                // Verifica si se cambió el estado correctamente
                if (result)
                    return Ok(new { status = true, message = "El estado de la caja se cambió correctamente." });
                else
                    return BadRequest(new { status = false, message = "No se pudo cambiar el estado de la caja." });
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return StatusCode(500, new { status = false, message = "Ocurrió un error al cambiar el estado de la caja .", error = ex.Message });
            }
        }


        [Authorize]
        [HttpGet("usuario/{idUsuario}")]
        public async Task<ActionResult<Caja>> ObtenerCajaPorUsuario(int idUsuario)
        {
            try
            {
                var caja = await _cajaServicio.ObtenerCajaPorUsuario(idUsuario);

                if (caja == null)
                {
                    return NotFound();
                }

                // Verificar si la caja está en estado abierto
                if (caja.Estado != "Abierto")
                {
                    // Si la caja no está en estado abierto, retornar un mensaje de error
                    return BadRequest("La caja no está en estado abierto");
                }

                return caja;
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return StatusCode(500, $"Error al obtener la caja por usuario: {ex.Message}");
            }
        }
        //[HttpGet("caja/{idCaja}")]
        //public async Task<ActionResult<Caja>> ObtenerCajaPoridCaja(int idCaja)
        //{
        //    try
        //    {
        //        var caja = await _cajaServicio.ObtenerCajaPoridCaja(idCaja);

        //        if (caja == null)
        //        {
        //            return NotFound();
        //        }

        //        // Verificar si la caja está en estado abierto
        //        if (caja.Estado != "Abierto")
        //        {
        //            // Si la caja no está en estado abierto, retornar un mensaje de error
        //            return BadRequest("La caja no está en estado abierto");
        //        }

        //        return caja;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Manejo de errores
        //        return StatusCode(500, $"Error al obtener la caja por usuario: {ex.Message}");
        //    }
        //}

        //// PUT: api/Caja/5
        //[HttpPut("{id}")]
        //public async Task<IActionResult> ActualizarCaja(int id, Caja caja)
        //{
        //    if (id != caja.IdCaja)
        //    {
        //        return BadRequest();
        //    }

        //    try
        //    {
        //        var cajaActualizada = await _cajaServicio.ActualizarCaja(caja);
        //        return Ok(cajaActualizada);
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(500, "Error interno del servidor");
        //    }
        //}

        [Authorize]
        [HttpPost("gastos")]
        public async Task<IActionResult> gastos([FromBody] GastoRequest prestamoRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                // Llama al servicio para manejar el préstamo
                var result = await _cajaServicio.Gastos(prestamoRequest.IdCaja, prestamoRequest.GastosTexto, prestamoRequest.ComentariosGastos, prestamoRequest.Estado);

                if (result)
                {
                    return Ok(); // Devuelve un Ok si el préstamo se realizó correctamente
                }
                else
                {
                    return BadRequest(); // Devuelve un BadRequest si hubo un problema al realizar el préstamo
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al realizar el préstamo: " + ex.Message);
            }
        }
        [Authorize]
        [HttpPost("PagoDevolucion")]
        public async Task<IActionResult> PagoDevolucion([FromBody] GastoRequest prestamoRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                // Llama al servicio para manejar el préstamo
                var result = await _cajaServicio.PagoDevolucion(prestamoRequest.IdCaja, prestamoRequest.GastosTexto, prestamoRequest.ComentariosGastos, prestamoRequest.Estado);

                if (result)
                {
                    return Ok(); // Devuelve un Ok si el préstamo se realizó correctamente
                }
                else
                {
                    return BadRequest(); // Devuelve un BadRequest si hubo un problema al realizar el préstamo
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al realizar el préstamo: " + ex.Message);
            }
        }



        [Authorize]
        [HttpPost("prestamo")]
        public async Task<IActionResult> Prestamo([FromBody] PrestamoRequest prestamoRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                // Llama al servicio para manejar el préstamo
                var result = await _cajaServicio.Prestamo(prestamoRequest.IdCaja, prestamoRequest.PrestamosTexto, prestamoRequest.Comentarios, prestamoRequest.Estado);

                if (result)
                {
                    return Ok(); // Devuelve un Ok si el préstamo se realizó correctamente
                }
                else
                {
                    return BadRequest(); // Devuelve un BadRequest si hubo un problema al realizar el préstamo
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al realizar el préstamo: " + ex.Message);
            }
        }
        [Authorize]
        [HttpPost("PagoPrestamo")]
        public async Task<IActionResult> PagoPrestamo([FromBody] PrestamoRequest prestamoRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                // Llama al servicio para manejar el préstamo
                var result = await _cajaServicio.PagoDelPrestamo(prestamoRequest.IdCaja, prestamoRequest.PrestamosTexto, prestamoRequest.Comentarios, prestamoRequest.Estado);

                if (result)
                {
                    return Ok(); // Devuelve un Ok si el préstamo se realizó correctamente
                }
                else
                {
                    return BadRequest(); // Devuelve un BadRequest si hubo un problema al realizar el préstamo
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al realizar el préstamo: " + ex.Message);
            }
        }


        [Authorize]
        [HttpPost("gasto")]
        public async Task<IActionResult> Gasto([FromBody] PrestamoRequestComenGasto prestamoRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                // Llama al servicio para manejar el préstamo
                var result = await _cajaServicio.Gasto(prestamoRequest.IdCaja, prestamoRequest.numeroDocumentoCompra, prestamoRequest.ComentariosGastos);

                if (result)
                {
                    return Ok(); // Devuelve un Ok si el préstamo se realizó correctamente
                }
                else
                {
                    return BadRequest(); // Devuelve un BadRequest si hubo un problema al realizar el préstamo
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al realizar el préstamo: " + ex.Message);
            }
        }
        [Authorize]
        [HttpPost("devoluciones")]
        public async Task<IActionResult> Devoluciones([FromBody] PrestamoRequestComen prestamoRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                // Llama al servicio para manejar el préstamo
                var result = await _cajaServicio.Devoluciones(prestamoRequest.IdCaja, prestamoRequest.numeroDocumento, prestamoRequest.ComentariosDevoluciones, prestamoRequest.Estado);

                if (result)
                {
                    return Ok(); // Devuelve un Ok si el préstamo se realizó correctamente
                }
                else
                {
                    return BadRequest(); // Devuelve un BadRequest si hubo un problema al realizar el préstamo
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al realizar el préstamo: " + ex.Message);
            }
        }

        //[Authorize]
        //[HttpPost("devolucionesCotizacion")]
        //public async Task<IActionResult> DevolucionesCotizaciones([FromBody] PrestamoRequestComenGasto prestamoRequest)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    try
        //    {
        //        // Llama al servicio para manejar el préstamo
        //        var result = await _cajaServicio.DevolucionesCotizaciones(prestamoRequest.IdCaja, prestamoRequest.numeroDocumentoCompra, prestamoRequest.ComentariosGastos);

        //        if (result)
        //        {
        //            return Ok(); // Devuelve un Ok si el préstamo se realizó correctamente
        //        }
        //        else
        //        {
        //            return BadRequest(); // Devuelve un BadRequest si hubo un problema al realizar el préstamo
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "Error al realizar el préstamo: " + ex.Message);
        //    }
        //}

        public class GastoRequest
        {
            [Required]
            public int IdCaja { get; set; }
            [Required]
            public decimal GastosTexto { get; set; }
            [Required]
            public string ComentariosGastos { get; set; }
            public string Estado { get; set; }

        }

        public class PrestamoRequest
        {
            [Required]
            public int IdCaja { get; set; }
            [Required]
            public decimal PrestamosTexto { get; set; }
            [Required]
            public string Comentarios { get; set; }
            public string Estado { get; set; }

        }

        public class PrestamoRequestComenGasto
        {
            [Required]
            public int IdCaja { get; set; }
            [Required]
            public string numeroDocumentoCompra { get; set; }
            [Required]
            public string ComentariosGastos { get; set; }


        }



        public class PrestamoRequestComen
        {
            [Required]
            public int IdCaja { get; set; }
            [Required]
            public string numeroDocumento { get; set; }
            [Required]
            public string ComentariosDevoluciones { get; set; }

            public string Estado { get; set; }
        }


    }
}

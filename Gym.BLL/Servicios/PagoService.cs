using Gym.BLL.Servicios.Contrato;
using Gym.DTO;
using Gym.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.BLL.Servicios
{
    public class PagoService : IPagoService
    {
        private readonly DbgymContext _context;

        public PagoService(DbgymContext context)
        {
            _context = context;
        }

        public async Task<(Pago Pago, string Mensaje)> RegistrarPago(PagoDTO pagoDto)
        {
            var zonaHorariaColombia = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            var fechaHoraColombia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHorariaColombia);

            var fechaColombiana = fechaHoraColombia;
            fechaHoraColombia = fechaHoraColombia.Date.AddHours(23); // Ajustar la hora a las 11:00 PM

            var asignacion = await _context.AsignacionesMembresia
                .Include(am => am.IdMembresiaNavigation)
                .FirstOrDefaultAsync(am => am.IdUsuario == pagoDto.IdUsuario);

            if (asignacion == null)
            {
                throw new Exception("El usuario no tiene una asignación de membresía.");
            }

            if (asignacion.Estado != "Pendiente")
            {
                throw new Exception("La membresía ya está activada o no está en estado pendiente.");
            }

            var dias = asignacion.IdMembresiaNavigation.DuracionDias;

            if (dias < 2)
            {
                pagoDto.TipoPago = "diario";
            }
            else if (dias >= 28 && dias <= 33)
            {
                pagoDto.TipoPago = "mensual";
            }
            else
            {
                pagoDto.TipoPago = "anual";
            }

            var pago = new Pago
            {
                IdUsuario = pagoDto.IdUsuario,
                Monto = asignacion.IdMembresiaNavigation.Precio,
                MetodoPago = pagoDto.MetodoPago,
                TipoPago = pagoDto.TipoPago,
                FechaPago = fechaColombiana,
                Observaciones = pagoDto.Observaciones
            };

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            asignacion.Estado = "Activado";

            switch (pagoDto.TipoPago.Trim().ToLower())
            {
                case "diario":
                    asignacion.FechaVencimiento = fechaHoraColombia;
                    break;
                //case "mensual":
                //    asignacion.FechaVencimiento = asignacion.FechaVencimiento;
                //    break;
                //case "anual":
                //    asignacion.FechaVencimiento = asignacion.FechaVencimiento;
                //    break;
                case "mensual":
                    var fechaVencimientoMensual = asignacion.FechaVencimiento?.AddMonths(1) ?? fechaHoraColombia.AddMonths(1);
                    var ultimoDiaDelMes = DateTime.DaysInMonth(fechaVencimientoMensual.Year, fechaVencimientoMensual.Month);

                    if (fechaColombiana.Day != asignacion.FechaVencimiento?.Day)
                    {
                        asignacion.FechaVencimiento = new DateTime(
                          fechaVencimientoMensual.Year,
                          fechaVencimientoMensual.Month,
                          Math.Min(fechaHoraColombia.Day, ultimoDiaDelMes),
                          fechaHoraColombia.Hour,
                          fechaHoraColombia.Minute,
                          fechaHoraColombia.Second
                      );
                    }
                    else
                    {

                        asignacion.FechaVencimiento = new DateTime(
                       fechaVencimientoMensual.Year,
                       fechaVencimientoMensual.Month,
                       Math.Min(fechaVencimientoMensual.Day, ultimoDiaDelMes),
                       fechaHoraColombia.Hour,
                       fechaHoraColombia.Minute,
                       fechaHoraColombia.Second
                   );

                    }

                   
                    break;
                case "anual":
                    var fechaVencimientoAnual = asignacion.FechaVencimiento?.AddYears(1) ?? fechaHoraColombia.AddYears(1);
                    var ultimoDiaDelMesAnual = DateTime.DaysInMonth(fechaVencimientoAnual.Year, fechaVencimientoAnual.Month);

                    if (fechaColombiana.Day != asignacion.FechaVencimiento?.Day)
                    {


                        asignacion.FechaVencimiento = new DateTime(
                       fechaVencimientoAnual.Year,
                       fechaVencimientoAnual.Month,
                       Math.Min(fechaHoraColombia.Day, ultimoDiaDelMesAnual),
                       fechaHoraColombia.Hour,
                       fechaHoraColombia.Minute,
                       fechaHoraColombia.Second
                              );

                    }
                    else
                    {

                         asignacion.FechaVencimiento = new DateTime(
                         fechaVencimientoAnual.Year,
                         fechaVencimientoAnual.Month,
                         Math.Min(fechaVencimientoAnual.Day, ultimoDiaDelMesAnual),
                         fechaHoraColombia.Hour,
                         fechaHoraColombia.Minute,
                         fechaHoraColombia.Second
                                );
                     


                    }
                    break;

                default:
                    throw new Exception("Tipo de pago no válido. Debe ser 'Diario', 'Mensual' o 'Anual'.");
            }

            asignacion.FechaAsignacion = fechaColombiana;

            _context.AsignacionesMembresia.Update(asignacion);
            await _context.SaveChangesAsync();

            var historialPago = new HistorialPago
            {
                PagoId = pago.PagoId,
                IdUsuario = pago.IdUsuario,
                FechaPago = pago.FechaPago,
                Monto = pago.Monto,
                TipoPago = pago.TipoPago
            };
            _context.HistorialPagos.Add(historialPago);
            await _context.SaveChangesAsync();

            var cajaActiva = await _context.Cajas
                .FirstOrDefaultAsync(c => c.Estado == "Abierto");

            if (cajaActiva != null)
            {
                if (pago.MetodoPago == "Efectivo")
                {
                    cajaActiva.Ingresos += pago.Monto;
                }
                else
                {
                    cajaActiva.Transacciones += pago.Monto;
                }

                _context.Cajas.Update(cajaActiva);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("No hay una caja activa disponible.");
            }

            // Buscar y actualizar la asistencia del usuario
            var asistencia = await _context.AsistenciaPersonals
                .FirstOrDefaultAsync(a =>
                    a.IdUsuario == pagoDto.IdUsuario &&
                    a.FechaAsistencia.Value.Date == fechaColombiana.Date);

            if (asistencia != null)
            {
                asistencia.PagoRealizado = true;
                _context.AsistenciaPersonals.Update(asistencia);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Si no existe la asistencia, creamos una nueva
                var nuevaAsistencia = new AsistenciaPersonal
                {
                    IdUsuario = pagoDto.IdUsuario,
                    FechaAsistencia = fechaColombiana,
                    PagoRealizado = true
                };

                _context.AsistenciaPersonals.Add(nuevaAsistencia);
                await _context.SaveChangesAsync();
            }

            string mensajeExito = $"El pago ha sido registrado exitosamente. El nuevo estado de la membresía es '{asignacion.Estado}', con vencimiento el {asignacion.FechaVencimiento:yyyy-MM-dd HH:mm:ss}.";
            return (pago, mensajeExito);
        }


        public async Task<(Pago Pago, string Mensaje)> RegistrarPagoCalendario(PagoDTO pagoDto)
        {
            var zonaHorariaColombia = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            var fechaHoraColombia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHorariaColombia);

            var fechaColombiana = fechaHoraColombia;
            fechaHoraColombia = fechaHoraColombia.Date.AddHours(23); // Ajustar la hora a las 11:00 PM

            var asignacion = await _context.AsignacionesMembresia
                .Include(am => am.IdMembresiaNavigation)
                .FirstOrDefaultAsync(am => am.IdUsuario == pagoDto.IdUsuario);

            if (asignacion == null)
            {
                throw new Exception("El usuario no tiene una asignación de membresía.");
            }

            //if (asignacion.Estado != "Pendiente")
            //{
            //    throw new Exception("La membresía ya está activada o no está en estado pendiente.");
            //}

            var dias = asignacion.IdMembresiaNavigation.DuracionDias;

            if (dias < 2)
            {
                pagoDto.TipoPago = "diario";
            }
            else if (dias >= 28 && dias <= 33)
            {
                pagoDto.TipoPago = "mensual";
            }
            else
            {
                pagoDto.TipoPago = "anual";
            }

            var pago = new Pago
            {
                IdUsuario = pagoDto.IdUsuario,
                Monto = asignacion.IdMembresiaNavigation.Precio,
                MetodoPago = pagoDto.MetodoPago,
                TipoPago = pagoDto.TipoPago,
                FechaPago = fechaColombiana,
                Observaciones = pagoDto.Observaciones
            };

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            //asignacion.Estado = "Activado";

            //asignacion.FechaAsignacion = fechaColombiana;

            //_context.AsignacionesMembresia.Update(asignacion);
            //await _context.SaveChangesAsync();

            var historialPago = new HistorialPago
            {
                PagoId = pago.PagoId,
                IdUsuario = pago.IdUsuario,
                FechaPago = pago.FechaPago,
                Monto = pago.Monto,
                TipoPago = pago.TipoPago
            };
            _context.HistorialPagos.Add(historialPago);
            await _context.SaveChangesAsync();

            var cajaActiva = await _context.Cajas
                .FirstOrDefaultAsync(c => c.Estado == "Abierto");

            if (cajaActiva != null)
            {
                if (pago.MetodoPago == "Efectivo")
                {
                    cajaActiva.Ingresos += pago.Monto;
                }
                else
                {
                    cajaActiva.Transacciones += pago.Monto;
                }

                _context.Cajas.Update(cajaActiva);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("No hay una caja activa disponible.");
            }

            // Buscar y actualizar la asistencia del usuario
        
            var asistencia = await _context.AsistenciaPersonals
           .FirstOrDefaultAsync(a =>
               a.IdUsuario == pagoDto.IdUsuario &&a.AsistenciaId == pagoDto.IdAsistencia);

            if (asistencia != null)
            {

                if(asistencia.FechaAsistencia.Value.Date == fechaColombiana.Date)
                {

                    asignacion.Estado = "Activado";
                    asignacion.FechaAsignacion = fechaColombiana;
                    asignacion.FechaVencimiento = fechaHoraColombia;

                    _context.AsignacionesMembresia.Update(asignacion);
                    await _context.SaveChangesAsync();

                    asistencia.PagoRealizado = true;
                    _context.AsistenciaPersonals.Update(asistencia);
                    await _context.SaveChangesAsync();


                }
                else
                {
                    asistencia.PagoRealizado = true;
                    _context.AsistenciaPersonals.Update(asistencia);
                    await _context.SaveChangesAsync();


                }



             
            }
          

            string mensajeExito = $"El pago ha sido registrado exitosamente. El nuevo estado de la membresía es '{asignacion.Estado}', con vencimiento el {asignacion.FechaVencimiento:yyyy-MM-dd HH:mm:ss}.";
            return (pago, mensajeExito);
        }


        public async Task<List<HistorialPagoDTO>> ObtenerPagosPorUsuario(int idUsuario)
        {
            var historialPagos = await _context.HistorialPagos
                .Where(hp => hp.IdUsuario == idUsuario)
                .OrderByDescending(hp => hp.FechaPago)
                .Include(hp => hp.IdUsuarioNavigation)
                .ToListAsync();

            // Mapeo manual
            return historialPagos.Select(hp => new HistorialPagoDTO
            {
                HistorialPagoId = hp.HistorialPagoId, // Asumiendo que el campo "Id" en HistorialPago corresponde a HistorialPagoId
                PagoId = hp.PagoId,
                IdUsuario = hp.IdUsuario,
                NombreUsuario = hp.IdUsuarioNavigation?.NombreCompleto, // Extraer el nombre del usuario relacionado
                FechaPago = hp.FechaPago?.ToString("dd/MM/yyyy hh:mm tt") ?? "Fecha no disponible",
                MontoTexto = hp.Monto.ToString(), // Convertir el monto a formato de texto
                TipoPago = hp.TipoPago

            }).ToList();
        }

        public async Task<IEnumerable<HistorialPagoDTO>> ObtenerTodosLosPagos()
        {
            var historialPagos = await _context.HistorialPagos
                .OrderByDescending(h => h.FechaPago)
                .Include(h => h.IdUsuarioNavigation)
                .ToListAsync();

            // Mapeo manual
            return historialPagos.Select(hp => new HistorialPagoDTO
            {
                HistorialPagoId = hp.HistorialPagoId, // Asumiendo que el campo "Id" en HistorialPago corresponde a HistorialPagoId
                PagoId = hp.PagoId,
                IdUsuario = hp.IdUsuario,
                NombreUsuario = hp.IdUsuarioNavigation?.NombreCompleto, // Extraer el nombre del usuario relacionado
                FechaPago = hp.FechaPago?.ToString("dd/MM/yyyy hh:mm tt") ?? "Fecha no disponible",
                MontoTexto = hp.Monto.ToString(), // Convertir el monto a formato de texto
                TipoPago = hp.TipoPago
            });
        }


    }

}

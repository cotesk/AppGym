using Gym.BLL.Servicios.Contrato;
using Gym.DTO;
using Gym.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gym.BLL.Servicios
{
    public class AsistenciaPersonalService : IAsistenciaPersonalService
    {
        private readonly DbgymContext _context;

        public AsistenciaPersonalService(DbgymContext context)
        {
            _context = context;
        }

    

        public async Task<List<AsistenciaPersonalDTO>> ListarAsistencias()
        {
            return await _context.AsistenciaPersonals
               .OrderByDescending(a => a.FechaAsistencia)
                .Select(a => new AsistenciaPersonalDTO
            {
                AsistenciaId = a.AsistenciaId,
                IdUsuario = a.IdUsuario,
                FechaAsistencia = a.FechaAsistencia.HasValue ? a.FechaAsistencia.Value.ToString("dd/MM/yyyy hh:mm tt") : null,
                NombreUsuario = a.IdUsuarioNavigation != null ? a.IdUsuarioNavigation.NombreCompleto : null, // Nombre del usuario
                PagoRealizado = a.PagoRealizado,


            }).ToListAsync();
        }

        public async Task<List<AsistenciaPersonalDTO>> ConsultarAsistenciasPorUsuario(int idUsuario)
        {
            return await _context.AsistenciaPersonals
                .Where(a => a.IdUsuario == idUsuario )
                .OrderByDescending(a => a.FechaAsistencia)
                .Select(a => new AsistenciaPersonalDTO
                {
                    AsistenciaId = a.AsistenciaId,
                    IdUsuario = a.IdUsuario,
                    FechaAsistencia = a.FechaAsistencia.HasValue ? a.FechaAsistencia.Value.ToString("dd/MM/yyyy hh:mm tt") : null,
                    NombreUsuario = a.IdUsuarioNavigation != null ? a.IdUsuarioNavigation.NombreCompleto : null, // Nombre del usuario
                    PagoRealizado = a.PagoRealizado,

                }).ToListAsync();
        }
    }
}

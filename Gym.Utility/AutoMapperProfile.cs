using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using Gym.DTO;
using Gym.Model;



namespace Gym.Utility
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {


            #region Usuario
            CreateMap<Usuario, UsuarioDTO>()
               .ForMember(destino =>
                destino.RolDescripcion,
                opt => opt.MapFrom(origen => origen.IdRolNavigation.Nombre)
                )
                .ForMember(destino =>
                destino.EsActivo,
                opt => opt.MapFrom(origen => origen.EsActivo == true ? 1 : 0)

                );

            CreateMap<Usuario, SesionDTO>()
               .ForMember(destino =>
                destino.RolDescripcion,
                opt => opt.MapFrom(origen => origen.IdRolNavigation.Nombre)
                );

            CreateMap<UsuarioDTO, Usuario>()
                .ForMember(destino =>
                destino.IdRolNavigation,
                opt => opt.Ignore()
                )
                 .ForMember(destino =>
                destino.EsActivo,
                opt => opt.MapFrom(origen => origen.EsActivo == 1 ? true : false)

                );

            #endregion Usuario

            #region membresia
            CreateMap<Membresia, MembresiaDTO>()

                  .ForMember(destino =>
                destino.EsActivo,
                opt => opt.MapFrom(origen => origen.EsActivo == true ? 1 : 0)

                )
              

               .ForMember(destino =>
                destino.PrecioTexto,
                 opt => opt.MapFrom(origen => Convert.ToString(origen.Precio.ToString("F0")))
                 );

            CreateMap<MembresiaDTO, Membresia>()

                      .ForMember(destino =>
                destino.EsActivo,
                opt => opt.MapFrom(origen => origen.EsActivo == 1 ? true : false)

                )
                   .ForMember(destino =>
                destino.Precio,
                 opt => opt.MapFrom(origen => Convert.ToDecimal(origen.PrecioTexto))
                 );

            #endregion membresia

            #region Caja
            CreateMap<Caja, CajaDTO>()
                  .ForMember(destino =>
                destino.NombreUsuario,
                 opt => opt.MapFrom(origen => origen.IdUsuarioNavigation.NombreCompleto)
                 )
          .ForMember(destino =>
    destino.FechaApertura,
    opt => opt.MapFrom(origen => origen.FechaApertura.Value.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture))
    )
    .ForMember(destino =>
    destino.FechaCierre,
    opt => opt.MapFrom(origen => origen.FechaCierre.Value.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture))
    )
    .ForMember(destino =>
    destino.FechaRegistro,
    opt => opt.MapFrom(origen => origen.FechaRegistro.Value.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture))
   )
    .ForMember(destino =>
    destino.UltimaActualizacion,
    opt => opt.MapFrom(origen => origen.UltimaActualizacion.Value.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture))
         )


                                         .ForMember(destino =>
                destino.SaldoInicialTexto,
                 opt => opt.MapFrom(origen => Convert.ToString(origen.SaldoInicial.Value))
                 )
                               .ForMember(destino =>
               destino.NombreUsuario,
                opt => opt.MapFrom(origen => Convert.ToString(origen.NombreUsuario))
                )
               .ForMember(destino =>
                destino.SaldoFinalTexto,
                 opt => opt.MapFrom(origen => Convert.ToString(origen.SaldoFinal.Value))
                 )
                   .ForMember(destino =>
                destino.IngresosTexto,
                 opt => opt.MapFrom(origen => Convert.ToString(origen.Ingresos.Value))
                 )
                       .ForMember(destino =>
                destino.TransaccionesTexto,
                 opt => opt.MapFrom(origen => Convert.ToString(origen.Transacciones.Value))
                 )
                       .ForMember(destino =>
                destino.DevolucionesTexto,
                 opt => opt.MapFrom(origen => Convert.ToString(origen.Devoluciones.Value))
                 )
                           .ForMember(destino =>
                destino.PrestamosTexto,
                 opt => opt.MapFrom(origen => Convert.ToString(origen.Prestamos.Value))
                 )
                               .ForMember(destino =>
                destino.GastosTexto,
                 opt => opt.MapFrom(origen => Convert.ToString(origen.Gastos.Value))
                 );
            CreateMap<CajaDTO, Caja>()

           .ForMember(destino =>
              destino.SaldoInicial,
          opt => opt.MapFrom(origen => origen.SaldoInicialTexto)
             )
      .ForMember(destino =>
      destino.SaldoFinal,
      opt => opt.MapFrom(origen => origen.SaldoFinalTexto))
          .ForMember(destino =>
              destino.Ingresos,
          opt => opt.MapFrom(origen => origen.IngresosTexto)
             )
            .ForMember(destino =>
              destino.Devoluciones,
          opt => opt.MapFrom(origen => origen.DevolucionesTexto)
             )
              .ForMember(destino =>
              destino.Prestamos,
          opt => opt.MapFrom(origen => origen.PrestamosTexto)
             )
                .ForMember(destino =>
              destino.Transacciones,
          opt => opt.MapFrom(origen => origen.TransaccionesTexto)
             )
                    .ForMember(destino =>
                destino.NombreUsuario,
                 opt => opt.MapFrom(origen => origen.NombreUsuario)
                 )
                .ForMember(destino =>
              destino.Gastos,
          opt => opt.MapFrom(origen => origen.GastosTexto)
             );
            #endregion Caja


            #region Rol
            CreateMap<Rol, RolDTO>().ReverseMap();
            #endregion Rol

            #region Empresa
            CreateMap<Empresa, EmpresaDTO>().ReverseMap();
            #endregion Empresa

            #region Contenido
            CreateMap<Contenido, ContenidoDTO>().ReverseMap();
            #endregion Contenido

            #region Menu
            CreateMap<Menu, MenuDTO>().ReverseMap();
            #endregion Menu

            #region Asistencia
            CreateMap<AsistenciaPersonal, AsistenciaPersonalDTO>().ReverseMap();
            #endregion Asistencia

            #region historialPago
            CreateMap<HistorialPago, HistorialPagoDTO>()
                .ForMember(destino =>
                destino.MontoTexto,
                 opt => opt.MapFrom(origen => Convert.ToString(origen.Monto))
                 )
                       .ForMember(destino =>
             destino.FechaPago,
             opt => opt.MapFrom(origen => origen.FechaPago.Value.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture))
              );

            CreateMap<HistorialPagoDTO, HistorialPago>()
                   .ForMember(destino =>
                        destino.Monto,
                        opt => opt.MapFrom(origen => origen.MontoTexto));
        

            #endregion historialPago

            #region Entrenadores
            CreateMap<Entrenadores, EntrenadoresDTO>().ReverseMap();
            #endregion Entrenadores

            #region EntrenadoresCliente
            CreateMap<EntrenadorCliente, EntrenadorClienteDTO>().ReverseMap();
            #endregion EntrenadoresCliente


            #region MenuRol
            CreateMap<MenuRol, MenuDTO>()
       .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.IdMenuNavigation.Nombre))
       // Mapea otras propiedades si es necesario
       .ReverseMap();


            #endregion MenuRol

        }
    }
}

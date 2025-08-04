using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Hosting;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Globalization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Gym.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


using Gym.DAL.Repositorios.Contrato;
using Gym.DAL.Repositorios;
using Gym.Utility;
using Gym.BLL.Servicios.Contrato;
using Gym.BLL.Servicios;
using Microsoft.AspNetCore.Builder;
using Gym.Model;


namespace Gym.IOC
{
    public static class Dependecia
    {

        public static void InyectarDependecias(this IServiceCollection service, IConfiguration configuration)
        {


            service.AddDbContext<DbgymContext>(options => {
                options.UseSqlServer(configuration.GetConnectionString("cadenaSQL"));
            });
            // Configuración de la cultura y el formato
            var cultureInfo = new CultureInfo("es-CO"); // Cultura colombiana
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            // Opcional: configurar formatos específicos (por ejemplo, para fechas y números)
            var dateTimeFormats = new DateTimeFormatInfo
            {
                ShortDatePattern = "dd/MM/yyyy",
                LongDatePattern = "dd/MM/yyyy HH:mm:ss",
                ShortTimePattern = "HH:mm",
                LongTimePattern = "HH:mm:ss"
            };
            cultureInfo.DateTimeFormat = dateTimeFormats;



        


            // Repositorios
            service.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
     
            //service.AddScoped<IAbonosVentaRepository, AbonosVentaRepository>();
            //service.AddScoped<ICompraRepository, CompraRepository>();
            //service.AddScoped<ICotizacionRepository, CotizacionRepository>();

            // Configuración de AutoMapper
            service.AddAutoMapper(typeof(AutoMapperProfile));

            // Servicios
        
            service.AddScoped<IUsuarioService, UsuarioService>();
            service.AddScoped<IRolService, RolService>();
            service.AddScoped<IMembresiaService, MembresiaService>();
            service.AddScoped<IDashBoardService, DashBoardService>();
            service.AddScoped<IMenuService, MenuService>();
            service.AddScoped<IAsistenciaPersonalService, AsistenciaPersonalService>();
            service.AddScoped<IPagoService, PagoService>();
            service.AddScoped<IEmpresaService, EmpresaService>();
            service.AddScoped<ICajaService, CajaService>();
            service.AddScoped<IContenidoService, ContenidoService>();
            //service.AddScoped<IAbonoService, AbonoService>();
            service.AddScoped<IEmailService, EmailService>();
            //service.AddScoped<IBodegaService, BodegaService>();
            //service.AddScoped<ICotizacionService, CotizacionService>();
            //service.AddScoped<BackupService>();


        }




    }
}

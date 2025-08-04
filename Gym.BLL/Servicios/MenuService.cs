using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using Gym.BLL.Servicios.Contrato;
using Gym.DAL.Repositorios.Contrato;
using Gym.DTO;
using Gym.Model;



namespace Gym.BLL.Servicios
{
    public class MenuService : IMenuService
    {


        private readonly IGenericRepository<MenuRol> _menuRolRepositorio;
        private readonly IGenericRepository<Menu> _menuRepositorio;
        private readonly IGenericRepository<Usuario> _usuarioRepositorio;
        private readonly IMapper _mapper;


        public MenuService(IGenericRepository<MenuRol> menuRolRepositorio, IGenericRepository<Menu> menuRepositorio, IGenericRepository<Usuario> usuarioRepositorio, IMapper mapper)
        {
            _menuRolRepositorio = menuRolRepositorio;
            _menuRepositorio = menuRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _mapper = mapper;
        }

        public async Task<List<MenuDTO>> Lista(int idUsuario)
        {

            IQueryable<Usuario> tbUsuario = await _usuarioRepositorio.Consultar(u => u.IdUsuario == idUsuario);
            IQueryable<Menu> tbMenu = await _menuRepositorio.Consultar();
            IQueryable<MenuRol> tbMenuRol = await _menuRolRepositorio.Consultar();


            try
            {
                IQueryable<Menu> tbResultado = (from u in tbUsuario
                                                join mr in tbMenuRol on u.IdRol equals mr.IdRol
                                                join m in tbMenu on mr.IdMenu equals m.IdMenu
                                                select m).AsQueryable();
                var listaMenus = tbResultado.ToList();
                return _mapper.Map<List<MenuDTO>>(listaMenus);
            }
            catch
            {


                throw;

            }


        }
        private readonly string logFilePath = "C:\\Software DE Bajo web\\SistemaVenta2023-V2\\error.log";

        public async Task<bool> AgregarPermiso(int idRol, int idMenu)
        {
            try
            {
                Console.WriteLine("Intentando agregar permiso con los siguientes datos:");
                Console.WriteLine("ID de Rol:", idRol);
                Console.WriteLine("ID de Menú:", idMenu);

                // Verificar si el permiso ya existe
                var existePermiso = await _menuRolRepositorio.Consultar(mr => mr.IdRol == idRol && mr.IdMenu == idMenu);
                if (existePermiso.Any())
                {
                    // El permiso ya existe, no es necesario agregarlo nuevamente
                    Console.WriteLine("El permiso para idRol: {0} y idMenu: {1} ya existe. No se agregará nuevamente.", idRol, idMenu);
                    return false;
                }

                // Crear el nuevo permiso
                var nuevoPermiso = new MenuRol
                {
                    IdRol = idRol,
                    IdMenu = idMenu
                };

                // Guardar el nuevo permiso en la base de datos
                await _menuRolRepositorio.Crear(nuevoPermiso);


                Console.WriteLine("Permiso agregado correctamente para idRol: {0} y idMenu: {1}", idRol, idMenu);

                // Éxito
                return true;
            }
            catch (Exception ex)
            {
                // Capturar la excepción y registrarla para obtener más detalles
                RegistrarError(ex);
                // Puedes lanzar la excepción nuevamente si deseas que sea manejada en un nivel superior
                throw;
            }
        }
        public async Task<bool> EliminarPermiso(int idRol, int idMenu)
        {
            try
            {
                // Buscar el permiso a eliminar
                var permiso = await _menuRolRepositorio.Consultar(mr => mr.IdRol == idRol && mr.IdMenu == idMenu);

                if (!permiso.Any())
                {
                    // El permiso no existe
                    return false;
                }

                // Eliminar el permiso
                await _menuRolRepositorio.Eliminar(permiso.First());

                // Éxito
                return true;
            }
            catch (Exception ex)
            {
                // Manejar errores aquí
                throw new Exception("Error al eliminar el permiso.", ex);
            }
        }
        public async Task<MenuDTO> ObtenerMenuPorId(int idMenu)
        {
            try
            {
                // Realiza una consulta en tu repositorio de menús para obtener el menú por su idMenu
                var menu = await _menuRepositorio.ObtenerPorId(idMenu);

                // Verifica si el menú fue encontrado
                if (menu != null)
                {
                    // Mapea el menú a un objeto MenuDTO utilizando AutoMapper u otra técnica de mapeo
                    var menuDTO = _mapper.Map<MenuDTO>(menu);

                    // Retorna el menú DTO
                    return menuDTO;
                }
                else
                {
                    // Retorna null si el menú no fue encontrado
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Maneja cualquier excepción que pueda ocurrir durante la obtención del menú
                // Puedes registrar el error, lanzar una excepción o retornar un valor predeterminado
                // Aquí solo se está registrando el error
                Console.WriteLine("Error al obtener el menú por ID:", ex.Message);

                // Retorna null para indicar que ocurrió un error al obtener el menú
                return null;
            }
        }



        private void RegistrarError(Exception ex)
        {
            try
            {
                string errorMessage = $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n";

                // Escribe el mensaje de error en el archivo de registro
                File.AppendAllText(logFilePath, errorMessage);
            }
            catch (Exception e)
            {
                // Si hay un error al registrar el error, imprime el error en la consola
                Console.WriteLine($"Error al registrar el error: {e.Message}");
            }
        }


        public async Task<List<MenuDTO>> ObtenerMenuRolesPorRol(int idRol)
        {
            try
            {
                var menuRoles = await _menuRolRepositorio.Consultar(menuRol => menuRol.IdRol == idRol);
                // Mapea los resultados a objetos MenuDTO y proporciona valores por defecto para propiedades nulas
                var menuDTOs = menuRoles.Select(menuRol => new MenuDTO
                {
                    IdMenu = menuRol.IdMenu ?? 0, // Utiliza ?? para proporcionar un valor predeterminado si IdMenu es null
                    Nombre = menuRol.IdMenuNavigation != null ? menuRol.IdMenuNavigation.Nombre : null,
                    Icono = menuRol.IdMenuNavigation != null ? menuRol.IdMenuNavigation.Icono : null,
                    Url = menuRol.IdMenuNavigation != null ? menuRol.IdMenuNavigation.Url : null,
                    IdMenuPadre = menuRol.IdMenuNavigation != null ? menuRol.IdMenuNavigation.IdMenuPadre : null
                }).ToList();
                return menuDTOs;
            }
            catch (Exception ex)
            {
                // Manejar la excepción según tus necesidades
                throw ex;
            }
        }

        //public async Task<bool> ModificarPermiso(int idRol, int idMenu, bool agregar)
        //{
        //    try
        //    {
        //        if (agregar)
        //        {
        //            // Verificar si el permiso ya existe
        //            var existePermiso = await _menuRolRepositorio.Consultar(mr => mr.IdRol == idRol && mr.IdMenu == idMenu);
        //            if (existePermiso.Any())
        //            {
        //                // El permiso ya existe, puedes retornar false o lanzar una excepción según tu lógica de negocio
        //                return false;
        //            }

        //            // Crear el nuevo permiso
        //            var nuevoPermiso = new MenuRol
        //            {
        //                IdRol = idRol,
        //                IdMenu = idMenu
        //            };

        //            // Agregar el nuevo permiso a la base de datos
        //            await _menuRolRepositorio.Crear(nuevoPermiso);
        //        }
        //        else
        //        {
        //            // Buscar el permiso a eliminar
        //            var permiso = await _menuRolRepositorio.Consultar(mr => mr.IdRol == idRol && mr.IdMenu == idMenu);

        //            if (!permiso.Any())
        //            {
        //                // El permiso no existe
        //                return false;
        //            }

        //            // Eliminar el permiso
        //            await _menuRolRepositorio.Eliminar(permiso.First());
        //        }

        //        // Éxito
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Manejar errores aquí
        //        throw new Exception("Error al modificar el permiso.", ex);
        //    }
        //}


        public async Task<List<MenuDTO>> ListaMenu()
        {
            try
            {

                var ListaMenu = await _menuRepositorio.Consultar();

                return _mapper.Map<List<MenuDTO>>(ListaMenu.ToList());
            }
            catch
            {
                throw;
            }
        }

        //public async Task<bool> ActualizarPermisos(int idRol, List<int> menuIds)
        //{
        //    try
        //    {
        //        foreach (var menuId in menuIds)
        //        {
        //            bool permisoAsignado = await AgregarPermiso(idRol, menuId);
        //            if (!permisoAsignado)
        //            {
        //                bool permisoEliminado = await EliminarPermiso(idRol, menuId);
        //                if (!permisoEliminado)
        //                {
        //                    // Manejar el caso en que ni el permiso fue asignado ni eliminado correctamente
        //                    return false;
        //                }
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Manejar errores aquí
        //        throw new Exception("Error al actualizar permisos.", ex);
        //    }
        //}

    }
}

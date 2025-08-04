using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gym.DTO;

namespace Gym.BLL.Servicios.Contrato
{
    public interface IMenuService
    {

        Task<List<MenuDTO>> Lista(int idUsuario);
        Task<List<MenuDTO>> ListaMenu();
        Task<bool> AgregarPermiso(int idRol, int idMenu);
        Task<bool> EliminarPermiso(int idRol, int idMenu);
        //Task<bool> ModificarPermiso(int idRol, int idMenu, bool agregar);
        Task<List<MenuDTO>> ObtenerMenuRolesPorRol(int idRol);
        Task<MenuDTO> ObtenerMenuPorId(int idMenu);
        //Task<bool> ActualizarPermisos(int idRol, List<int> menuIds);
    }
}

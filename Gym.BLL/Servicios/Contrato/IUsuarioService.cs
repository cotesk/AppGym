using Gym.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.BLL.Servicios.Contrato
{
    public interface IUsuarioService
    {



        Task<List<UsuarioDTO>> Lista();
        Task<SesionDTO> ValidadarCredenciales(string correo, string clave);
        Task<UsuarioDTO> Crear(UsuarioDTO modelo);
        Task<bool> Editar(UsuarioDTO modelo);
        Task<bool> EditarUsuario(UsuarioDTO modelo);
        Task<bool> Eliminar(int id);

    }
}

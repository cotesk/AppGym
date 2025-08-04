using Gym.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym.BLL.Servicios.Contrato
{
    public interface IEmailService
    {

        void SendEmail(EmailDTO request);
        void SendEmail2(EmailDTO request);
        //Task EnviarCorreoElectronico(string destinatario, string asunto, string cuerpo,bool isHtml = false);
        Task EnviarCorreoElectronico(string destinatario, string asunto, string cuerpo, bool isHtml = false, byte[] archivoAdjunto = null, string nombreAdjunto = "", string tipoMimeAdjunto = "");

        Task EnviarCorreoElectronicoOutlook(string destinatario, string asunto, string cuerpo, bool isHtml = false);
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Security;
using MimeKit.Text;
using MimeKit;
using MailKit.Net.Smtp;
using Gym.BLL.Servicios.Contrato;
using Microsoft.Extensions.Configuration;
using Gym.DTO;
using Gym.BLL.Servicios.Contrato;

namespace Gym.BLL.Servicios
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendEmail(EmailDTO request)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config.GetSection("Email:UserName").Value));
            email.To.Add(MailboxAddress.Parse(request.Para));
            email.Subject = request.Asunto;
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = request.Contenido
            };

            using var smtp = new SmtpClient();
            smtp.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true; // Ignorar validación del certificado solo para pruebas

            smtp.Connect(
                _config.GetSection("Email:Host").Value,
                Convert.ToInt32(_config.GetSection("Email:Port").Value),
                SecureSocketOptions.StartTls
            );

            smtp.Authenticate(_config.GetSection("Email:UserName").Value, _config.GetSection("Email:PassWord").Value);

            smtp.Send(email);
            smtp.Disconnect(true);
        }
        public void SendEmail2(EmailDTO request)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config.GetSection("Email2:UserName").Value));
            email.To.Add(MailboxAddress.Parse(request.Para));
            email.Subject = request.Asunto;
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = string.IsNullOrEmpty(request.Contenido) ? "Contenido vacío" : request.Contenido
            };

            using var smtp = new SmtpClient();
            smtp.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            try
            {
                smtp.Connect(
                    _config.GetSection("Email2:Host").Value,
                    Convert.ToInt32(_config.GetSection("Email2:Port").Value),
                    SecureSocketOptions.StartTls
                );

                smtp.Authenticate(_config.GetSection("Email2:UserName").Value, _config.GetSection("Email2:PassWord").Value);
                smtp.Send(email);
            }
            catch (SmtpCommandException ex)
            {

                throw new Exception("Error de comando SMTP: " + ex.Message, ex);
            }
            catch (SmtpProtocolException ex)
            {

                throw new Exception("Error de protocolo SMTP: " + ex.Message, ex);
            }
            catch (Exception ex)
            {

                throw new Exception("Error general al enviar el correo electrónico: " + ex.Message, ex);
            }
            finally
            {
                smtp.Disconnect(true);
            }
        }


        //public async Task EnviarCorreoElectronico(string destinatario, string asunto, string cuerpo, bool isHtml = false)
        //{
        //    var email = new MimeMessage();
        //    email.From.Add(MailboxAddress.Parse(_config["Email:UserName"]));
        //    email.To.Add(MailboxAddress.Parse(destinatario));
        //    email.Subject = asunto;
        //    // Cambia el cuerpo del correo a HTML si isHtml es true
        //    email.Body = new TextPart(isHtml ? TextFormat.Html : TextFormat.Plain)
        //    {
        //        Text = cuerpo
        //    };

        //    using (var client = new SmtpClient())
        //    {
        //        client.ServerCertificateValidationCallback = (s, c, h, e) => true; // Ignorar validación del certificado solo para pruebas

        //        await client.ConnectAsync(_config["Email:Host"], Convert.ToInt32(_config["Email:Port"]), SecureSocketOptions.StartTls);
        //        await client.AuthenticateAsync(_config["Email:UserName"], _config["Email:PassWord"]);
        //        await client.SendAsync(email);
        //        await client.DisconnectAsync(true);
        //    }
        //}


        public async Task EnviarCorreoElectronico(string destinatario, string asunto, string cuerpo, bool isHtml = false, byte[] archivoAdjunto = null, string nombreArchivo = "", string tipoMimeAdjunto = "")
        {
            try
            {
                // Crea el mensaje de correo electrónico
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_config["Email:UserName"]));
                email.To.Add(MailboxAddress.Parse(destinatario));
                email.Subject = asunto;

                // Crea el cuerpo del correo
                var body = new TextPart(isHtml ? TextFormat.Html : TextFormat.Plain)
                {
                    Text = cuerpo
                };

                // Si el archivo adjunto no es nulo, agregamos el adjunto al correo
                if (archivoAdjunto != null && archivoAdjunto.Length > 0)
                {
                    var attachment = new MimePart("application", tipoMimeAdjunto ?? "octet-stream")
                    {
                        Content = new MimeContent(new MemoryStream(archivoAdjunto)),  // Creamos un MemoryStream con el contenido del archivo
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        FileName = nombreArchivo  // Nombre del archivo adjunto
                    };

                    // Creamos un Multipart que contiene el cuerpo del correo y el archivo adjunto
                    var multipart = new Multipart("mixed")
            {
                body, // Cuerpo del correo
                attachment // El archivo adjunto
            };

                    email.Body = multipart;  // Asignamos el multipart como el cuerpo del correo
                }
                else
                {
                    email.Body = body;  // Si no hay archivo adjunto, usamos solo el cuerpo
                }

                // Enviamos el correo a través de SMTP
                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true; // Ignorar validación del certificado solo para pruebas

                    // Conectar al servidor SMTP
                    await client.ConnectAsync(_config["Email:Host"], Convert.ToInt32(_config["Email:Port"]), SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_config["Email:UserName"], _config["Email:PassWord"]);

                    // Enviar el correo
                    await client.SendAsync(email);

                    // Desconectar
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                Console.WriteLine($"Error al enviar correo: {ex.Message}");
                throw;  // Lanza el error para ser capturado por el controlador
            }
        }



        public async Task EnviarCorreoElectronicoOutlook(string destinatario, string asunto, string cuerpo, bool isHtml = false)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["Email2:UserName"]));
            email.To.Add(MailboxAddress.Parse(destinatario));
            email.Subject = asunto;
            // Cambia el cuerpo del correo a HTML si isHtml es true
            email.Body = new TextPart(isHtml ? TextFormat.Html : TextFormat.Plain)
            {
                Text = cuerpo
            };

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true; // Ignorar validación del certificado solo para pruebas

                await client.ConnectAsync(_config["Email2:Host"], Convert.ToInt32(_config["Email2:Port"]), SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_config["Email2:UserName"], _config["Email2:PassWord"]);
                await client.SendAsync(email);
                await client.DisconnectAsync(true);
            }
        }


        //public void SendEmail(EmailDTO request)
        //{
        //    if (request == null || string.IsNullOrEmpty(request.Contenido))
        //    {
        //        throw new ArgumentException("El contenido del correo electrónico no puede ser nulo o vacío.", nameof(request));
        //    }

        //    var email = new MimeMessage();
        //    email.From.Add(MailboxAddress.Parse(_config.GetSection("Email:UserName").Value));
        //    email.To.Add(MailboxAddress.Parse(request.Para));
        //    email.Subject = request.Asunto;

        //    // Verificar si el contenido del correo electrónico no es nulo o vacío antes de asignarlo al cuerpo
        //    if (!string.IsNullOrEmpty(request.Contenido))
        //    {
        //        email.Body = new TextPart(TextFormat.Html)
        //        {
        //            Text = request.Contenido
        //        };
        //    }

        //    using var smtp = new SmtpClient();
        //    smtp.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true; // Ignorar validación del certificado solo para pruebas

        //    smtp.Connect(
        //        _config.GetSection("Email:Host").Value,
        //        Convert.ToInt32(_config.GetSection("Email:Port").Value),
        //        SecureSocketOptions.StartTls
        //    );

        //    // Autenticación según el proveedor de correo
        //    if (_config.GetSection("Email:Host").Value.Contains("gmail"))
        //    {
        //        smtp.Authenticate(_config.GetSection("Email:UserName").Value, _config.GetSection("Email:PassWord").Value);
        //    }
        //    else if (_config.GetSection("Email:Host").Value.Contains("hotmail"))
        //    {
        //        smtp.Authenticate(_config.GetSection("Email:UserName").Value, _config.GetSection("Email:PassWord").Value); // Enviar contraseña en claro para Hotmail
        //    }
        //    else
        //    {
        //        // Si no es Gmail ni Hotmail, utiliza la autenticación predeterminada
        //        smtp.Authenticate(_config.GetSection("Email:UserName").Value, _config.GetSection("Email:PassWord").Value);
        //    }

        //    smtp.Send(email);
        //    smtp.Disconnect(true);
        //}



    }
}

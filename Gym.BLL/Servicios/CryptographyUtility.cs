using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;

//using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace Gym.BLL.Servicios
{
    public class CryptographyUtility
    {
        private readonly string _encryptionKey;

        public CryptographyUtility(IConfiguration configuration)
        {
            _encryptionKey = configuration["EncryptionSettings:Key"];
        }

        public string EncryptToken(string token)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(_encryptionKey);
            byte[] tokenBytes = Encoding.UTF8.GetBytes(token);

            using (Aes aesAlg = Aes.Create())
            {
                // Ajustar la longitud de la clave a 256 bits (32 bytes)
                aesAlg.KeySize = 256;
                aesAlg.BlockSize = 128;

                // Si la clave es demasiado corta, hacer un hash de la clave para ajustarla a la longitud requerida
                if (keyBytes.Length < aesAlg.KeySize / 8)
                {
                    aesAlg.Key = new Rfc2898DeriveBytes(_encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }, 1000).GetBytes(aesAlg.KeySize / 8);
                }
                else
                {
                    aesAlg.Key = keyBytes.Take(aesAlg.KeySize / 8).ToArray();
                }

                aesAlg.IV = new byte[16]; // Use a zero IV for simplicity. You may want to use a random IV for more security.

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(tokenBytes, 0, tokenBytes.Length);
                        csEncrypt.FlushFinalBlock();
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public string DecryptToken(string encryptedToken)
        {
            try
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(_encryptionKey);
                byte[] tokenBytes = Convert.FromBase64String(encryptedToken);

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.KeySize = 256;
                    aesAlg.BlockSize = 128;

                    if (keyBytes.Length < aesAlg.KeySize / 8)
                    {
                        aesAlg.Key = new Rfc2898DeriveBytes(_encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }, 1000).GetBytes(aesAlg.KeySize / 8);
                    }
                    else
                    {
                        aesAlg.Key = keyBytes.Take(aesAlg.KeySize / 8).ToArray();
                    }

                    aesAlg.IV = new byte[16];

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(tokenBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Captura cualquier excepción y lanza un mensaje de error
                throw new Exception("Error al descifrar el token. Verifica la clave de cifrado y el formato del token.", ex);
            }
        }




    }
}

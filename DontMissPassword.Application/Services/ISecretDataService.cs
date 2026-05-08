using DontMissPassword.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Application.Services
{
    public class SecretDataService : ISecretDataService
    {
        private readonly byte[] _key;

        public SecretDataService(IConfiguration config)
        {
            var keyString = config["Aes:Key"];
            _key = Convert.FromBase64String(keyString);

            if (_key.Length != 32)
                throw new Exception("AES-256 must have a 32-byte key.");
        }
        public string DecryptData(string encryptedData, string ivBase64)
        {
           byte[] iv = Convert.FromBase64String(ivBase64);
           byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = iv;
                ICryptoTransform cryptoTransform = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream(encryptedBytes))
                {
                    using (CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                           return sr.ReadToEnd();
                            
                        }
                    }
                }
            }
        }

        public (string EncryptedData, string IV) EncryptData(string plantText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.GenerateIV();
                byte[] iv = aes.IV;

                ICryptoTransform cryptoTransform = aes.CreateDecryptor(aes.Key, iv);
                using (MemoryStream ms = new MemoryStream()) {
                    using (CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plantText);
                        }
                        byte[] encryptedData = ms.ToArray();
                        return (Convert.ToBase64String(encryptedData), Convert.ToBase64String(iv));
                    }
                }
            }
        }
    }
}

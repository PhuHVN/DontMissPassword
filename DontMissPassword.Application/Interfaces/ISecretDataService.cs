using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Application.Interfaces
{
    public interface ISecretDataService
    {
        string DecryptData(string encryptedData, string ivBase64);
        (string EncryptedData, string IV) EncryptData(string plantText);
    }
}

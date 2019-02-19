using System;
using System.IO;
using System.Security.Cryptography;
using Hephaestus.Model.Core.Interfaces;

namespace Hephaestus.Model.Core
{
    public class Md5 : IMd5
    {
        public string Create(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}

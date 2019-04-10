using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hephaestus.Model.Transcompiler
{
    class Extensions
    {
        public static string SHA256(string filename)
        {
            using (var os = File.OpenRead(filename))
            {
                var hasher = new SHA256CryptoServiceProvider();
                return HashingStream.ToHex(hasher.ComputeHash(os));
            };
            
        }

    }
}

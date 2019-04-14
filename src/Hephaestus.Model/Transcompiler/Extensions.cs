using IniParser.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hephaestus.Model.Transcompiler
{
    public static class Extensions
    {
        public static string SHA256(string filename)
        {
            using (var os = File.OpenRead(filename))
            {
                var hasher = new SHA256CryptoServiceProvider();
                return HashingStream.ToHex(hasher.ComputeHash(os));
            };
            
        }

        public static string GetIn(this IniData ini, string section, string key)
        {
            if (ini == null) return null;
            var section_val = ini[section];
            if (section_val == null) return null;
            return section_val[key];

        }

    }
}

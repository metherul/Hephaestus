using System.Collections.Generic;

namespace Hephaestus.Model.Modpack.ModpackBase
{
    public class ModpackHeader
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string AutomatonVersion { get; set; }

        public List<string> ModProfiles { get; set; }
    }
}

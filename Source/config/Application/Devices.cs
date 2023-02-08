using System.Collections.Generic;

namespace Config.Application
{
    public class Devices
    {
        public Verifone Verifone { get; set; }
        public List<string> ComPortBlackList { get; set; } = new List<string>();
    }
}

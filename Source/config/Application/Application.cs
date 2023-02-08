using System;

namespace Config.Application
{
    [Serializable]
    public class Application
    {
        public Colors Colors { get; set; }
    }

    [Serializable]
    public class Colors
    {
        public string ForeGround { get; set; } = "WHITE";
        public string BackGround { get; set; } = "BLUE";
    }
}

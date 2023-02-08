using System.Collections.Generic;

namespace AppCommon.Helpers.EMVKernel
{
    public class EMVKernelConfiguration
    {
        public enum FrontEnd : int
        {
            FDRC,
            TSYS
        };

        //TODO: per Austin's suggestion, load from JSON file to avoid updating code every time a new certification requires updates in this structure 
        public static readonly Dictionary<string, (string checksum, bool allowChange, List<int> frontend)> VerifoneKernelConfiguration = new Dictionary<string, (string checksum, bool allowChange, List<int>)>()
        {
            { "1C", ("96369E1F", true, new List<int>(){ (int)FrontEnd.FDRC, (int)FrontEnd.TSYS }) },        // Attended, PIN
            //{ "2C", ("0F602A4C", false, null) },                                                          // TODO: validate checksum during certification
            //{ "3C", ("92CC8774", false, null) },                                                          // TODO: validate checksum during certification  
            //{ "4C", ("C76E6769", false, null) },                                                          // TODO: validate checksum during certification
            //{ "5C", ("B415F3D9", false, null) },                                                          // TODO: validate checksum during certification
            //{ "6C", ("59881906", false, null) },                                                          // TODO: validate checksum during certification
            //{ "7C", ("BEFA519B", false, null) },                                                          // TODO: validate checksum during certification
            { "8C", ("D196BA9D", true, new List<int>(){ (int)FrontEnd.FDRC, (int)FrontEnd.TSYS }) },        // Unattended, PIN
            //{ "9C", ("6A695602", false, null) },                                                          // TODO: validate checksum during certification
            { "10C", ("49D432BB", false, new List<int>(){ (int)FrontEnd.FDRC }) },                          // Unattended, No-PIN
            //{ "12C", ("D856CB80", false, null) },                                                         // TODO: validate checksum during certification
            //{ "13C", ("1EC74C36", false, null) },                                                         // TODO: validate checksum during certification
            //{ "14C", ("CDA9CD3A", false, null) },                                                         // TODO: validate checksum during certification
            { "15C", ("8798CC22", false, new List<int>(){ (int)FrontEnd.FDRC }) }                           // Attended, No-PIN
        };

        public string Frontend { get; set; }
        public string Checksum { get; set; }
    }
}

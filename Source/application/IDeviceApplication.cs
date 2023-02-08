using Common.XO.Requests;
using System.Threading.Tasks;

namespace DEVICE_CORE
{
    public interface IDeviceApplication
    {
        void Initialize(string pluginPath);
        Task Run();
        Task Command(LinkDeviceActionType action);
        void Shutdown();
    }
}

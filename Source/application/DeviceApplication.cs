using Common.XO.Requests;
using Devices.Core.State.Enums;
using Devices.Core.State.Management;
using Ninject;
using System;
using System.Threading.Tasks;

namespace DEVICE_CORE
{
    internal class DeviceApplication : IDeviceApplication
    {
        [Inject]
        internal IDeviceStateManager DeviceStateManager { get; set; }

        private string pluginPath;

        public void Initialize(string pluginPath) => (this.pluginPath) = (pluginPath);

        private async Task WaitForManageWorkflow(bool displayProgress = false)
        {
            // Wait for Manage State
            if (displayProgress)
            {
                Console.Write("INITIALIZING ");
            }
            for (; ; )
            {
                if (DeviceStateManager.GetCurrentWorkflow() == DeviceWorkflowState.Manage)
                {
                    break;
                }
                if (displayProgress)
                {
                    Console.Write(".");
                }
                await Task.Delay(500);
            }
            if (displayProgress)
            {
                Console.WriteLine(" FINISHED!\n");
            }
        }

        public async Task Run()
        {
            DeviceStateManager.SetPluginPath(pluginPath);
            _ = Task.Run(() => DeviceStateManager.LaunchWorkflow());
            await WaitForManageWorkflow();
            DeviceStateManager.DisplayDeviceStatus();
        }

        public async Task Command(LinkDeviceActionType action)
        {
            Console.WriteLine($"\n==========================================================================================");
            Console.WriteLine($"DAL COMMAND: {action}");
            Console.WriteLine($"==========================================================================================");

            DeviceStateManager.SetWorkflow(action);
            await WaitForManageWorkflow();
        }

        public void Shutdown()
        {
            if (DeviceStateManager != null)
            {
                DeviceStateManager.StopWorkflow();
                DeviceStateManager = null;
            }
        }
    }
}

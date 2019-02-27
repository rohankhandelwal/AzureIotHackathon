
using System.Threading.Tasks;

namespace AzureIoT.Hackathon.Test.Console
{
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using AzureIoT.Hackathon.Device.Provisioning;
    using AzureIoT.Hackathon.Device.Client;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Client;
    using System;
    using System.Collections.Generic;

    class Program
    {
        static void Main(string[] args)
        {

            var thumbprints = File.ReadAllLines("thumbprints.txt");
            var taskList = new List<Task>();
            foreach (var thumbprint in thumbprints)
            {
                taskList.Add(StartDeviceLifeCycle(thumbprint));
            }

            Task.WaitAll(taskList.ToArray());

            //RegistryManager rm = RegistryManager.CreateFromConnectionString("HostName=spradhanscus.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=oLdeaajxU9+IfphZCr4uLL2woHPvM8zZKkGhQOqNlUw=");
            //Configuration cfg = new Configuration("firmware2");
            //cfg.Labels = new Dictionary<string, string>() { { "AppType", "Firmware update" } };
            //cfg.TargetCondition = "properties.reported.firmware.version.major <= 2";
            //cfg.Content = new ConfigurationContent();
            //cfg.Content.DeviceContent = new Dictionary<string, object>{
            //    {
            //        "properties.desired.firmware", new
            //        {
            //            version = new
            //            {
            //                major = 2,
            //                minor = 0,
            //                patch = 0,
            //            },
            //            url = "https://www.example.com/fw/2.0.0.000000"
            //        }
            //    }
            //};
            //cfg.Metrics.Queries.Add("compliant", "select deviceId from devices where properties.reported.firmware.version.major >= 2");
            //rm.AddConfigurationAsync(cfg).GetAwaiter().GetResult();

            Console.WriteLine($"Press Enter to exit");
            Console.ReadLine();
        }

        public static async Task StartDeviceLifeCycle(string thumbprint)
        {
            var dps = new DeviceProvisioning(thumbprint);
            var dc = await dps.StartProvisioning();
            var dma = new DeviceManagementAgent(await dps.StartProvisioning());
            dma.InitializeAsync().GetAwaiter().GetResult();
        }
    }
}

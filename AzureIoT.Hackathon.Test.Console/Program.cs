namespace AzureIoT.Hackathon.Test.Console
{
    using AzureIoT.Hackathon.Device.Client;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Client;
    using System;
    using System.Collections.Generic;

    class Program
    {
        private static string deviceConnectionString = "HostName=spradhanscus.azure-devices.net;DeviceId=dpstestdevice1;SharedAccessKey=binZoHgVqAGGmf5g42C5NbNNlmW27KzFLN6vbPiGRKw=";

        static void Main(string[] args)
        {
            //DeviceClient dc = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp);

            //var dma = new DeviceManagementAgent(dc);
            //dma.InitializeAsync().GetAwaiter().GetResult();

            RegistryManager rm = RegistryManager.CreateFromConnectionString("HostName=spradhanscus.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=oLdeaajxU9+IfphZCr4uLL2woHPvM8zZKkGhQOqNlUw=");
            Configuration cfg = new Configuration("firmware2");
            cfg.Labels = new Dictionary<string, string>() { { "AppType", "Firmware update" } };
            cfg.TargetCondition = "properties.reported.firmware.version.major <= 2";
            cfg.Content = new ConfigurationContent();
            cfg.Content.DeviceContent = new Dictionary<string, object>{
                {
                    "properties.desired.firmware", new
                    {
                        version = new
                        {
                            major = 2,
                            minor = 0,
                            patch = 0,
                        },
                        url = "https://www.example.com/fw/2.0.0.000000"
                    }
                }
            };
            cfg.Metrics.Queries.Add("compliant", "select deviceId from devices where properties.reported.firmware.version.major >= 2");
            rm.AddConfigurationAsync(cfg).GetAwaiter().GetResult();

            Console.WriteLine($"Press Enter to exit");
            Console.ReadLine();
        }
    }
}

namespace AzureIoT.Hackathon.Test.Console
{
    using AzureIoT.Hackathon.Device.Client;
    using Microsoft.Azure.Devices.Client;
    using System;

    class Program
    {
        private static string deviceConnectionString = "HostName=spradhanscus.azure-devices.net;DeviceId=dpstestdevice1;SharedAccessKey=binZoHgVqAGGmf5g42C5NbNNlmW27KzFLN6vbPiGRKw=";

        static void Main(string[] args)
        {
            DeviceClient dc = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp);

            var dma = new DeviceManagementAgent(dc);
            dma.InitializeAsync().GetAwaiter().GetResult();

            Console.WriteLine($"Press Enter to exit");
            Console.ReadLine();
        }
    }
}

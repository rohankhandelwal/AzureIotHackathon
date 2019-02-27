// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoT.Hackathon.Test.Console
{
    using System.Threading.Tasks;
    using AzureIoT.Hackathon.Device.Client;
    using AzureIoT.Hackathon.Device.Provisioning;
    using System;
    using System.Collections.Generic;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            // Start Test for the devices in thumbprints.txt
            var thumbprints = File.ReadAllLines("thumbprints.txt");
            var taskList = new List<Task>();
            foreach (var thumbprint in thumbprints)
            {
                taskList.Add(StartDeviceLifeCycle(thumbprint));
            }

            Task.WaitAll(taskList.ToArray());

            Console.WriteLine($"Press Enter to exit");
            Console.ReadLine();
        }

        public static async Task StartDeviceLifeCycle(string thumbprint)
        {
            var dps = new DeviceProvisioning(thumbprint);
            var dc = await dps.StartProvisioningAsync();
            var dma = new DeviceManagementAgent(await dps.StartProvisioningAsync());
            dma.InitializeAsync().GetAwaiter().GetResult();
        }
    }
}
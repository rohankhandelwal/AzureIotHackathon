namespace AzureIoT.Hackathon.Device.Client
{
    using AzureIoT.Hackathon.Common.Data;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class DeviceManagementAgent
    {
        const string FirwarePropertyName = "firmware";
        readonly DeviceClient deviceClient;
        Firmware desiredFirmware;
        Firmware currentFirmware;

        public DeviceManagementAgent(DeviceClient deviceClient)
        {
            this.deviceClient = deviceClient;
        }

        public async Task InitializeAsync()
        {
            Console.WriteLine($"Initialize");
            var twin = await this.deviceClient.GetTwinAsync().ConfigureAwait(false);

            if (twin != null && twin.Properties != null && twin.Properties.Reported != null &&
                twin.Properties.Reported.Contains(FirwarePropertyName))
            {
                currentFirmware =
                    (JObject.FromObject(twin.Properties.Reported[FirwarePropertyName])).ToObject<Firmware>();
            }
            else
            {
                currentFirmware = new Firmware
                {
                    DownloadUrl = new Uri("https://www.example.com/fw/0.0"),
                    Version = new Common.Data.Version
                    {
                        Major = 0,
                        Minor = 0,
                    }
                };
            }

            this.PopulateDesiredFirmwareFromDesiredProperties(twin.Properties.Desired, true);
            await this.deviceClient.SetDesiredPropertyUpdateCallbackAsync(this.OnDesiredPropertiesChangeAsync, null);
            this.CheckNewVersionAndApplyAsync().Fork();
        }

        Task OnDesiredPropertiesChangeAsync(TwinCollection desiredProperties, object userContext)
        {
            Console.WriteLine("\tDesired property changed:");
            Console.WriteLine($"\t{desiredProperties.ToJson()}");

            PopulateDesiredFirmwareFromDesiredProperties(desiredProperties, false);

            return Task.CompletedTask;
        }

        void PopulateDesiredFirmwareFromDesiredProperties(TwinCollection desired, bool overwriteIfAbsent)
        {
            if (!desired.Contains(FirwarePropertyName) && overwriteIfAbsent)
            {
                this.desiredFirmware = this.currentFirmware.Clone();
                return;
            }

            if (this.desiredFirmware == null)
            {
                this.desiredFirmware = (JObject.FromObject(desired[FirwarePropertyName])).ToObject<Firmware>();
                return;
            }
            var firmwarePatch = JObject.FromObject(desired[FirwarePropertyName]);
            var desiredBefore = JObject.FromObject(this.desiredFirmware);
            desiredBefore.Merge(firmwarePatch, new JsonMergeSettings { MergeNullValueHandling = MergeNullValueHandling.Ignore });
            this.desiredFirmware = desiredBefore.ToObject<Firmware>();

            Console.WriteLine($"Desired Firmware version changed : {JsonConvert.SerializeObject(this.desiredFirmware, Formatting.Indented)}");
        }

        async Task CheckNewVersionAndApplyAsync()
        {
            while (true)
            {
                await Task.Delay(2000);

                if (currentFirmware.Version.CompareTo(desiredFirmware.Version) >= 0)
                {
                    continue;
                }

                Console.WriteLine($"Firmware upgrade desired from {JsonConvert.SerializeObject(this.currentFirmware)} => {JsonConvert.SerializeObject(this.desiredFirmware)}");
                // Download desiredFirmware.DownloadUrl
                // Run downloaded binary
                // update currentFirmware
                this.currentFirmware = this.desiredFirmware.Clone();

                // report current firmware
                await this.deviceClient.UpdateReportedPropertiesAsync(this.GetReportedPropertiesPatch());
            }
        }

        TwinCollection GetReportedPropertiesPatch()
        {
            var patch = new TwinCollection();
            patch[FirwarePropertyName] = JObject.FromObject(this.currentFirmware);
            return patch;
        }
    }
}

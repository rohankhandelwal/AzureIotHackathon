namespace AzureIoT.Hackathon.Device.Client
{
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json;
    using System;
    using System.Threading.Tasks;

    public class TwinWrapper
    {
        readonly DeviceClient deviceClient;

        TwinCollection desiredState;
        TwinCollection reportedState;

        long currentStatus;

        public TwinWrapper(DeviceClient deviceClient, long status)
        {
            this.deviceClient = deviceClient;
            this.currentStatus = status;
        }

        public async Task InitializeAsync()
        {
            var twin = await this.deviceClient.GetTwinAsync();

            Console.WriteLine("Initial Twin:");

            this.desiredState = twin.Properties.Desired;
            this.reportedState = twin.Properties.Reported;
            Console.WriteLine($"Desired : {JsonConvert.SerializeObject(this.desiredState, Formatting.Indented)}");
            Console.WriteLine($"Reported : {JsonConvert.SerializeObject(this.reportedState, Formatting.Indented)}");

            await this.deviceClient.SetDesiredPropertyUpdateCallbackAsync(this.OnDesiredPropertiesChangeAsync, null);

            if (this.desiredState.Contains("status"))
            {
                if (this.currentStatus < (long)this.desiredState["status"])
                {
                    this.ActAndReportAsync((long)this.desiredState["status"]).Fork();
                }
            }
        }

        async Task OnDesiredPropertiesChangeAsync(TwinCollection desiredProperties, object userContext)
        {
            Console.WriteLine("\tDesired property changed:");
            Console.WriteLine($"\t{desiredProperties.ToJson()}");

            this.desiredState = TwinCollectionMergeHelper.Merge(this.desiredState, desiredProperties);

            Console.WriteLine("Desird state after merge:");
            Console.WriteLine($"{JsonConvert.SerializeObject(this.desiredState, Formatting.Indented)}");

            if (this.desiredState.Contains("status"))
            {
                if (this.currentStatus < (long)this.desiredState["status"])
                {
                    this.ActAndReportAsync((long)this.desiredState["status"]).Fork();
                }
            }
        }

        async Task ActAndReportAsync(long status)
        {
            Console.WriteLine($"Acting on status {status}");
            await Task.Delay(TimeSpan.FromSeconds(status));
            this.currentStatus = status;
            await this.ReportStatusAsync();
        }

        async Task ReportStatusAsync()
        {
            TwinCollection toReport = new TwinCollection();
            if (this.reportedState.Contains("status") && this.currentStatus < (long)this.reportedState["status"])
            {
                toReport["status"] = this.currentStatus;
                toReport["error"] = "Invalid state";
                Console.WriteLine($"Reporting Invalid State => currentStatus = {this.currentStatus}, reportedStatus = {this.reportedState["status"]}");
                await this.deviceClient.UpdateReportedPropertiesAsync(toReport);
                this.reportedState = TwinCollectionMergeHelper.Merge(this.reportedState, toReport);
                return;
            }

            toReport["status"] = this.currentStatus;
            Console.WriteLine($"Reporting current state currentStatus = {toReport["status"]}");
            await this.deviceClient.UpdateReportedPropertiesAsync(toReport);
            this.reportedState = TwinCollectionMergeHelper.Merge(this.reportedState, toReport);
        }
    }
}

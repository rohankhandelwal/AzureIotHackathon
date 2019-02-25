namespace AzureIoT.Hackathon.Device.Client
{
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class TwinWrapper
    {
        const int DesiredPropertiesUpdateTimeoutInMs = 1000;
        const int DesiredPropertiesReadTimeoutInMs = 2000;

        readonly DeviceClient deviceClient;
        readonly ReaderWriterLock desiredPropsRwLock;

        TwinCollection desiredState;
        TwinCollection reportedState;

        int currentStatus;

        public TwinWrapper(DeviceClient deviceClient, int status)
        {
            this.deviceClient = deviceClient;
            this.desiredPropsRwLock = new ReaderWriterLock();
            this.currentStatus = status;
        }

        public async Task InitializeAsync()
        {
            var twin = await this.deviceClient.GetTwinAsync();

            Console.WriteLine("Initial Twin:");

            try
            {
                while (this.desiredPropsRwLock.IsReaderLockHeld)
                {
                    await Task.Delay(10);
                }

                this.desiredPropsRwLock.AcquireWriterLock(DesiredPropertiesUpdateTimeoutInMs);
                try
                {
                    this.desiredState = twin.Properties.Desired;
                    this.reportedState = twin.Properties.Reported;
                    Console.WriteLine($"Desired : {JsonConvert.SerializeObject(this.desiredState, Formatting.Indented)}");
                    Console.WriteLine($"Reported : {JsonConvert.SerializeObject(this.reportedState, Formatting.Indented)}");

                    await this.ReportStatusAsync();
                }
                finally
                {
                    this.desiredPropsRwLock.ReleaseLock();
                }
            }
            catch (ApplicationException)
            {
                Console.WriteLine("Timeout setting up initial state");
            }

            await this.deviceClient.SetDesiredPropertyUpdateCallbackAsync(this.OnDesiredPropertiesChangeAsync, null);

            if (this.desiredState.Contains("status"))
            {
                if (this.currentStatus < (int)this.desiredState["status"])
                {
                    this.ActAndReportAsync((int)this.desiredState["status"]).Fork();
                }
            }
        }

        async Task OnDesiredPropertiesChangeAsync(TwinCollection desiredProperties, object userContext)
        {
            Console.WriteLine("\tDesired property changed:");
            Console.WriteLine($"\t{desiredProperties.ToJson()}");

            try
            {
                while (this.desiredPropsRwLock.IsReaderLockHeld)
                {
                    await Task.Delay(10);
                }

                this.desiredPropsRwLock.AcquireWriterLock(DesiredPropertiesUpdateTimeoutInMs);
                try
                {
                    this.desiredState = TwinCollectionMergeHelper.Merge(this.desiredState, desiredProperties);
                }
                finally
                {
                    this.desiredPropsRwLock.ReleaseLock();
                }
            }
            catch (ApplicationException)
            {
                Console.WriteLine("Timeout updating desired state");
            }

            Console.WriteLine("Desird state after merge:");
            Console.WriteLine($"{JsonConvert.SerializeObject(this.desiredState, Formatting.Indented)}");

            if (this.desiredState.Contains("status"))
            {
                if (this.currentStatus < (int)this.desiredState["status"])
                {
                    this.ActAndReportAsync(this.desiredState["status"]).Fork();
                }
            }
        }

        async Task ActAndReportAsync(int status)
        {
            Console.WriteLine($"Acting on status {status}");
            await Task.Delay(TimeSpan.FromSeconds(status));
            this.currentStatus = status;
            await this.ReportStatusAsync();
        }

        async Task ReportStatusAsync()
        {
            TwinCollection toReport = new TwinCollection();
            if (this.reportedState.Contains("status") && this.currentStatus < (int)this.reportedState["status"])
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

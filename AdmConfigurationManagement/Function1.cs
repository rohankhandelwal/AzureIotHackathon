namespace AdmConfigurationManagement
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AzureIoT.Hackathon.Common.Data;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([BlobTrigger("samples-workitems/{name}", Connection = "")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");


            try
            {
                string fwText = null;
                using (TextReader tr = new StreamReader(myBlob))
                {
                    fwText = tr.ReadToEnd();
                }

                Firmware fw = JsonConvert.DeserializeObject<Firmware>(fwText);

                RegistryManager rm = RegistryManager.CreateFromConnectionString("HostName=spradhanscus.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=oLdeaajxU9+IfphZCr4uLL2woHPvM8zZKkGhQOqNlUw=");
                Configuration cfg = new Configuration(SanitizeConfigName(name));
                cfg.Labels = new Dictionary<string, string>() { { "AppType", "Firmware update" } };
                if (fw.Version.Minor > 0)
                {
                    cfg.TargetCondition = $"properties.reported.firmware.version.major = {fw.Version.Major} AND properties.reported.firmware.version.minor = {fw.Version.Minor - 1}";
                }
                else
                {
                    cfg.TargetCondition = $"properties.reported.firmware.version.major <= {fw.Version.Major}";
                }
                cfg.Content = new ConfigurationContent();
                cfg.Content.DeviceContent = new Dictionary<string, object>{
                {
                    "properties.desired.firmware", new
                    {
                        version = new
                        {
                            major = fw.Version.Major,
                            minor = fw.Version.Minor,
                        },
                        url = fw.DownloadUrl.ToString()
                    }
                }
                };
                cfg.Metrics.Queries.Add("compliant", $"select deviceId from devices where properties.reported.firmware.version.major >= {fw.Version.Major}");
                rm.AddConfigurationAsync(cfg).GetAwaiter().GetResult();
            }
            catch(Exception ex)
            {
                log.LogError(ex.ToString());
            }
        }

        static string SanitizeConfigName(string name)
        {
            string blobName = name.Substring(0, name.LastIndexOf("."));
            string sanitized = blobName.Replace(".", "_");
            return sanitized;
        }
    }
}

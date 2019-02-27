// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoT.Hackathon.Device.Provisioning
{
    using Microsoft.Azure.Devices.Provisioning.Client;
    using Microsoft.Azure.Devices.Provisioning.Client.Transport;
    using Microsoft.Azure.Devices.Shared;
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;

    public class DeviceProvisioning
    {
        string _provThumbprint;

        public DeviceProvisioning(string thumbprint)
        {
            _provThumbprint = thumbprint;
        }

        private static string s_idScope = "<ID_SCOPE>";//Environment.GetEnvironmentVariable("DPS_IDSCOPE");

        private const string GlobalDeviceEndpoint = "global.azure-devices-provisioning.net";

        public async Task<DeviceClient> StartProvisioningAsync()
        {
            ServicePointManager
                    .ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;

            X509Certificate2 rootCA = LoadCertificate("574D9333C86537BC99070353EF1A8C8C50B0B5AE");
            X509Certificate2 certificate = LoadCertificate(_provThumbprint);


            var myChain = new X509Certificate2Collection();
            myChain.Add(rootCA);
            myChain.Add(certificate);

            using (var security = new SecurityProviderX509Certificate(certificate, myChain))

            using (var transport = new ProvisioningTransportHandlerHttp())
            {
                Microsoft.Azure.Devices.Provisioning.Client.ProvisioningDeviceClient provClient =
                    Microsoft.Azure.Devices.Provisioning.Client.ProvisioningDeviceClient.Create(GlobalDeviceEndpoint, s_idScope, security, transport);

                var sample = new ProvisioningDeviceClient(provClient, security);
                return sample.ProvisionDeviceAndGetClientAsync().GetAwaiter().GetResult();
            }
        }

        private static X509Certificate2 LoadCertificate(string thumbprint)
        {
            X509Store certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            certStore.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certCollection = certStore.Certificates.Find(
                X509FindType.FindByThumbprint, thumbprint, false);
            certStore.Close();

            X509Certificate2 certificate = null;

            foreach (X509Certificate2 element in certCollection)
            {
                Console.WriteLine($"Found certificate: {element?.Thumbprint} {element?.Subject}; PrivateKey: {element?.HasPrivateKey}");
                if (certificate == null && element.HasPrivateKey)
                {
                    certificate = element;
                }
                else
                {
                    element.Dispose();
                }
            }

            if (certificate == null)
            {
                throw new FileNotFoundException($"did not contain any certificate with a private key.");
            }
            else
            {
                Console.WriteLine($"Using certificate {certificate.Thumbprint} {certificate.Subject}");
            }

            return certificate;
        }
    }
}
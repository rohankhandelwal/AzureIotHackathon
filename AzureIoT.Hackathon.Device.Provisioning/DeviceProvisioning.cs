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
        // The Provisioning Hub IDScope.

        // For this sample either:
        // - pass this value as a command-prompt argument
        // - set the DPS_IDSCOPE environment variable 
        // - create a launchSettings.json (see launchSettings.json.template) containing the variable
        private static string s_idScope = "0ne00043AE8";//Environment.GetEnvironmentVariable("DPS_IDSCOPE");

        // In your Device Provisioning Service please go to "Manage enrollments" and select "Individual Enrollments".
        // Select "Add individual enrollment" then fill in the following:
        // Mechanism: X.509
        // Certificate: 
        //    You can generate a self-signed certificate by running the GenerateTestCertificate.ps1 powershell script.
        //    Select the public key 'certificate.cer' file. ('certificate.pfx' contains the private key and is password protected.)
        //    For production code, it is advised that you install the certificate in the CurrentUser (My) store.
        // DeviceID: iothubx509device1

        // X.509 certificates may also be used for enrollment groups.
        // In your Device Provisioning Service please go to "Manage enrollments" and select "Enrollment Groups".
        // Select "Add enrollment group" then fill in the following:
        // Group name: <your  group name>
        // Attestation Type: Certificate
        // Certificate Type: 
        //    choose CA certificate then link primary and secondary certificates 
        //    OR choose Intermediate certificate and upload primary and secondary certificate files
        // You may also change other enrollment group parameters according to your needs

        private const string GlobalDeviceEndpoint = "global.azure-devices-provisioning.net";

        public async Task<DeviceClient> StartProvisioning()
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

            // Select one of the available transports:
            // To optimize for size, reference only the protocols used by your application.
            using (var transport = new ProvisioningTransportHandlerHttp())
            // using (var transport = new ProvisioningTransportHandlerHttp())
            // using (var transport = new ProvisioningTransportHandlerMqtt(TransportFallbackType.TcpOnly))
            // using (var transport = new ProvisioningTransportHandlerMqtt(TransportFallbackType.WebSocketOnly))
            {
                ProvisioningDeviceClient provClient =
                    ProvisioningDeviceClient.Create(GlobalDeviceEndpoint, s_idScope, security, transport);

                var sample = new ProvisioningDeviceClientSample(provClient, security);
                return sample.RunSampleAsync().GetAwaiter().GetResult();
            }
        }

        private static X509Certificate2 LoadCertificate(string thumbprint)
        {
            //string certificatePassword = ReadCertificatePassword();

            //certificateCollection.Import(s_certificateFileName, certificatePassword, X509KeyStorageFlags.UserKeySet);
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

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoT.Hackathon.Device.Provisioning
{
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Provisioning.Client;
    using Microsoft.Azure.Devices.Shared;
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class ProvisioningDeviceClient
    {
        Microsoft.Azure.Devices.Provisioning.Client.ProvisioningDeviceClient _provClient;
        SecurityProvider _security;

        public ProvisioningDeviceClient(Microsoft.Azure.Devices.Provisioning.Client.ProvisioningDeviceClient provisioningDeviceClient, SecurityProvider security)
        {
            _provClient = provisioningDeviceClient;
            _security = security;
        }

        public async Task<DeviceClient> ProvisionDeviceAndGetClientAsync()
        {
            Console.WriteLine($"RegistrationID = {_security.GetRegistrationID()}");
            VerifyRegistrationIdFormat(_security.GetRegistrationID());

            Console.Write("ProvisioningClient RegisterAsync . . . ");
            DeviceRegistrationResult result = await _provClient.RegisterAsync().ConfigureAwait(false);

            Console.WriteLine($"{result.Status}");
            Console.WriteLine($"ProvisioningClient AssignedHub: {result.AssignedHub}; DeviceID: {result.DeviceId}");

            if (result.Status != ProvisioningRegistrationStatusType.Assigned)
            {
                Console.WriteLine($"Provisioning failed : Status: {result.Status}");
                return null;
            }

            Console.WriteLine("Creating X509 DeviceClient authentication.");
            IAuthenticationMethod auth = new DeviceAuthenticationWithX509Certificate(result.DeviceId, (_security as SecurityProviderX509).GetAuthenticationCertificate());

            return DeviceClient.Create(result.AssignedHub, auth, TransportType.Amqp);
        }

        private void VerifyRegistrationIdFormat(string v)
        {
            var r = new Regex("^[a-z0-9-]*$");
            if (!r.IsMatch(v))
            {
                throw new FormatException("Invalid registrationId: The registration ID is alphanumeric, lowercase, and may contain hyphens");
            }
        }
    }
}
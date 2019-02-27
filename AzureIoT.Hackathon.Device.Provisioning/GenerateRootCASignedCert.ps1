# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

$caRoot = Get-ChildItem -Path Cert:\LocalMachine\My\574D9333C86537BC99070353EF1A8C8C50B0B5AE

for($i=0;$i -le 20;$i++)
{
    $subjectName = "CN=device" + $i + "-azurehackathon-azhack2019" + "-" + ($i)%2 + "-" + ($i)%10;
    $cert = New-SelfSignedCertificate -Type Custom -KeySpec Signature -Subject $subjectName -KeyExportPolicy Exportable -HashAlgorithm sha256 -KeyLength 2048 -CertStoreLocation "Cert:\LocalMachine\My" -Signer $caRoot -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2")
    $cert.Thumbprint
}

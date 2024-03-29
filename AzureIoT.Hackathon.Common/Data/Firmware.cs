﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoT.Hackathon.Common.Data
{
    using Newtonsoft.Json;
    using System;

    public class Firmware
    {
        [JsonProperty(PropertyName = "version")]
        public Version Version { get; set; }

        [JsonProperty(PropertyName = "url")]
        public Uri DownloadUrl { get; set; }

        public Firmware Clone()
        {
            return new Firmware
            {
                DownloadUrl = new Uri(this.DownloadUrl.ToString()),
                Version = this.Version.Clone()
            };
        }
    }

    public class Version : IEquatable<Version>, IComparable<Version>
    {
        [JsonProperty(PropertyName = "major")]
        public int Major { get; set; }

        [JsonProperty(PropertyName = "minor")]
        public int Minor { get; set; }

        public Version Clone()
        {
            return new Version
            {
                Major = this.Major,
                Minor = this.Minor,
            };
        }

        public int CompareTo(Version other)
        {
            if ((object)other == null)
            {
                return 1;
            }

            if (this.Major.CompareTo(other.Major) != 0)
            {
                return this.Major.CompareTo(other.Major);
            }

            if (this.Minor.CompareTo(other.Minor) != 0)
            {
                return this.Minor.CompareTo(other.Minor);
            }

            return 0;
        }

        public bool Equals(Version other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return this.Major == other.Major && this.Minor == other.Minor;
        }
    }
}
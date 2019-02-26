namespace AzureIoT.Hackathon.Device.Client
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

        [JsonProperty(PropertyName = "patch")]
        public int Patch { get; set; }

        public Version Clone()
        {
            return new Version
            {
                Major = this.Major,
                Minor = this.Minor,
                Patch = this.Patch,
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

            if (this.Patch.CompareTo(other.Patch) != 0)
            {
                return this.Patch.CompareTo(other.Patch);
            }

            return 0;
        }

        public bool Equals(Version other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return this.Major == other.Major && this.Minor == other.Minor && this.Patch == other.Patch;
        }
    }
}

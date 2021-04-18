using Newtonsoft.Json;

namespace IFToolsBriefings.Shared.Data.Types
{
    public class VersionFile
    {
        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
using Newtonsoft.Json;

namespace DummyRestAPI.Objects
{
    public class EmployeePayload
    {
        [JsonProperty("ame")]
        public string name { get; set; }

        [JsonProperty("salary")]
        public string salary { get; set; }

        [JsonProperty("age")]
        public string age { get; set; }
    }
}


using Newtonsoft.Json;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class UserInfoModel
    {
        [JsonProperty("sub")]
        public string Sub { get; set; }

        [JsonProperty("roles")]
        public string Roles { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("given_name")]
        public string GivenName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}

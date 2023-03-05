using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EtherScanDemo.Helper
{
    public static class APIIntegrationHelper
    {
        public static async Task<JObject> GetResponseContent(HttpResponseMessage response)
        {
            var stringResponse = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<JObject>(stringResponse);

            return result;
        }
    }
}

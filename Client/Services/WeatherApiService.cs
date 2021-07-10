using System.Net.Http;
using System.Threading.Tasks;
using IFToolsBriefings.Shared.Data.Types;
using Newtonsoft.Json;

namespace IFToolsBriefings.Client.Services
{
    public static class WeatherApiService
    {
        private static readonly HttpClient Http = new ();
        
        private static string _baseUrl = "";

        public static void SetBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl + "WeatherApi";
        }
        
        public static async Task<ParsedMetar> GetMetarForStation(string weatherStationId)
        {
            var resultString = await Http.GetStringAsync($"{_baseUrl}/GetMetarForStation?weatherStationId={weatherStationId}");

            if (resultString == "null") return null;
            return JsonConvert.DeserializeObject<ParsedMetar>(resultString);
        }
    }
}

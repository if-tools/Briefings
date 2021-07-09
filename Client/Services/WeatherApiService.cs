using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using IFToolsBriefings.Shared.Data.Types;

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
            Console.WriteLine($"{_baseUrl}/GetMetarForStation?weatherStationId={weatherStationId}");
            return await Http.GetFromJsonAsync<ParsedMetar>($"{_baseUrl}/GetMetarForStation?weatherStationId={weatherStationId}");
        }
    }
}

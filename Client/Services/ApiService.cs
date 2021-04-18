using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using IFToolsBriefings.Shared.Data.Models;
using IFToolsBriefings.Shared.Data.Types;

namespace IFToolsBriefings.Client.Services
{
    public static class ApiService
    {
        private static readonly HttpClient Http = new ();
        
        private static string _baseUrl = "";

        public static void SetBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl + "api";
        }
        
        public static async Task<bool> CheckIfBriefingExists(int id)
        {
            return await Http.GetFromJsonAsync<bool>($"{_baseUrl}/CheckIfBriefingExists?id={id}");
        }
        
        public static async Task<Briefing> GetBriefing(int id, string viewPassword = null)
        {
            return await Http.GetFromJsonAsync<Briefing>($"{_baseUrl}/GetBriefing?id={id}&viewPassword={viewPassword}");
        }
        
        public static async Task<Briefing> GetBriefingToEdit(int id, string editPassword)
        {
            return await Http.GetFromJsonAsync<Briefing>($"{_baseUrl}/GetBriefingToEdit?id={id}&editPassword={editPassword}");
        }
        
        public static async Task<Briefing[]> GetBriefings(BriefingSearchMethod searchMethod, string query)
        {
            return await Http.GetFromJsonAsync<Briefing[]>($"{_baseUrl}/GetBriefings?searchMethod={searchMethod}&query={query}");
        }

        public static async Task<string> MakeBriefing(Briefing newBriefing)
        {
            var response = await Http.PostAsJsonAsync($"{_baseUrl}/MakeBriefing", newBriefing);

            return await response.Content.ReadAsStringAsync();
        }
        
        public static async Task EditBriefing(int id, Briefing editedBriefing, string editPassword)
        {
            await Http.PostAsJsonAsync($"{_baseUrl}/EditBriefing", new EditBriefingPostParameters(id, editedBriefing, editPassword));
        }
        
        public static async Task<bool> CheckPassword(int id, string editPassword = null, string viewPassword = null)
        {
            return await Http.GetFromJsonAsync<bool>($"{_baseUrl}/CheckPassword?id={id}&editPassword={editPassword}&viewPassword={viewPassword}");
        }
        
        public static async Task<FileAttachment[]> GetAttachments(string guidsJson)
        {
            return await Http.GetFromJsonAsync<FileAttachment[]>($"{_baseUrl}/GetAttachments?guidsJson={guidsJson}");
        }

        public static async Task<string> GetAppVersion()
        {
            return await Http.GetStringAsync($"{_baseUrl}/GetAppVersion");
        }

        public static async Task<string> GetFlightFpl(string callSign)
        {
            return await Http.GetFromJsonAsync<string>($"{_baseUrl}/if/IfApi/GetFlightFpl?callSign={callSign}");
        }
    }
}

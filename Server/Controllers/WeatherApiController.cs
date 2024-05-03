using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using IFToolsBriefings.Server.Data;
using IFToolsBriefings.Shared.Data.Types;
using Metar.Decoder;
using Metar.Decoder.Entity;
using Microsoft.AspNetCore.Mvc;

namespace IFToolsBriefings.Server.Controllers
{
    /// <summary>
    /// This uses the meteorological data provided by aviationweather.gov.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class WeatherApiController : Controller
    {
        private const string BaseUrl = "https://aviationweather.gov/api/data/metar?ids={0}";
        private static readonly HttpClient Http = new ();

        [HttpGet("[action]")]
        public async Task<ActionResult<ParsedMetar>> GetMetarForStation(string weatherStationId)
        {
            var httpResponse = await Http.GetAsync(string.Format(BaseUrl, weatherStationId));
            if(!httpResponse.IsSuccessStatusCode)
            {
                return null;
            }
            
            var rawText = await httpResponse.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(rawText)) return null;
            
            var rawMetar = rawText.Replace("\n", "");

            DecodedMetar decodedMetar = null;
            var valid = true;

            try
            {
                decodedMetar = new MetarDecoder().Parse(rawMetar);
            }
            catch
            {
                valid = false;
            }

            if (valid == false || decodedMetar == null)
            {
                return new ParsedMetar { IsValid = false, RawMetar = rawMetar };
            }

            var parsedMetar = MetarHelper.ConstructParsedMetar(decodedMetar);
            return parsedMetar;
        }
    }
}
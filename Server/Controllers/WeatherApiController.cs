using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using ENG.WMOCodes.Codes;
using ENG.WMOCodes.Decoders;
using IFToolsBriefings.Server.Data;
using IFToolsBriefings.Shared.Data.Types;
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
        private const string BaseUrl = "https://www.aviationweather.gov/adds/dataserver_current/httpparam?dataSource=metars&requestType=retrieve&format=xml&hoursBeforeNow=3&mostRecent=true&stationString=";
        private static readonly HttpClient Http = new ();

        [HttpGet("[action]")]
        public async Task<ActionResult<ParsedMetar>> GetMetarForStation(string weatherStationId)
        {
            var httpResponse = await Http.GetAsync(BaseUrl + weatherStationId);
            if(!httpResponse.IsSuccessStatusCode)
            {
                return null;
            }
            
            var rawText = await httpResponse.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(rawText)) return null;

            var xml = XDocument.Parse(rawText);

            var xMetar = xml.Root?.Descendants("data").Descendants("METAR").FirstOrDefault();
            if (xMetar == null) return null;
            
            var rawMetar = "METAR " + xMetar.Element("raw_text")?.Value;

            Metar decodedMetar = null;
            var valid = true;

            try
            {
                decodedMetar = new MetarDecoder().Decode(rawMetar);
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
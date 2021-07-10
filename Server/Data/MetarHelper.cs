using System;
using ENG.WMOCodes.Codes;
using ENG.WMOCodes.Types;
using IFToolsBriefings.Shared.Data.Types;

namespace IFToolsBriefings.Server.Data
{
    public class MetarHelper
    {
        public static ParsedMetar ConstructParsedMetar(Metar decodedMetar)
        {
            if (decodedMetar == null) return new ParsedMetar {IsValid = false};
            
            var parsedMetar = new ParsedMetar();

            parsedMetar.IsValid = true;
            parsedMetar.RawMetar = decodedMetar.ToCode();
            parsedMetar.ReportTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, decodedMetar.Date.Day,
                decodedMetar.Date.Hour, decodedMetar.Date.Minute, 0);
            parsedMetar.Temperature = decodedMetar.Temperature;

            var decodedPhenomena = decodedMetar.Phenomena;
            var cloudLayers = decodedMetar.Clouds;
            var weatherConditions = WeatherConditions.Clear;

            // clouds (any)
            if (cloudLayers.Count > 0)
            {
                weatherConditions = WeatherConditions.Clouds;
            }
            
            // rain / showers
            if (decodedPhenomena.Contains(Phenomenon.RA) || decodedPhenomena.Contains(Phenomenon.SH))
            {
                weatherConditions = WeatherConditions.Rain;
            }
            
            // snow
            if (decodedPhenomena.Contains(Phenomenon.SN))
            {
                weatherConditions = WeatherConditions.Snow;
            }
            
            // thunderstorm
            if (decodedPhenomena.Contains(Phenomenon.TS))
            {
                weatherConditions = WeatherConditions.Thunderstorm;
            }

            parsedMetar.WeatherConditions = weatherConditions;
            parsedMetar.WindDirection = decodedMetar.Wind.Direction ?? 0;
            // convert to kts if needed
            parsedMetar.WindSpeed = decodedMetar.Wind.Unit == SpeedUnit.mps ? (int)Math.Ceiling(decodedMetar.Wind.Speed / 0.514f) : decodedMetar.Wind.Speed;
            parsedMetar.WindGusts = decodedMetar.Wind.GustSpeed ?? 0;

            return parsedMetar;
        }
    }
}
using System;
using IFToolsBriefings.Shared.Data.Types;
using Metar.Decoder.Entity;

namespace IFToolsBriefings.Server.Data
{
    public class MetarHelper
    {
        public static ParsedMetar ConstructParsedMetar(DecodedMetar decodedMetar)
        {
            if (decodedMetar == null) return new ParsedMetar {IsValid = false};
            
            var parsedMetar = new ParsedMetar();

            parsedMetar.IsValid = true;
            parsedMetar.RawMetar = decodedMetar.RawMetar;
            parsedMetar.ReportTime = new DateTime();
            parsedMetar.Temperature = (int)decodedMetar.AirTemperature.ActualValue;

            var decodedPhenomena = decodedMetar.PresentWeather;
            var cloudLayers = decodedMetar.Clouds;
            var weatherConditions = WeatherConditions.Clear;

            // clouds (any)
            if (cloudLayers.Count > 0)
            {
                weatherConditions = WeatherConditions.Clouds;
            }
            
            /*
            // rain / showers
            if (decodedPhenomena.Contains(new WeatherPhenomenon || decodedPhenomena.Contains(Phenomenon.SH))
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
            }*/

            parsedMetar.WeatherConditions = weatherConditions;
            parsedMetar.WindDirection = (int)decodedMetar.SurfaceWind.MeanDirection.ActualValue;
            parsedMetar.WindSpeed = (int)decodedMetar.SurfaceWind.MeanSpeed.ActualValue;

            return parsedMetar;
        }
    }
}
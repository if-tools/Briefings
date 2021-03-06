using System;

namespace IFToolsBriefings.Shared.Data.Types
{
    public class ParsedMetar
    {
        public bool IsValid { get; set; }
        public string RawMetar { get; set; }
        public WeatherConditions WeatherConditions { get; set; }
        public int WindDirection { get; set; }
        public int WindSpeed { get; set; }
        public int WindGusts { get; set; }
        public int Temperature { get; set; }
        public DateTime ReportTime { get; set; }

        public static ParsedMetar Default => new ();
        public static ParsedMetar Loading => new () { RawMetar = "Loading..."};
        public static ParsedMetar StationNotFound => new () { RawMetar = "No such weather station found."};

    }
}
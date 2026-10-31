// Visual weather categories the environment knows how to show.
public enum WeatherState
{
    Clear,
    Cloudy,
    Rain,
    Snow,
    Fallback   // anything unsupported (fog, haze, dust...) or missing
}

// Cleaned-up weather data shared by the UI and the environment, so both always
// show the same response. Nullable values mean "missing from the response".
public class WeatherReport
{
    public string LocationLabel { get; set; }
    public string CoordinatesLabel { get; set; }
    public string Description { get; set; }
    public float? TemperatureC { get; set; }
    public int? Humidity { get; set; }
    public float? WindSpeed { get; set; }
    public int? CloudCoverage { get; set; }
    public bool? IsDay { get; set; }
    public string ObservedTime { get; set; }        // when the station took the reading, in the location's time
    public int? TimezoneOffsetSeconds { get; set; }  // location's shift from UTC, used for the live clock
    public WeatherState State { get; set; }
    public bool IsSample { get; set; }
    public string SourceLabel { get; set; }

    // Neutral look used before any data arrives or after a failed request.
    public static WeatherReport Neutral(bool isDay, WeatherState state = WeatherState.Fallback)
    {
        return new WeatherReport { IsDay = isDay, State = state, WindSpeed = 1f, CloudCoverage = 40 };
    }
}

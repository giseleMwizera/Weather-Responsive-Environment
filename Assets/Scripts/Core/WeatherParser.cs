using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

// Turns raw JSON (live or sample) into a WeatherReport. Live and test data both go through here.
public static class WeatherParser
{
    public static RequestResult<WeatherReport> Parse(string json, WeatherRequest request)
    {
        if (string.IsNullOrWhiteSpace(json))
            return RequestResult<WeatherReport>.Fail("The weather service returned an empty response.");

        WeatherResponse data;
        try
        {
            data = JsonUtility.FromJson<WeatherResponse>(json);
        }
        catch (ArgumentException)
        {
            return RequestResult<WeatherReport>.Fail("The weather data could not be read (invalid JSON).");
        }
        if (data == null)
            return RequestResult<WeatherReport>.Fail("The weather data could not be read.");

        // JsonUtility creates empty objects for missing keys, so check the raw JSON
        // to avoid showing 0 °C or 0% when the value was never sent.
        bool hasMain = HasObject(json, "main");
        bool hasWind = HasObject(json, "wind");
        bool hasClouds = HasObject(json, "clouds");
        WeatherCondition condition = data.weather != null && data.weather.Length > 0 ? data.weather[0] : null;

        // Coordinates from the response when present (sample files), otherwise the ones requested.
        bool hasCoord = HasObject(json, "coord");
        float lat = hasCoord ? data.coord.lat : request.Latitude;
        float lon = hasCoord ? data.coord.lon : request.Longitude;
        string coordinates = FormatCoordinates(lat, lon);

        var report = new WeatherReport
        {
            LocationLabel = BuildLocationLabel(data, coordinates),
            CoordinatesLabel = coordinates,
            Description = condition != null && !string.IsNullOrEmpty(condition.description)
                ? Capitalise(condition.description)
                : "Description unavailable",
            TemperatureC = hasMain ? data.main.temp : (float?)null,
            Humidity = hasMain ? data.main.humidity : (int?)null,
            WindSpeed = hasWind ? data.wind.speed : (float?)null,
            CloudCoverage = hasClouds ? data.clouds.all : (int?)null,
            IsDay = WeatherClassifier.IsDaytime(condition, data),
            ObservedTime = FormatObservedTime(data),
            TimezoneOffsetSeconds = Regex.IsMatch(json, "\"timezone\"\\s*:") ? data.timezone : (int?)null,
            State = WeatherClassifier.Classify(condition),
            IsSample = request.IsSample,
            SourceLabel = BuildSourceLabel(request, FormatObservedTime(data))
        };
        return RequestResult<WeatherReport>.Ok(report);
    }

    public static string FormatCoordinates(float lat, float lon)
    {
        return string.Format(CultureInfo.InvariantCulture, "Lat {0:0.####}, Lon {1:0.####}", lat, lon);
    }

    // Name and country when the API has them, otherwise the coordinates that were requested.
    static string BuildLocationLabel(WeatherResponse data, string coordinates)
    {
        if (string.IsNullOrWhiteSpace(data.name))
            return coordinates;

        string country = data.sys != null ? data.sys.country : null;
        return string.IsNullOrWhiteSpace(country) ? data.name : $"{data.name}, {country}";
    }

    // Time of the reading in the location's own time zone (dt + timezone offset).
    static string FormatObservedTime(WeatherResponse data)
    {
        if (data.dt <= 0)
            return null;
        return FormatLocationTime(DateTimeOffset.FromUnixTimeSeconds(data.dt), data.timezone);
    }

    // Shifts a UTC moment by the location's offset. The computer's own time zone is never used.
    public static string FormatLocationTime(DateTimeOffset utc, int offsetSeconds)
    {
        DateTime local = utc.UtcDateTime.AddSeconds(offsetSeconds);
        return local.ToString("ddd HH:mm", CultureInfo.InvariantCulture);
    }

    static string BuildSourceLabel(WeatherRequest request, string observed)
    {
        string source = request.IsSample
            ? $"TEST MODE sample: {request.Sample.FileName}.json"
            : "Live data from OpenWeather";
        return observed == null ? source : $"{source}\nReading taken {observed} local time";
    }

    static bool HasObject(string json, string key)
    {
        return Regex.IsMatch(json, "\"" + key + "\"\\s*:\\s*\\{");
    }

    static string Capitalise(string text)
    {
        return char.ToUpperInvariant(text[0]) + text.Substring(1);
    }
}

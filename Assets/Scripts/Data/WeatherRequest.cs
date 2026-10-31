using System;

// A named place with coordinates, used for the preset dropdown.
[Serializable]
public class Location
{
    public string name;
    public float latitude;
    public float longitude;

    public Location(string name, float latitude, float longitude)
    {
        this.name = name;
        this.latitude = latitude;
        this.longitude = longitude;
    }
}

// A canned JSON response used by test mode. HttpStatus other than 200 simulates a failed call.
public class SampleScenario
{
    public string Label { get; }
    public string FileName { get; }
    public long HttpStatus { get; }

    public SampleScenario(string label, string fileName, long httpStatus = 200)
    {
        Label = label;
        FileName = fileName;
        HttpStatus = httpStatus;
    }
}

// Everything needed to ask for weather once. Kept so the refresh button can repeat it.
public class WeatherRequest
{
    public string Label { get; }
    public float Latitude { get; }
    public float Longitude { get; }
    public SampleScenario Sample { get; }
    public bool IsSample => Sample != null;

    public WeatherRequest(string label, float latitude, float longitude, SampleScenario sample = null)
    {
        Label = label;
        Latitude = latitude;
        Longitude = longitude;
        Sample = sample;
    }
}

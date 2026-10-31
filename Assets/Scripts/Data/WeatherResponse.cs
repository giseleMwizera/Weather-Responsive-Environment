using System;

// Mirrors the JSON returned by OpenWeather's Current Weather Data API.
// JsonUtility fills fields by name, so each field name must match its JSON key exactly.
[Serializable]
public class WeatherResponse
{
    public Coord coord;
    public WeatherCondition[] weather;   // array, the first entry is the primary condition
    public MainData main;
    public int visibility;
    public Wind wind;
    public Clouds clouds;
    public long dt;                      // observation time, Unix seconds (UTC)
    public SysData sys;
    public int timezone;                 // shift from UTC in seconds
    public string name;
    public int cod;
}

[Serializable]
public class Coord
{
    public float lon;
    public float lat;
}

[Serializable]
public class WeatherCondition
{
    public int id;              // condition code, e.g. 500 = light rain, 800 = clear
    public string main;         // group, e.g. "Rain"
    public string description;  // e.g. "light rain"
    public string icon;         // e.g. "10n", last letter is d (day) or n (night)
}

[Serializable]
public class MainData
{
    public float temp;
    public float feels_like;
    public float temp_min;
    public float temp_max;
    public int pressure;
    public int humidity;
}

[Serializable]
public class Wind
{
    public float speed;   // m/s with units=metric
    public int deg;
    public float gust;
}

[Serializable]
public class Clouds
{
    public int all;       // cloud coverage in %
}

[Serializable]
public class SysData
{
    public string country;
    public long sunrise;  // Unix seconds (UTC)
    public long sunset;
}

// Body OpenWeather sends back when a request fails, e.g. {"cod":401,"message":"Invalid API key..."}
[Serializable]
public class ApiError
{
    public string message;
}

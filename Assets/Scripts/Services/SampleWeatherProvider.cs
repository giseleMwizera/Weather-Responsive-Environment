using System;
using System.Collections;
using UnityEngine;

// Test-mode provider: returns JSON files from Resources/SampleWeather instead of calling the API.
// The JSON then goes through the same parser, UI and environment as live data.
public class SampleWeatherProvider : IWeatherProvider
{
    const float SimulatedDelay = 0.6f;   // long enough to see the loading indicator

    public static readonly SampleScenario[] Scenarios =
    {
        new SampleScenario("Clear, day", "clear_day"),
        new SampleScenario("Clear, night", "clear_night"),
        new SampleScenario("Cloudy, day", "cloudy_day"),
        new SampleScenario("Cloudy, night", "cloudy_night"),
        new SampleScenario("Rain, day", "rain_day"),
        new SampleScenario("Rain, night", "rain_night"),
        new SampleScenario("Snow, day", "snow_day"),
        new SampleScenario("Snow, night", "snow_night"),
        new SampleScenario("Fog (unsupported, fallback look)", "fog_fallback"),
        new SampleScenario("Missing data in response", "missing_data"),
        new SampleScenario("Broken JSON response", "malformed"),
        new SampleScenario("API failure (401 bad key)", "api_error_401", 401),
        new SampleScenario("API failure (city not found 404)", "api_error_404", 404),
    };

    public IEnumerator FetchJson(WeatherRequest request, Action<RequestResult<string>> onComplete)
    {
        yield return new WaitForSeconds(SimulatedDelay);

        TextAsset file = Resources.Load<TextAsset>("SampleWeather/" + request.Sample.FileName);
        if (file == null)
        {
            onComplete(RequestResult<string>.Fail($"Sample file '{request.Sample.FileName}' was not found."));
            yield break;
        }

        if (request.Sample.HttpStatus != 200)
        {
            // Same error handling the live provider uses for a failed HTTP call.
            onComplete(RequestResult<string>.Fail(WeatherErrors.Describe(request.Sample.HttpStatus, file.text)));
            yield break;
        }

        onComplete(RequestResult<string>.Ok(file.text));
    }
}

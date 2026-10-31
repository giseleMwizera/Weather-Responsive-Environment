using System;
using System.Collections;

// Anything that can hand back weather JSON: the live API or the test-mode samples.
// The service does not care which one it talks to, so both share the same pipeline.
public interface IWeatherProvider
{
    IEnumerator FetchJson(WeatherRequest request, Action<RequestResult<string>> onComplete);
}

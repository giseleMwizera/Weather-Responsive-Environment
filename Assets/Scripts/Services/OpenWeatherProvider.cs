using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

// Live provider: calls the OpenWeather Current Weather Data API with UnityWebRequest.
public class OpenWeatherProvider : IWeatherProvider
{
    const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";
    const int TimeoutSeconds = 10;

    readonly string apiKey;

    public OpenWeatherProvider(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public IEnumerator FetchJson(WeatherRequest request, Action<RequestResult<string>> onComplete)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            onComplete(RequestResult<string>.Fail(
                "No API key yet. Paste your OpenWeather key in the API key field and click Save key. Test mode works without a key."));
            yield break;
        }

        string url = string.Format(CultureInfo.InvariantCulture,
            "{0}?lat={1}&lon={2}&appid={3}&units=metric",
            BaseUrl, request.Latitude, request.Longitude, apiKey);

        using (UnityWebRequest web = UnityWebRequest.Get(url))
        {
            web.timeout = TimeoutSeconds;
            yield return web.SendWebRequest();

            if (web.result == UnityWebRequest.Result.Success)
            {
                onComplete(RequestResult<string>.Ok(web.downloadHandler.text));
            }
            else
            {
                // responseCode is 0 when the server was never reached (offline, timeout, blocked).
                long status = web.result == UnityWebRequest.Result.ProtocolError ? web.responseCode : 0;
                string body = web.downloadHandler != null ? web.downloadHandler.text : null;
                Debug.LogWarning($"Weather request failed: {web.result} {web.responseCode} {web.error}");
                onComplete(RequestResult<string>.Fail(WeatherErrors.Describe(status, body)));
            }
        }
    }
}

using System;
using System.Collections;
using UnityEngine;

// Runs weather requests and announces the results through events.
// The UI and the environment subscribe to these events and never call each other.
public class WeatherService : MonoBehaviour
{
    const string ApiKeyResource = "WeatherApiKey";   // Assets/Resources/WeatherApiKey.txt (git-ignored)
    const string SavedKeyName = "OpenWeatherApiKey";

    public event Action<WeatherRequest> RequestStarted;
    public event Action<WeatherReport> WeatherUpdated;
    public event Action<string> WeatherFailed;

    IWeatherProvider liveProvider;
    IWeatherProvider sampleProvider;
    WeatherRequest lastRequest;
    int requestVersion;   // lets a newer request win if an older one finishes late

    public bool HasRequest => lastRequest != null;

    void Awake()
    {
        liveProvider = new OpenWeatherProvider(LoadApiKey());
        sampleProvider = new SampleWeatherProvider();
    }

    public void RequestLive(string label, float latitude, float longitude)
    {
        Send(new WeatherRequest(label, latitude, longitude));
    }

    public void RequestSample(SampleScenario scenario)
    {
        Send(new WeatherRequest(scenario.Label, 0f, 0f, scenario));
    }

    // Repeats the last request, live or sample.
    public void Refresh()
    {
        if (lastRequest != null)
            Send(lastRequest);
    }

    void Send(WeatherRequest request)
    {
        lastRequest = request;
        requestVersion++;
        StartCoroutine(Run(request, requestVersion));
    }

    IEnumerator Run(WeatherRequest request, int version)
    {
        RequestStarted?.Invoke(request);

        IWeatherProvider provider = request.IsSample ? sampleProvider : liveProvider;
        RequestResult<string> raw = RequestResult<string>.Fail("No response.");
        yield return provider.FetchJson(request, result => raw = result);

        if (version != requestVersion)
            yield break;   // a newer request was made while this one was running

        if (!raw.Success)
        {
            WeatherFailed?.Invoke(raw.Error);
            yield break;
        }

        RequestResult<WeatherReport> parsed = WeatherParser.Parse(raw.Data, request);
        if (parsed.Success)
            WeatherUpdated?.Invoke(parsed.Data);
        else
            WeatherFailed?.Invoke(parsed.Error);
    }

    // Used by the API key field in the UI, so the WebGL build on GitHub Pages ships without a key.
    // PlayerPrefs is browser storage in WebGL, so the key is remembered on that computer only.
    public void SaveApiKey(string key)
    {
        key = key.Trim();
        PlayerPrefs.SetString(SavedKeyName, key);
        PlayerPrefs.Save();
        liveProvider = new OpenWeatherProvider(key);
    }

    public bool HasApiKey => !string.IsNullOrEmpty(LoadApiKey());

    // The local key file wins (Unity editor), otherwise the key saved from the UI.
    static string LoadApiKey()
    {
        TextAsset keyFile = Resources.Load<TextAsset>(ApiKeyResource);
        if (keyFile != null && keyFile.text.Trim().Length > 0)
            return keyFile.text.Trim();
        return PlayerPrefs.GetString(SavedKeyName, "");
    }
}

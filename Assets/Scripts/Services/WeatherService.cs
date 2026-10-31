using System;
using System.Collections;
using UnityEngine;

// Runs weather requests and announces the results through events.
// The UI and the environment subscribe to these events and never call each other.
public class WeatherService : MonoBehaviour
{
    const string ApiKeyResource = "WeatherApiKey";   // Assets/Resources/WeatherApiKey.txt (git-ignored)

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

    static string LoadApiKey()
    {
        TextAsset keyFile = Resources.Load<TextAsset>(ApiKeyResource);
        return keyFile != null ? keyFile.text.Trim() : null;
    }
}

using UnityEngine;

// Rain and wind sounds generated from filtered noise, so no audio files are needed.
// Rain plays during rain; wind volume follows the API's wind speed.
public class WeatherAudio : MonoBehaviour, IWeatherEffect
{
    [SerializeField] AudioSource rainSource;
    [SerializeField] AudioSource windSource;
    [SerializeField] float rainVolume = 0.35f;
    [SerializeField] float maxWindVolume = 0.3f;
    [SerializeField] float fadeSpeed = 0.4f;

    const int SampleRate = 44100;

    float rainTarget;
    float windTarget;

    void Awake()
    {
        rainSource.clip = CreateNoise("Rain", 0.35f, false);
        windSource.clip = CreateNoise("Wind", 0.015f, true);
        foreach (AudioSource source in new[] { rainSource, windSource })
        {
            source.loop = true;
            source.volume = 0f;
            source.Play();
        }
    }

    public void Apply(WeatherReport report, bool instant)
    {
        rainTarget = report.State == WeatherState.Rain ? rainVolume : 0f;
        float windAmount = Mathf.Clamp01((report.WindSpeed ?? 0f) / 15f);
        windTarget = Mathf.Lerp(0.03f, maxWindVolume, windAmount);

        if (instant && rainSource != null && windSource != null)
        {
            rainSource.volume = rainTarget;
            windSource.volume = windTarget;
        }
    }

    void Update()
    {
        rainSource.volume = Mathf.MoveTowards(rainSource.volume, rainTarget, fadeSpeed * Time.deltaTime);
        windSource.volume = Mathf.MoveTowards(windSource.volume, windTarget, fadeSpeed * Time.deltaTime);
    }

    // White noise through a simple low-pass filter. Lower smoothing = deeper sound.
    static AudioClip CreateNoise(string clipName, float smoothing, bool gusts)
    {
        int length = SampleRate * 4;
        var data = new float[length];
        var random = new System.Random(7);
        float filtered = 0f;
        float peak = 0.0001f;

        for (int i = 0; i < length; i++)
        {
            float white = (float)(random.NextDouble() * 2.0 - 1.0);
            filtered += smoothing * (white - filtered);
            float gain = gusts ? 0.6f + 0.4f * Mathf.Sin(i * 2f * Mathf.PI / length) : 1f;
            data[i] = filtered * gain;
            peak = Mathf.Max(peak, Mathf.Abs(data[i]));
        }

        // Normalise and fade the loop seam to avoid clicks.
        int fade = SampleRate / 10;
        for (int i = 0; i < length; i++)
        {
            float edge = Mathf.Min(1f, Mathf.Min(i, length - 1 - i) / (float)fade);
            data[i] = data[i] / peak * 0.8f * edge;
        }

        AudioClip clip = AudioClip.Create(clipName, length, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}

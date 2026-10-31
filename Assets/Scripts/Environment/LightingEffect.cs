using System.Collections;
using UnityEngine;

// Changes the skybox, sun, fog, street lamps and ground to match the weather and time of day.
// Skyboxes come from the "Day-Night Skyboxes" asset pack.
public class LightingEffect : MonoBehaviour, IWeatherEffect
{
    [Header("Skyboxes")]
    [SerializeField] Material clearDaySky;
    [SerializeField] Material clearNightSky;
    [SerializeField] Material cloudyDaySky;
    [SerializeField] Material cloudyNightSky;
    [SerializeField] Material stormDaySky;     // rain and snow
    [SerializeField] Material stormNightSky;

    [Header("Scene")]
    [SerializeField] Light sun;
    [SerializeField] Light[] streetLamps;
    [SerializeField] Renderer ground;
    [SerializeField] float fadeTime = 1.5f;

    [Header("Ground colours")]
    [SerializeField] Color grass = new Color(0.36f, 0.55f, 0.3f);
    [SerializeField] Color wetGrass = new Color(0.22f, 0.36f, 0.2f);
    [SerializeField] Color snow = new Color(0.92f, 0.94f, 0.98f);

    Coroutine fade;

    public void Apply(WeatherReport report, bool instant)
    {
        bool isDay = report.IsDay != false;   // unknown counts as day
        WeatherState state = report.State;

        // Work out how the scene should look.
        Material sky = PickSky(state, isDay);
        float sunPower = isDay ? 1.3f : 0.25f;
        if (state != WeatherState.Clear)
            sunPower *= 0.55f;
        sunPower *= 1f - (report.CloudCoverage ?? 0) / 250f;   // more cloud cover, dimmer sun

        Color ambient = isDay ? new Color(0.6f, 0.64f, 0.7f) : new Color(0.1f, 0.12f, 0.22f);
        if (state != WeatherState.Clear)
            ambient *= 0.8f;

        Color groundColor = state == WeatherState.Snow ? snow : state == WeatherState.Rain ? wetGrass : grass;
        if (!isDay)
            groundColor *= 0.6f;

        Color fogColor = FogColor(state, isDay);
        float fogDensity = FogDensity(state);
        float lamps = isDay ? 0f : 12f;

        sun.color = isDay ? new Color(1f, 0.95f, 0.86f) : new Color(0.6f, 0.7f, 1f);   // warm sun, cool moon

        if (fade != null)
            StopCoroutine(fade);

        if (instant || !Application.isPlaying)
        {
            SetSky(sky, 1f);
            SetScene(sunPower, ambient, fogColor, fogDensity, groundColor, lamps);
        }
        else
        {
            fade = StartCoroutine(Fade(sky, sunPower, ambient, fogColor, fogDensity, groundColor, lamps));
        }
    }

    Material PickSky(WeatherState state, bool isDay)
    {
        switch (state)
        {
            case WeatherState.Clear:
                return isDay ? clearDaySky : clearNightSky;
            case WeatherState.Rain:
            case WeatherState.Snow:
                return isDay ? stormDaySky : stormNightSky;
            default:   // cloudy, and the fallback look for fog, haze, unknown codes or errors
                return isDay ? cloudyDaySky : cloudyNightSky;
        }
    }

    static Color FogColor(WeatherState state, bool isDay)
    {
        if (!isDay)
            return new Color(0.03f, 0.04f, 0.08f);
        switch (state)
        {
            case WeatherState.Clear: return new Color(0.75f, 0.83f, 0.92f);
            case WeatherState.Snow: return new Color(0.82f, 0.84f, 0.88f);
            case WeatherState.Rain: return new Color(0.45f, 0.47f, 0.5f);
            default: return new Color(0.62f, 0.64f, 0.67f);
        }
    }

    static float FogDensity(WeatherState state)
    {
        switch (state)
        {
            case WeatherState.Clear: return 0.002f;
            case WeatherState.Cloudy: return 0.006f;
            case WeatherState.Rain: return 0.015f;
            case WeatherState.Snow: return 0.02f;
            default: return 0.035f;   // fallback is a thick haze so it looks different from cloudy
        }
    }

    // Smooth transition: the old sky fades out, swaps, and the new one fades in while the lights blend.
    IEnumerator Fade(Material newSky, float sunPower, Color ambient, Color fogColor, float fogDensity, Color groundColor, float lamps)
    {
        float startSun = sun.intensity;
        Color startAmbient = RenderSettings.ambientLight;
        Color startFog = RenderSettings.fogColor;
        float startDensity = RenderSettings.fogDensity;
        Color startGround = GetGroundColor();
        float startLamps = streetLamps.Length > 0 ? streetLamps[0].intensity : 0f;
        bool swapped = false;

        for (float t = 0f; t < 1f; t += Time.deltaTime / fadeTime)
        {
            if (t >= 0.5f && !swapped)
            {
                SetSky(newSky, 0f);
                swapped = true;
            }
            float brightness = Mathf.Abs(t - 0.5f) * 2f;   // 1 -> 0 -> 1
            if (RenderSettings.skybox != null)
                RenderSettings.skybox.SetFloat("_Exposure", Mathf.Max(brightness, 0.15f));

            SetScene(Mathf.Lerp(startSun, sunPower, t), Color.Lerp(startAmbient, ambient, t),
                Color.Lerp(startFog, fogColor, t), Mathf.Lerp(startDensity, fogDensity, t),
                Color.Lerp(startGround, groundColor, t), Mathf.Lerp(startLamps, lamps, t));
            yield return null;
        }

        SetSky(newSky, 1f);
        SetScene(sunPower, ambient, fogColor, fogDensity, groundColor, lamps);
        fade = null;
    }

    // A copy of the material is used so Play mode never edits the asset file.
    void SetSky(Material source, float exposure)
    {
        if (source == null)
            return;
        RenderSettings.skybox = new Material(source);
        RenderSettings.skybox.SetFloat("_Exposure", exposure);
    }

    void SetScene(float sunPower, Color ambient, Color fogColor, float fogDensity, Color groundColor, float lamps)
    {
        sun.intensity = sunPower;
        RenderSettings.ambientLight = ambient;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;

        foreach (Light lamp in streetLamps)
        {
            lamp.intensity = lamps;
            lamp.enabled = lamps > 0.05f;
        }

        var block = new MaterialPropertyBlock();
        ground.GetPropertyBlock(block);
        block.SetColor("_BaseColor", groundColor);
        ground.SetPropertyBlock(block);
    }

    Color GetGroundColor()
    {
        var block = new MaterialPropertyBlock();
        ground.GetPropertyBlock(block);
        return block.isEmpty ? grass : block.GetColor("_BaseColor");
    }
}

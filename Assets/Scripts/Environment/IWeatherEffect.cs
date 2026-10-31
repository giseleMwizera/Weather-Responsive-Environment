// One visual part of the environment (lighting, rain, snow, clouds, audio...).
// The EnvironmentController calls every effect with the same report, and each effect
// decides for itself whether it should be on or off.
public interface IWeatherEffect
{
    // instant = skip the smooth transition (used for the first frame and editor screenshots).
    void Apply(WeatherReport report, bool instant);
}

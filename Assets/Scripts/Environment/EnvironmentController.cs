using UnityEngine;

// Listens to the WeatherService and passes each report to every IWeatherEffect under this object.
public class EnvironmentController : MonoBehaviour
{
    [SerializeField] WeatherService weatherService;

    IWeatherEffect[] effects;
    WeatherReport current;

    void Awake()
    {
        effects = GetComponentsInChildren<IWeatherEffect>(true);
    }

    void OnEnable()
    {
        weatherService.WeatherUpdated += HandleWeatherUpdated;
        weatherService.WeatherFailed += HandleWeatherFailed;
    }

    void OnDisable()
    {
        weatherService.WeatherUpdated -= HandleWeatherUpdated;
        weatherService.WeatherFailed -= HandleWeatherFailed;
    }

    void Start()
    {
        // Calm clear day until the first response arrives.
        ApplyReport(WeatherReport.Neutral(true, WeatherState.Clear), true);
    }

    public void ApplyReport(WeatherReport report, bool instant = false)
    {
        if (effects == null)
            effects = GetComponentsInChildren<IWeatherEffect>(true);

        current = report;
        foreach (IWeatherEffect effect in effects)
            effect.Apply(report, instant);
    }

    void HandleWeatherUpdated(WeatherReport report) => ApplyReport(report);

    // No trustworthy data: stop rain/snow and fall back to a neutral overcast look,
    // keeping the previous day/night so the scene does not jump.
    void HandleWeatherFailed(string error)
    {
        bool wasDay = current == null || current.IsDay != false;
        ApplyReport(WeatherReport.Neutral(wasDay));
    }
}

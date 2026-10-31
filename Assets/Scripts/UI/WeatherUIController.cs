using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Handles user input (presets, custom coordinates, refresh, test mode) and shows
// whatever the WeatherService reports through its events.
public class WeatherUIController : MonoBehaviour
{
    [SerializeField] WeatherService weatherService;

    [Header("Location input")]
    [SerializeField] TMP_Dropdown locationDropdown;
    [SerializeField] TMP_InputField latitudeInput;
    [SerializeField] TMP_InputField longitudeInput;
    [SerializeField] Button customButton;
    [SerializeField] TMP_Text validationText;
    [SerializeField] Button refreshButton;

    [Header("Test mode")]
    [SerializeField] TMP_Dropdown sampleDropdown;
    [SerializeField] Button sampleButton;
    [SerializeField] GameObject testModeBanner;

    [Header("Weather display")]
    [SerializeField] TMP_Text locationText;
    [SerializeField] TMP_Text coordinatesText;
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] TMP_Text temperatureText;
    [SerializeField] TMP_Text humidityText;
    [SerializeField] TMP_Text windText;
    [SerializeField] TMP_Text dayNightText;
    [SerializeField] TMP_Text localTimeText;
    [SerializeField] TMP_Text sourceText;
    [SerializeField] GameObject loadingIndicator;
    [SerializeField] GameObject errorPanel;
    [SerializeField] TMP_Text errorText;

    // Picked for different time zones and climates (tropical, desert, polar, wet, southern hemisphere).
    [SerializeField] List<Location> presetLocations = new List<Location>
    {
        new Location("Kigali, Rwanda", -1.9441f, 30.0619f),
        new Location("London, United Kingdom", 51.5074f, -0.1278f),
        new Location("Reykjavik, Iceland", 64.1466f, -21.9426f),
        new Location("Tokyo, Japan", 35.6762f, 139.6503f),
        new Location("Dubai, UAE", 25.2048f, 55.2708f),
        new Location("New York, USA", 40.7128f, -74.0060f),
        new Location("Sydney, Australia", -33.8688f, 151.2093f),
        new Location("Anchorage, USA", 61.2181f, -149.9003f),
    };

    const string Missing = "--";
    const string PlaceholderOption = "Choose a city...";

    int? clockOffsetSeconds;   // set while live data is shown, so the local time keeps ticking

    void Awake()
    {
        locationDropdown.ClearOptions();
        var names = new List<string> { PlaceholderOption };
        foreach (Location location in presetLocations)
            names.Add(location.name);
        locationDropdown.AddOptions(names);

        sampleDropdown.ClearOptions();
        var samples = new List<string>();
        foreach (SampleScenario scenario in SampleWeatherProvider.Scenarios)
            samples.Add(scenario.Label);
        sampleDropdown.AddOptions(samples);

        locationDropdown.onValueChanged.AddListener(OnPresetSelected);
        customButton.onClick.AddListener(OnCustomCoordinatesSubmitted);
        refreshButton.onClick.AddListener(weatherService.Refresh);
        sampleButton.onClick.AddListener(OnSampleRequested);
    }

    void OnEnable()
    {
        weatherService.RequestStarted += ShowLoading;
        weatherService.WeatherUpdated += ShowWeather;
        weatherService.WeatherFailed += ShowError;
    }

    void OnDisable()
    {
        weatherService.RequestStarted -= ShowLoading;
        weatherService.WeatherUpdated -= ShowWeather;
        weatherService.WeatherFailed -= ShowError;
    }

    void Start()
    {
        ClearWeatherFields();
        locationText.text = "No location yet";
        descriptionText.text = "Pick a city or enter coordinates.";
        loadingIndicator.SetActive(false);
        errorPanel.SetActive(false);
        testModeBanner.SetActive(false);
        validationText.text = "";
        refreshButton.interactable = false;
    }

    // Current time at the location: UTC now plus the offset the API returned.
    void Update()
    {
        if (clockOffsetSeconds.HasValue)
            localTimeText.text = WeatherParser.FormatLocationTime(System.DateTimeOffset.UtcNow, clockOffsetSeconds.Value);
    }

    // ---------- Input ----------

    void OnPresetSelected(int index)
    {
        if (index == 0)
            return;   // placeholder entry

        Location location = presetLocations[index - 1];
        validationText.text = "";
        weatherService.RequestLive(location.name, location.latitude, location.longitude);
    }

    void OnCustomCoordinatesSubmitted()
    {
        if (!CoordinateValidator.TryValidate(latitudeInput.text, longitudeInput.text,
                out float lat, out float lon, out string error))
        {
            validationText.text = error;   // nothing is sent when input is invalid
            return;
        }

        validationText.text = "";
        locationDropdown.SetValueWithoutNotify(0);
        weatherService.RequestLive(WeatherParser.FormatCoordinates(lat, lon), lat, lon);
    }

    void OnSampleRequested()
    {
        SampleScenario scenario = SampleWeatherProvider.Scenarios[sampleDropdown.value];
        locationDropdown.SetValueWithoutNotify(0);
        weatherService.RequestSample(scenario);
    }

    // ---------- Event handlers ----------

    void ShowLoading(WeatherRequest request)
    {
        loadingIndicator.SetActive(true);
        errorPanel.SetActive(false);
        testModeBanner.SetActive(request.IsSample);
        refreshButton.interactable = true;
        sourceText.text = request.IsSample ? "TEST MODE: loading sample data" : $"Requesting {request.Label}";
    }

    void ShowWeather(WeatherReport report)
    {
        loadingIndicator.SetActive(false);
        errorPanel.SetActive(false);
        testModeBanner.SetActive(report.IsSample);

        locationText.text = report.LocationLabel;
        coordinatesText.text = report.CoordinatesLabel;
        descriptionText.text = report.Description;
        temperatureText.text = report.TemperatureC.HasValue
            ? report.TemperatureC.Value.ToString("0.0", CultureInfo.InvariantCulture) + " °C"
            : Missing;
        humidityText.text = report.Humidity.HasValue ? report.Humidity.Value + " %" : Missing;
        windText.text = report.WindSpeed.HasValue
            ? report.WindSpeed.Value.ToString("0.0", CultureInfo.InvariantCulture) + " m/s"
            : Missing;
        dayNightText.text = report.IsDay.HasValue ? (report.IsDay.Value ? "Daytime" : "Nighttime") : "Unknown";
        // Samples are a fixed moment, so they show the time of the reading to match their day/night.
        clockOffsetSeconds = report.IsSample ? null : report.TimezoneOffsetSeconds;
        localTimeText.text = report.IsSample ? report.ObservedTime ?? Missing : Missing;
        Update();
        sourceText.text = report.SourceLabel;
    }

    // On failure the old values are cleared so the screen never shows stale weather as current.
    void ShowError(string message)
    {
        loadingIndicator.SetActive(false);
        ClearWeatherFields();
        locationText.text = "No weather data";
        descriptionText.text = "Request failed";
        errorText.text = message;
        errorPanel.SetActive(true);
        sourceText.text = testModeBanner.activeSelf ? "TEST MODE: simulated failure" : "";
    }

    void ClearWeatherFields()
    {
        clockOffsetSeconds = null;
        coordinatesText.text = "";
        temperatureText.text = Missing;
        humidityText.text = Missing;
        windText.text = Missing;
        dayNightText.text = Missing;
        localTimeText.text = Missing;
        sourceText.text = "";
    }
}

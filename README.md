# Weather-Responsive Environment

Unity app that shows the current weather for a location using the OpenWeather API. A small 3D village changes with the weather (clear, cloudy, rain, snow) and with day or night at that location.

Unity 6000.6.7f1 (URP), built for WebGL.

**Live WebGL build:** https://giselemwizera.github.io/Weather-Responsive-Environment/

## API key setup

The key is never committed. Get a free key at [openweathermap.org](https://openweathermap.org/) (My API keys), then:

- **WebGL build:** paste the key in the **API key** field and click **Save key**. It is stored in your browser only.
- **Unity editor:** create `Assets/Resources/WeatherApiKey.txt` and paste only the key into it. The file is in `.gitignore`.

Test mode works without a key.

## Run

Open `Assets/Scenes/WeatherScene.unity` and press Play.

## Features

- 8 preset cities and custom latitude/longitude input with validation
- Temperature, humidity, wind, description, day/night and local time
- Loading indicator and clear error messages
- Test mode with sample JSON (all weather states, day/night, fog fallback, missing data, API errors)
- Refresh button, smooth transitions, rain/wind audio, wind and cloud coverage affect the scene

## Assets

- [Fantasy Town Kit](https://kenney.nl/assets/fantasy-town-kit) by Kenney (CC0)
- Day-Night Skyboxes (Unity Asset Store)

# Weather-Responsive Environment

Unity app that shows the current weather for a location using the OpenWeather API. A small 3D village changes with the weather (clear, cloudy, rain, snow) and with day or night at that location.

Unity 6000.6.7f1 (URP), built for WebGL.

## API key setup

The key is not committed. To run the project:

1. Get a free key at [openweathermap.org](https://openweathermap.org/) (My API keys).
2. Create `Assets/Resources/WeatherApiKey.txt`.
3. Paste only the key into it and save.

The file is in `.gitignore`. Without it, test mode still works.

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

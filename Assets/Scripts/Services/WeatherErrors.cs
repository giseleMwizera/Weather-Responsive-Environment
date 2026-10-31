using UnityEngine;

// Turns HTTP status codes into messages a user can act on.
public static class WeatherErrors
{
    public static string Describe(long status, string body)
    {
        switch (status)
        {
            case 0:
                return "Could not reach the weather service. Check your internet connection and try again.";
            case 401:
                return "The API key was rejected (401). Check the key, new OpenWeather keys can take up to 2 hours to activate.";
            case 404:
                return "No weather data was found for this location (404).";
            case 429:
                return "Too many requests (429). Wait a minute, then try again.";
        }

        if (status >= 500)
            return $"The weather service is having problems ({status}). Try again later.";

        string apiMessage = ReadApiMessage(body);
        return string.IsNullOrEmpty(apiMessage)
            ? $"The request failed ({status})."
            : $"The request failed ({status}): {apiMessage}";
    }

    static string ReadApiMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;
        try
        {
            ApiError error = JsonUtility.FromJson<ApiError>(body);
            return error != null ? error.message : null;
        }
        catch (System.ArgumentException)
        {
            return null;
        }
    }
}

using System.Globalization;

// Checks the custom latitude/longitude fields before any request is sent.
public static class CoordinateValidator
{
    public static bool TryValidate(string latText, string lonText, out float latitude, out float longitude, out string error)
    {
        latitude = 0f;
        longitude = 0f;

        bool latEmpty = string.IsNullOrWhiteSpace(latText);
        bool lonEmpty = string.IsNullOrWhiteSpace(lonText);
        if (latEmpty && lonEmpty)
        {
            error = "Enter a latitude and a longitude.";
            return false;
        }
        if (latEmpty)
        {
            error = "Latitude is empty.";
            return false;
        }
        if (lonEmpty)
        {
            error = "Longitude is empty.";
            return false;
        }

        if (!TryParseNumber(latText, out latitude))
        {
            error = $"Latitude \"{latText.Trim()}\" is not a number. Use a dot for decimals, e.g. -1.94";
            return false;
        }
        if (!TryParseNumber(lonText, out longitude))
        {
            error = $"Longitude \"{lonText.Trim()}\" is not a number. Use a dot for decimals, e.g. 30.06";
            return false;
        }

        if (latitude < -90f || latitude > 90f)
        {
            error = "Latitude must be between -90 and 90.";
            return false;
        }
        if (longitude < -180f || longitude > 180f)
        {
            error = "Longitude must be between -180 and 180.";
            return false;
        }

        error = null;
        return true;
    }

    // InvariantCulture so "1.5" parses the same on every machine and browser.
    static bool TryParseNumber(string text, out float value)
    {
        return float.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value)
               && !float.IsNaN(value)
               && !float.IsInfinity(value);
    }
}

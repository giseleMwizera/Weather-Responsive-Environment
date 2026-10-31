using TMPro;
using UnityEngine;

// Animated "Retrieving weather..." text plus a spinning icon while a request runs.
public class LoadingIndicator : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] RectTransform spinner;
    [SerializeField] string message = "Retrieving weather";
    [SerializeField] float spinSpeed = 270f;

    void Update()
    {
        int dots = (int)(Time.unscaledTime * 3f) % 4;
        label.text = message + new string('.', dots);
        if (spinner != null)
            spinner.Rotate(0f, 0f, -spinSpeed * Time.unscaledDeltaTime);
    }
}

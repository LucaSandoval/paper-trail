using UnityEngine;
using System.Collections;

public class CameraIntroPan : MonoBehaviour
{
    public float panDuration = 3f;       // Duration of the pan effect
    public Transform startPoint;        // The starting position for the camera pan
    public Canvas gameElements;         // Canvas containing the game UI elements
    public float fadeDuration = 2f;     // Duration for the fade-in effect

    private Vector3 originalPosition;   // The camera's default position
    private Quaternion originalRotation; // The camera's default rotation

    void Start()
    {
        CanvasGroup canvasGroup = gameElements.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameElements.gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;

        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Start the pan effect
        if (startPoint != null)
        {
            transform.position = startPoint.position;
            StartCoroutine(PanToDefault());
        }
    }

    private IEnumerator PanToDefault()
    {
        float elapsedTime = 0f;

        Vector3 startPosition = transform.position;

        while (elapsedTime < panDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / panDuration;

            transform.position = Vector3.Lerp(startPosition, originalPosition, t);

            yield return null;
        }

        // Snap to final position to ensure precision
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // Start fading in the game UI elements
        StartCoroutine(FadeInElements());
    }

    private IEnumerator FadeInElements()
    {
        CanvasGroup canvasGroup = gameElements.GetComponent<CanvasGroup>();
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Enable the camera zoom functionality
        CameraZoomOnHover cam = FindObjectOfType<CameraZoomOnHover>();
        if (cam != null)
        {
            cam.enabled = true;
            cam.RegisterOriginalCameraSettings(originalPosition, originalRotation);
            FindObjectOfType<OptimizedWrittingScripts>().enabled = true;
        }
    }
}

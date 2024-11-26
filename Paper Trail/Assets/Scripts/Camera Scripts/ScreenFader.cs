using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public Image fadeImage;          
    public float fadeDuration = 1f;
    private NumberRollupText nbr;
    public Text score;
    public 
    

    void Start()
    {
        nbr = FindObjectOfType<NumberRollupText>();
        StartCoroutine(FadeFromBlack());

    }

    public void Fade()
    {
        StartCoroutine(FadeToBlack());
    }

    public IEnumerator FadeFromBlack()
    {
        float elapsedTime = 0f;
        Color startColor = fadeImage.color;
        startColor.a = 1f; 
        fadeImage.color = startColor;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration); 
            fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        //fadeImage.gameObject.SetActive(false);
    }

    public IEnumerator FadeToBlack()
    {
        float elapsedTime = 0f;
        Color startColor = fadeImage.color;
        startColor.a = 1f;
        fadeImage.color = startColor;
        //StartCoroutine(FadeInElements());
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, 1f);

        startColor = Color.white;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            score.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        
        score.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
        nbr.AnimateNumber(FindObjectOfType<OptimizedWrittingScripts>().storedScore);
        HardCodedBS();
    }

    //private IEnumerator FadeInElements()
    //{
    //    gameElements.enabled = true;
    //    CanvasGroup canvasGroup = gameElements.GetComponent<CanvasGroup>();
    //    float elapsedTime = 0f;
    //    Button retry = gameElements.GetComponentInChildren<Button>();

    //    while (elapsedTime < fadeDuration)
    //    {
    //        elapsedTime += Time.deltaTime;
    //        canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
    //        retry.GetComponent<CanvasGroup>().alpha = 1;
    //        yield return null;
    //    }
    //    canvasGroup.alpha = 0f;
    //    retry.GetComponent<CanvasGroup>().alpha = 1;

    //    // Enable the camera zoom functionality
    //    CameraZoomOnHover cam = FindObjectOfType<CameraZoomOnHover>();
    //    if (cam != null)
    //    {
    //        cam.enabled = false;
    //        FindObjectOfType<OptimizedWrittingScripts>().enabled = false;
    //    }
    //}

    private void HardCodedBS()
    {
        GameObject.Find("complete button").SetActive(false);
        GameObject.Find("mockui1").SetActive(false);
        GameObject.Find("mockui1 (2)").SetActive(false);
        GameObject.Find("mockui1 (1)").SetActive(false);

        // Disable the camera zoom functionality
        CameraZoomOnHover cam = FindObjectOfType<CameraZoomOnHover>();
        if (cam != null)
        {
            cam.enabled = false;
            FindObjectOfType<OptimizedWrittingScripts>().enabled = false;
        }
    }
}

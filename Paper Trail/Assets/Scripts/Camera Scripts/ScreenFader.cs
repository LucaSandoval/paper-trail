using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public Image fadeImage;          
    public float fadeDuration = 1f;

    void Start()
    {
        StartCoroutine(FadeFromBlack());
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
        fadeImage.gameObject.SetActive(false);
    }
}

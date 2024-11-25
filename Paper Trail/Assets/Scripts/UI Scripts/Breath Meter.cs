using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BreathMeter : MonoBehaviour
{
    private OptimizedWrittingScripts breathScript;
    private Slider meter;
    private CanvasGroup canvasGroup;
    private float fadeSpeed = 2f; // Speed at which the meter fades
    private float fadeDelay = 1f; // How long to wait before starting to fade when full
    private float fadeTimer;

    void Start()
    {
        breathScript = GameObject.FindObjectOfType<OptimizedWrittingScripts>();
        meter = GetComponent<Slider>();
        canvasGroup = GetComponent<CanvasGroup>();

        // Add CanvasGroup if it doesn't exist
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        meter.maxValue = breathScript.maxBreathTime;
        meter.value = breathScript.currentBreathTime;
    }

    void Update()
    {
        meter.value = breathScript.currentBreathTime;

        if (meter.value >= meter.maxValue) // If meter full, then fade bar 
        {
            fadeTimer += Time.deltaTime;

            // Start fading after delay
            if (fadeTimer >= fadeDelay)
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, fadeSpeed * Time.deltaTime);
            }
        }
        else // If meter being used, then fade in bar 
        {
            fadeTimer = 0f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 1f, fadeSpeed * Time.deltaTime);
        }
    }
}
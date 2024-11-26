using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class NumberRollupText : MonoBehaviour
{
    public Text targetText;

    public TextMeshProUGUI tmpText;

    public float duration = 1f;

    public AnimationCurve easeCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public int decimalPlaces = 0;

   
    public void AnimateNumber(float targetValue)
    {

        StartCoroutine(RollupCoroutine(targetValue));
    }

    
    private IEnumerator RollupCoroutine(float targetValue)
    {
        float startValue = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Calculate progress and apply easing
            float progress = elapsedTime / duration;
            float easedProgress = easeCurve.Evaluate(progress);

            // Interpolate the current value
            float currentValue = Mathf.Lerp(startValue, targetValue, easedProgress);

            // Update the text with formatted number
            UpdateTextValue(currentValue);

            // Increment time
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure we end exactly on the target value
        UpdateTextValue(targetValue);
    }

   
    private void UpdateTextValue(float value)
    {
        // Format the number based on decimal places
        string formattedValue = "Score: " + value.ToString($"F{decimalPlaces}") + "%";

        // Update Text or TextMeshProUGUI
        if (targetText != null)
            targetText.text = formattedValue;

        if (tmpText != null)
            tmpText.text = formattedValue;
    }

    
    public void AnimateNumber(int targetValue)
    {
        AnimateNumber((float)targetValue);
    }

}
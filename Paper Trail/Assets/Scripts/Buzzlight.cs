using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buzzlight : MonoBehaviour
{
    public Light spotlight; 
    public AudioSource buzzAudio; 

    // Flash happens every 10 seconds, and for each flash the light flicker 3-4 times
    public float flashInterval = 0.1f; 
    public int minCount = 3; 
    public int maxCount = 4; 
    public float flashCooldown = 10f; 
    private bool isFlashing = false;
    private bool isEnabled = true;

    void Start()
    {
        InvokeRepeating(nameof(StartFlash), 0f, flashCooldown);
    }

    public void StartFlash()
    {
        if (!isFlashing && isEnabled)
        {
            StartCoroutine(flashRoutine());
        }
    }

    private IEnumerator flashRoutine()
    {
        isFlashing = true;
        //buzz either 3 or 4 times
        int flashCount = Random.Range(minCount, maxCount + 1);
        for (int i = 0; i < flashCount; i++)
        {
            if (!isEnabled) break;

            if (i % 2 == 0)
            {
                buzzAudio.Play();
            }
            spotlight.intensity = Random.Range(0.5f, 2.2f);
            yield return new WaitForSeconds(flashInterval);
        }


        // Return to default
        spotlight.intensity = 2.2f;
        isFlashing = false;
    }

    //for complete button
    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;

        if (!isEnabled)
        {
            StopAllCoroutines();
            spotlight.intensity = 2.2f; 
            buzzAudio.Stop();
        }
    }

}

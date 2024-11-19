using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrittingScripts : MonoBehaviour
{
    public Renderer quadRenderer;
    public Color brushColor = Color.black;
    public int brushSize = 5;
    public float splotchFactor = 1.2f;
    public float minSpeedScale = 0.8f;
    public float maxSpeed = 1000f;
    public float smoothingFactor = 0.3f;
    public Texture2D referenceTexture;

    private Texture2D drawingTexture;
    private Vector2? previousDrawPosition = null;
    private bool isDrawing = false;
    private float lastDrawTime;
    private Vector2 lastVelocity = Vector2.zero;

    //AudioChangeOnWriting
    public AudioSource WritingSFX; 
    private float minPitch = 0.9f;  
    private float maxPitch = 1.1f;  
    private float pitchSpeedFactor = 0.01f; 


    void Start()
    {
        // Get the existing texture from the material
        Texture existingTexture = quadRenderer.material.mainTexture;

        // Create a new texture with the same dimensions
        drawingTexture = new Texture2D(existingTexture.width, existingTexture.height, TextureFormat.RGBA32, false);
        drawingTexture.filterMode = FilterMode.Point;

        // Create a temporary RenderTexture to copy the existing texture
        RenderTexture tempRT = RenderTexture.GetTemporary(
            existingTexture.width,
            existingTexture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );

        // Copy the existing texture to the temporary RenderTexture
        Graphics.Blit(existingTexture, tempRT);

        // Store the active RenderTexture
        RenderTexture previousRT = RenderTexture.active;

        // Set the temporary RenderTexture as active
        RenderTexture.active = tempRT;

        // Copy the pixels from the RenderTexture to our new texture
        drawingTexture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        drawingTexture.Apply();

        RenderTexture.active = previousRT;
        RenderTexture.ReleaseTemporary(tempRT);
        quadRenderer.material.mainTexture = drawingTexture;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            lastDrawTime = Time.time;
        }

        if (Input.GetMouseButton(0) && isDrawing)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == quadRenderer.gameObject)
            {
                Vector2 pixelUV = hit.textureCoord;
                Vector2 currentPos = new Vector2(
                    (int)(pixelUV.x * drawingTexture.width),
                    (int)(pixelUV.y * drawingTexture.height)
                );

                if (previousDrawPosition.HasValue)
                {
                    float deltaTime = Time.time - lastDrawTime;
                    Vector2 velocity = (currentPos - previousDrawPosition.Value) / deltaTime;
                    lastVelocity = Vector2.Lerp(lastVelocity, velocity, smoothingFactor);

                    DrawImprovedLine(previousDrawPosition.Value, currentPos, lastVelocity.magnitude);

                    //SOUND EFFECTS, play speed change on player's writing speed change
                    float speed = lastVelocity.magnitude;
                    float normalizedSpeed = Mathf.Clamp(speed / maxSpeed, 0, 1);
                    WritingSFX.pitch = Mathf.Lerp(minPitch, maxPitch, normalizedSpeed);

                    if (!WritingSFX.isPlaying)
                    {
                        WritingSFX.Play();
                    }

                }
                else
                {
                    DrawSplotch((int)currentPos.x, (int)currentPos.y);
                }

                previousDrawPosition = currentPos;
                lastDrawTime = Time.time;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (previousDrawPosition.HasValue)
            {
                DrawSplotch((int)previousDrawPosition.Value.x, (int)previousDrawPosition.Value.y);
            }
            isDrawing = false;
            previousDrawPosition = null;
            lastVelocity = Vector2.zero;
            WritingSFX.Stop();
        }
    }

    void DrawImprovedLine(Vector2 start, Vector2 end, float speed)
    {
        float distance = Vector2.Distance(start, end);
        if (distance < 0.1f) return;

        int steps = Mathf.Max(2, Mathf.CeilToInt(distance));
        Vector2 direction = (end - start).normalized;

        float speedScale = Mathf.Lerp(1f, minSpeedScale, Mathf.Clamp01(speed / maxSpeed));

        for (int i = 0; i < steps; i++)
        {
            float t = i / (float)(steps - 1);
            Vector2 pos = Vector2.Lerp(start, end, t);

            float taperFactor = 1f;
            if (t < 0.2f)
            {
                taperFactor = Mathf.SmoothStep(0.8f, 1f, t * 5f);
            }
            else if (t > 0.8f)
            {
                taperFactor = Mathf.SmoothStep(0.8f, 1f, (1f - t) * 5f);
            }

            int currentBrushSize = Mathf.Max(1, (int)(brushSize * speedScale * taperFactor));
            DrawAtPosition((int)pos.x, (int)pos.y, currentBrushSize);
        }

        drawingTexture.Apply();
    }

    void DrawAtPosition(int x, int y, int customBrushSize)
    {
        int brushSizeSquared = customBrushSize * customBrushSize;

        for (int i = -customBrushSize; i <= customBrushSize; i++)
        {
            for (int j = -customBrushSize; j <= customBrushSize; j++)
            {
                if (i * i + j * j <= brushSizeSquared)
                {
                    int newX = x + i;
                    int newY = y + j;

                    if (newX >= 0 && newX < drawingTexture.width &&
                        newY >= 0 && newY < drawingTexture.height)
                    {
                        float distanceFromCenter = (i * i + j * j) / (float)brushSizeSquared;
                        Color pixelColor = brushColor;
                        pixelColor.a = Mathf.Lerp(1f, 0.7f, distanceFromCenter);

                        Color existingColor = drawingTexture.GetPixel(newX, newY);
                        drawingTexture.SetPixel(newX, newY, Color.Lerp(existingColor, pixelColor, pixelColor.a));
                    }
                }
            }
        }
    }

    void DrawSplotch(int x, int y)
    {
        int splotchSize = (int)(brushSize * splotchFactor);
        DrawAtPosition(x, y, splotchSize);
        drawingTexture.Apply();
    }

    public void ClearTexture()
    {
        // Get the existing texture from the material
        Texture originalTexture = quadRenderer.material.GetTexture("_MainTex");

        // Create a temporary RenderTexture
        RenderTexture tempRT = RenderTexture.GetTemporary(
            originalTexture.width,
            originalTexture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );

        // Copy the original texture to the temporary RenderTexture
        Graphics.Blit(originalTexture, tempRT);
        RenderTexture previousRT = RenderTexture.active;
        RenderTexture.active = tempRT;
        drawingTexture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        drawingTexture.Apply();
        RenderTexture.active = previousRT;
        RenderTexture.ReleaseTemporary(tempRT);
    }

    public float CompareTextures(Texture2D texture1, Texture2D texture2)
    {
        int matchedBlackPixels = 0;
        int totalBlackPixelsReference = 0;
        float blackPixelThreshold = 0.1f; 

        for (int x = 0; x < texture1.width; x++)
        {
            for (int y = 0; y < texture1.height; y++)
            {
                Color color1 = texture1.GetPixel(x, y);
                Color color2 = texture2.GetPixel(x, y);

                bool isBlack1 = color1.r < blackPixelThreshold && color1.g < blackPixelThreshold && color1.b < blackPixelThreshold;
                bool isBlack2 = color2.r < blackPixelThreshold && color2.g < blackPixelThreshold && color2.b < blackPixelThreshold;

                // Count total black pixels in reference texture
                if (isBlack1)
                    totalBlackPixelsReference++;

                // Increment if pixels from both images are black
                if (isBlack1 && isBlack2)
                    matchedBlackPixels++;
            }
        }
        
        return (float)matchedBlackPixels / totalBlackPixelsReference;
    }


    public void CompareSignatures()
    {
        SaveTextureToFile(drawingTexture, "NewSignature.png");
        float similarityScore = CompareTextures(drawingTexture, referenceTexture);
        Debug.Log("Similarity: " + (similarityScore * 100f) + "%");
    }

    public void SaveTextureToFile(Texture2D texture, string fileName)
    {
        // Encode texture into PNG format
        byte[] bytes = texture.EncodeToPNG();

        // Define the path to the Downloads folder
        string downloadsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "\\Downloads";
        string filePath = System.IO.Path.Combine(downloadsPath, fileName);

        // Write the byte array to the file
        System.IO.File.WriteAllBytes(filePath, bytes);

        Debug.Log("Texture saved to: " + filePath);
    }
}




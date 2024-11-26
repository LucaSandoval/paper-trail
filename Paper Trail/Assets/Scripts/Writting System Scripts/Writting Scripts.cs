using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptimizedWrittingScripts : MonoBehaviour
{
    [Header("Drawing Components")]
    public Renderer quadRenderer;
    public Color brushColor = Color.black;
    public int brushSize = 5;
    public float splotchFactor = 1.2f;
    public float minSpeedScale = 0.8f;
    public float maxSpeed = 1000f;
    public float smoothingFactor = 0.3f;
    public Texture2D referenceTexture;

    [Header("Audio Settings")]
    public AudioSource WritingSFX;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    [Header("Shake Mechanics")]
    public float maxBreathTime = 5f;
    public float breathRechargeRate = 0.5f;
    public float ShakeAmount = 16f;
    public float shakeSpeed = 10f;

    // Cached components and optimization variables
    private Texture2D drawingTexture;
    private Camera mainCamera;
    private Color[] brushPixels;
    private Color[] texturePixels;
    private int textureWidth;
    private int textureHeight;

    // Optimization variables
    private Vector2? previousDrawPosition = null;
    private bool isDrawing = false;
    private float lastDrawTime;
    private Vector2 lastVelocity = Vector2.zero;

    // Breath and shake variables
    public float currentBreathTime;
    public bool isHoldingBreath;
    private float currentShakeAmount;

    public float storedScore = 0;

    void Awake()
    {
        // Cache main camera for performance
        mainCamera = Camera.main;

        // Pre-calculate brush pixels to reduce allocation
        PrepareBrushPixels();
    }

    void Start()
    {
        InitializeDrawingTexture();

        currentBreathTime = maxBreathTime;
        currentShakeAmount = ShakeAmount;
    }

    void PrepareBrushPixels()
    {
        // Pre-allocate and calculate brush pixels
        int maxBrushSize = Mathf.CeilToInt(brushSize * splotchFactor);
        int arraySize = (maxBrushSize * 2 + 1) * (maxBrushSize * 2 + 1);
        brushPixels = new Color[arraySize];
    }

    void InitializeDrawingTexture()
    {
        Texture existingTexture = quadRenderer.material.mainTexture;
        textureWidth = existingTexture.width;
        textureHeight = existingTexture.height;

        drawingTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        drawingTexture.filterMode = FilterMode.Point;

        RenderTexture tempRT = RenderTexture.GetTemporary(
            textureWidth,
            textureHeight,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );

        Graphics.Blit(existingTexture, tempRT);
        RenderTexture previousRT = RenderTexture.active;
        RenderTexture.active = tempRT;

        drawingTexture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        drawingTexture.Apply();

        // Store pixel array for faster access
        texturePixels = drawingTexture.GetPixels();

        RenderTexture.active = previousRT;
        RenderTexture.ReleaseTemporary(tempRT);

        quadRenderer.material.mainTexture = drawingTexture;
    }

    void Update()
    {
        UpdateBreathMechanics();
        HandleDrawing();
    }

    void HandleDrawing()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            lastDrawTime = Time.time;
        }

        if (Input.GetMouseButton(0) && isDrawing)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == quadRenderer.gameObject)
            {
                Vector2 pixelUV = hit.textureCoord;
                Vector2 currentPos = new Vector2(
                    (int)(pixelUV.x * textureWidth),
                    (int)(pixelUV.y * textureHeight)
                );

                // Apply shake only when not holding breath
                if (!isHoldingBreath)
                {
                    currentPos += CalculateShake();
                    currentPos.x = Mathf.Clamp(currentPos.x, 0, textureWidth - 1);
                    currentPos.y = Mathf.Clamp(currentPos.y, 0, textureHeight - 1);
                }

                if (previousDrawPosition.HasValue)
                {
                    float deltaTime = Time.time - lastDrawTime;
                    Vector2 velocity = (currentPos - previousDrawPosition.Value) / deltaTime;
                    lastVelocity = Vector2.Lerp(lastVelocity, velocity, smoothingFactor);
                    DrawImprovedLine(previousDrawPosition.Value, currentPos, lastVelocity.magnitude);

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

    private void UpdateBreathMechanics()
    {
        if (Input.GetMouseButtonDown(1) && currentBreathTime > 0)
        {
            isHoldingBreath = true;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isHoldingBreath = false;
        }

        currentBreathTime += isHoldingBreath
            ? -Time.deltaTime
            : Time.deltaTime * breathRechargeRate;

        currentBreathTime = Mathf.Clamp(currentBreathTime, 0, maxBreathTime);
        currentShakeAmount = isHoldingBreath ? 0 : ShakeAmount;
    }

    private Vector2 CalculateShake()
    {
        float time = Time.time * shakeSpeed;
        return new Vector2(
            Mathf.PerlinNoise(time, 0) * currentShakeAmount - (currentShakeAmount * 0.5f),
            Mathf.PerlinNoise(0, time) * currentShakeAmount - (currentShakeAmount * 0.5f)
        );
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

        // Batch apply pixels for better performance
        drawingTexture.SetPixels(texturePixels);
        drawingTexture.Apply(false);
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

                    if (newX >= 0 && newX < textureWidth &&
                        newY >= 0 && newY < textureHeight)
                    {
                        int pixelIndex = newY * textureWidth + newX;
                        float distanceFromCenter = (i * i + j * j) / (float)brushSizeSquared;

                        Color pixelColor = brushColor;
                        pixelColor.a = Mathf.Lerp(1f, 0.7f, distanceFromCenter);

                        Color existingColor = texturePixels[pixelIndex];
                        texturePixels[pixelIndex] = Color.Lerp(existingColor, pixelColor, pixelColor.a);
                    }
                }
            }
        }
    }

    void DrawSplotch(int x, int y)
    {
        int splotchSize = (int)(brushSize * splotchFactor);
        DrawAtPosition(x, y, splotchSize);

        // Batch apply pixels
        drawingTexture.SetPixels(texturePixels);
        drawingTexture.Apply(false);
    }

    public void ClearTexture()
    {
        // Use the pre-cached texture initialization method
        InitializeDrawingTexture();
    }

    public float CompareTextures(Texture2D texture1, Texture2D texture2)
    {
        int matchedBlackPixels = 0;
        int totalBlackPixelsReference = 0;
        float blackPixelThreshold = 0.1f;

        // Use Color[] for faster pixel access
        Color[] pixels1 = texture1.GetPixels();
        Color[] pixels2 = texture2.GetPixels();

        for (int i = 0; i < pixels1.Length; i++)
        {
            bool isBlack1 = pixels1[i].r < blackPixelThreshold &&
                            pixels1[i].g < blackPixelThreshold &&
                            pixels1[i].b < blackPixelThreshold;

            bool isBlack2 = pixels2[i].r < blackPixelThreshold &&
                            pixels2[i].g < blackPixelThreshold &&
                            pixels2[i].b < blackPixelThreshold;

            // Count total black pixels in reference texture
            if (isBlack1)
                totalBlackPixelsReference++;

            // Increment if pixels from both images are black
            if (isBlack1 && isBlack2)
                matchedBlackPixels++;
        }

        return (float)matchedBlackPixels / totalBlackPixelsReference;
    }

    public void CompareSignatures()
    {
        //SaveTextureToFile(drawingTexture, "NewSignature.png");
        float similarityScore = CompareTextures(drawingTexture, referenceTexture);
        storedScore = similarityScore * 1500f;
        Debug.Log("Similarity: " + (similarityScore * 1500f) + "%");
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
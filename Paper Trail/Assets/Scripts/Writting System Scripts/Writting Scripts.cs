using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrittingScripts : MonoBehaviour
{
    public Renderer quadRenderer;
    public Color brushColor = Color.black;
    public int brushSize = 10;
    public float splotchFactor = 1.2f;
    public float minSpeedScale = 0.8f;  // Minimum brush scale at high speeds
    public float maxSpeed = 1000f;      // Speed at which brush reaches minimum scale
    public float smoothingFactor = 0.3f; // Lower values = more smoothing

    private Texture2D drawingTexture;
    private Vector2? previousDrawPosition = null;
    private bool isDrawing = false;
    private float lastDrawTime;
    private Vector2 lastVelocity = Vector2.zero;

    void Start()
    {
        drawingTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        drawingTexture.filterMode = FilterMode.Point;
        ClearTexture();
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
            if (t < 0.2f) // Start taper
            {
                taperFactor = Mathf.SmoothStep(0.8f, 1f, t * 5f);
            }
            else if (t > 0.8f) // End taper
            {
                taperFactor = Mathf.SmoothStep(0.8f, 1f, (1f - t) * 5f);
            }

            // Combine speed scaling with taper
            int currentBrushSize = Mathf.Max(1, (int)(brushSize * speedScale * taperFactor));
            DrawAtPosition((int)pos.x, (int)pos.y, currentBrushSize);
        }

        drawingTexture.Apply();
    }

    void DrawAtPosition(int x, int y, int customBrushSize)
    {
        // Use squared distance comparison for better performance
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

                        float distanceFromCenter = (i * i + j * j) / (float)brushSizeSquared; // Smooth edges of stroke
                        Color pixelColor = brushColor;
                        pixelColor.a = Mathf.Lerp(1f, 0.7f, distanceFromCenter);

                        Color existingColor = drawingTexture.GetPixel(newX, newY); //Blends with existing pixel colors on paper
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
        Color[] clearPixels = new Color[drawingTexture.width * drawingTexture.height];
        for (int i = 0; i < clearPixels.Length; i++)
        {
            clearPixels[i] = Color.white;
        }
        drawingTexture.SetPixels(clearPixels);
        drawingTexture.Apply();
    }
}
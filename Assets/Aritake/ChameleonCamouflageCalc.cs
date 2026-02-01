using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using CriWare;


public class ChameleonCamouflageCalc : MonoBehaviour
{
    public CriAtomSource footstepSource;
    public CriAtomSource brushSource;
    public CriAtomSource colorPickSource;

    [Header("Textures")]
    public Texture2D[] frameTextures;
    public MeshRenderer overlayMeshRenderer;
    public Texture2D[] overlayTextures;
    public float baseAnimationSpeed = 5.0f; // Base multiplier for movement-based animation
    public Texture2D brushCursor;
    public Texture2D brushTipCursor;

    [Header("Paint Settings")]
    public Color paintColor = Color.white;
    public int brushSize = 8;
    public int interpolateCount = 4;

    private Texture2D[] writableTextures;
    private MeshRenderer meshRenderer;
    private int currentFrame = 0;
    private int frameCount = 0;
    private float animTimer = 0f;
    private bool canPaint = true;
    private bool isAnimating = true; // Control whether the walk animation plays
    private Vector2 lastPaintPoint;
    private Vector3 lastPosition;

    private CriAtomSource atomSource;

    public bool AccuracyDirty { get; private set; } = true;

    public void ConsumeAccuracyDirty() => AccuracyDirty = false;

    void Start()
    {
        atomSource = GetComponent<CriAtomSource>();


        frameCount = frameTextures.Length;
        writableTextures = new Texture2D[frameCount];
        lastPosition = transform.position;

        // Set custom cursor
        // Vector2 hotSpot = new Vector2(brushCursor.width / 2f, brushCursor.height / 2f);
        // Cursor.SetCursor(brushCursor, hotSpot, CursorMode.ForceSoftware);
        

        meshRenderer = GetComponent<MeshRenderer>();

        // Initialize writable textures with original frame content
        for (int i = 0; i < frameCount; i++)
        {
            writableTextures[i] = new Texture2D(frameTextures[i].width, frameTextures[i].height, TextureFormat.RGBA32, false);
            writableTextures[i].SetPixels(frameTextures[i].GetPixels());
            writableTextures[i].Apply();
        }

        RefreshTextures();
    }

    public void SetCustomCursor(Color newColor)
    {
        int width = brushCursor.width;
        int height = brushCursor.height;

        // 1. Create a new texture to hold the combined result
        Texture2D combinedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // 2. Get the pixel arrays
        Color[] brushPixels = brushCursor.GetPixels();
        Color[] tipPixels = brushTipCursor.GetPixels();
        Color[] resultPixels = new Color[brushPixels.Length];

        for (int i = 0; i < brushPixels.Length; i++)
        {
            Color brushCol = brushPixels[i];
            
            Color tintedTip = tipPixels[i];
            // Tint the tip: Multiply the white tip pixel by our target color
            if (tipPixels[i].a > 0.0f)
            {
               tintedTip = newColor * 1.3f;
            }
            
        
            // 3. Simple Alpha Blending
            // If the brush pixel is transparent, show the tinted tip. 
            // If not, show the brush.
            resultPixels[i] = Color.Lerp(tintedTip, brushCol, brushCol.a);
        }

        // 4. Apply pixels to the new texture
        combinedTexture.SetPixels(resultPixels);
        combinedTexture.Apply();

        // 5. Set the cursor
        Vector2 hotSpot = new Vector2(width / 2f, height / 2f);
        Cursor.SetCursor(combinedTexture, hotSpot, CursorMode.ForceSoftware);
    }

    void Update()
    {
        // 1. Dynamic Animation Logic
        if (isAnimating)
        {
                    
            // Calculate actual movement speed based on position delta (detects Easing curve acceleration)
            float distanceMoved = (transform.position - lastPosition).magnitude;

            // Advance animation timer proportional to movement speed
            animTimer += distanceMoved * baseAnimationSpeed;

            if (animTimer >= 1.0f) // Threshold to switch frames
            {
                animTimer = 0f;
                currentFrame = (currentFrame + 1) % frameCount;
                RefreshTextures();
                if (currentFrame % 2 == 0)
                    footstepSource.Play();
            }
        }
        else
        {
            // Reset to the first frame (Standing still) when at a checkpoint
            if (currentFrame != 0)
            {
                currentFrame = 0;
                animTimer = 0f;
                RefreshTextures();
            }
        }
        lastPosition = transform.position;

        // 2. Paint Logic
        if (canPaint && Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            if (!HandlePaint())
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    HandleColorPick();
                }
            }
        }
    }

    // Call this from GameDirector (e.g., SetAnimating(false) when arriving at checkpoint)
    public void SetAnimating(bool enabled) => isAnimating = enabled;

    public void SetPaintingEnabled(bool enabled) => canPaint = enabled;

    private void RefreshTextures()
    {
        meshRenderer.material.mainTexture = writableTextures[currentFrame];
        if (overlayMeshRenderer) overlayMeshRenderer.material.mainTexture = overlayTextures[currentFrame];
    }

    #region Color Picking and Painting Logic

    bool HandleColorPick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f).OrderBy(h => h.distance).ToArray();

        foreach (RaycastHit hit in hits)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            MeshCollider meshCollider = hit.collider as MeshCollider;

            if (renderer != null && meshCollider != null)
            {
                Texture2D tex = renderer.sharedMaterial.mainTexture as Texture2D;
                if (tex == null) continue;

                Vector2 pixelUV = hit.textureCoord;
                Vector2 tiling = renderer.sharedMaterial.mainTextureScale;
                Vector2 offset = renderer.sharedMaterial.mainTextureOffset;
                pixelUV = new Vector2((pixelUV.x * tiling.x) + offset.x, (pixelUV.y * tiling.y) + offset.y);

                int x = Mathf.FloorToInt(pixelUV.x * tex.width);
                int y = Mathf.FloorToInt(pixelUV.y * tex.height);

                Color pixelColor = tex.GetPixel(x, y);
                if (pixelColor.a < 0.1f) continue;

                paintColor = pixelColor;
                SetCustomCursor(paintColor);
                colorPickSource.Play();
                return true;
            }
        }
        return false;
    }

    bool HandlePaint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                Vector2 uv = hit.textureCoord;
                int x = (int)(uv.x * writableTextures[0].width);
                int y = (int)(uv.y * writableTextures[0].height);

                if (brushSource.status != CriAtomSource.Status.Playing)
                {
                    brushSource.Play();
                }
                return PaintAt(x, y);
            }
        }
        return false;
    }

    bool PaintAt(int centerX, int centerY)
    {
        bool changed = PaintCircle(centerX, centerY);

        if (lastPaintPoint != null && !Mouse.current.leftButton.wasPressedThisFrame)
        {
            int startX = centerX;
            int startY = centerY;
            int dx = Mathf.FloorToInt((lastPaintPoint.x - startX) / (float)interpolateCount);
            int dy = Mathf.FloorToInt((lastPaintPoint.y - startY) / (float)interpolateCount);

            for (int i = 0; i < interpolateCount; i++)
            {
                changed |= PaintCircle(startX + dx * i, startY + dy * i);
            }
        }
        lastPaintPoint = new Vector2(centerX, centerY);

        if (changed)
        {
            foreach (var tex in writableTextures) tex.Apply();
            AccuracyDirty = true;
            return true;
        }
        return false;
    }

    bool PaintCircle(int x, int y)
    {
        int width = writableTextures[0].width;
        int height = writableTextures[0].height;
        bool changed = false;
        for (int i = -brushSize; i <= brushSize; i++)
        {
            for (int j = -brushSize; j <= brushSize; j++)
            {
                int px = x + i; int py = y + j;
                if (px < 0 || px >= width || py < 0 || py >= height) continue;
                if (Vector2.SqrMagnitude(new Vector2(i, j)) > brushSize * brushSize) continue;

                for (int f = 0; f < frameCount; f++)
                {
                    if (frameTextures[f].GetPixel(px, py).a > 0.1f)
                    {
                        writableTextures[f].SetPixel(px, py, paintColor);
                        changed = true;
                    }
                }
            }
        }
        return changed;
    }
    #endregion

    public float CalculateAccuracy(Texture2D target)
    {
        if (target == null) return 0;

        Color[] playerPixels = writableTextures[0].GetPixels();
        Color[] maskPixels = frameTextures[0].GetPixels();
        int chamWidth = writableTextures[0].width;
        int chamHeight = writableTextures[0].height;
        int targetWidth = target.width;
        int targetHeight = target.height;

        float totalChameleonPixels = 0;
        float totalDiff = 0;

        for (int y = 0; y < chamHeight; y++)
        {
            for (int x = 0; x < chamWidth; x++)
            {
                int index = y * chamWidth + x;
                if (maskPixels[index].a > 0.1f)
                {
                    float u = (float)x / chamWidth;
                    float v = (float)y / chamHeight;

                    int targetX = Mathf.Clamp((int)(u * targetWidth), 0, targetWidth - 1);
                    int targetY = Mathf.Clamp((int)(v * targetHeight), 0, targetHeight - 1);

                    Color targetPixel = target.GetPixel(targetX, targetY);
                    if (targetPixel.a < 0.1f) continue;

                    totalChameleonPixels++;
                    totalDiff += (playerPixels[index] == targetPixel) ? 0.0f : 1.0f;
                }
            }
        }

        return totalChameleonPixels == 0 ? 0 : (1.0f - (totalDiff / totalChameleonPixels)) * 100f;
    }
}
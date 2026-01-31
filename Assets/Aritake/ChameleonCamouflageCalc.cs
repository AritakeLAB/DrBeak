using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.VisualScripting; // Required for OrderBy


// Extracted only the calculation logic from ChameleonCamouflageGame
public class ChameleonCamouflageCalc : MonoBehaviour
{
    [Header("Textures")]
    public Texture2D[] frameTextures;
    public MeshRenderer overlayMeshRenderer;
    public Texture2D[] overlayTextures;
    public float animationSpeed = 0.5f;
    public Texture2D brushCursor;
    public Texture2D colorPickerCursor;

    [Header("Paint Settings")]
    public Color paintColor = Color.red;
    public int brushSize = 8;
    public int interpolateCount = 4;

    private Texture2D[] writableTextures;
    private MeshRenderer meshRenderer;
    private int currentFrame = 0;
    private int frameCount = 0;
    private float animTimer = 0f;
    private bool canPaint = true;
    private Vector2 lastPaintPoint;

    void Start()
    {
        frameCount = frameTextures.Length;
        writableTextures = new Texture2D[frameCount];

        Vector2 hotSpot = new Vector2(brushCursor.width / 2f, brushCursor.height / 2f);
        Cursor.SetCursor(brushCursor, hotSpot, CursorMode.ForceSoftware);

        meshRenderer = GetComponent<MeshRenderer>();

        for (int i = 0; i < frameCount; i++)
        {
            writableTextures[i] = new Texture2D(frameTextures[i].width, frameTextures[i].height, TextureFormat.RGBA32, false);
            writableTextures[i].SetPixels(frameTextures[i].GetPixels());
            writableTextures[i].Apply();
        }

        meshRenderer.material.mainTexture = writableTextures[0];
        if (overlayMeshRenderer) overlayMeshRenderer.material.mainTexture = overlayTextures[0];
    }

    void Update()
    {
        // 1. Animation Logic
        animTimer += Time.deltaTime;
        if (animTimer >= animationSpeed)
        {
            animTimer = 0f;
            currentFrame = (currentFrame + 1) % frameCount;
            meshRenderer.material.mainTexture = writableTextures[currentFrame];
            if (overlayMeshRenderer) overlayMeshRenderer.material.mainTexture = overlayTextures[currentFrame];
        }

        // 2. Paint Logic
        if (canPaint && Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            if (!HandlePaint())
            {
                if(Mouse.current.leftButton.wasPressedThisFrame)
                {
                    HandleColorPick();
                }
            }
        }

        // 3. Color Selection (Shortcut keys)
        // if (Keyboard.current.digit1Key.wasPressedThisFrame) paintColor = Color.yellow;
        // if (Keyboard.current.digit2Key.wasPressedThisFrame) paintColor = Color.red;
        // if (Keyboard.current.digit3Key.wasPressedThisFrame) paintColor = Color.blue;
        // if (Keyboard.current.digit4Key.wasPressedThisFrame) paintColor = Color.green;
        // if (Keyboard.current.digit5Key.wasPressedThisFrame) paintColor = Color.purple;
        // if (Keyboard.current.digit6Key.wasPressedThisFrame) paintColor = Color.orange;
    }

    public void SetPaintingEnabled(bool enabled) => canPaint = enabled;

    bool HandleColorPick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        // Sort hits by distance to ensure we hit the closest object first
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f)
                                .OrderBy(h => h.distance)
                                .ToArray();

        foreach (RaycastHit hit in hits)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            MeshCollider meshCollider = hit.collider as MeshCollider;

            if (renderer != null && meshCollider != null)
            {
                Texture2D tex = renderer.sharedMaterial.mainTexture as Texture2D;
                if (tex == null) continue;

                // 1. Get raw UV
                Vector2 pixelUV = hit.textureCoord;

                // 2. Account for Tiling and Offset
                Vector2 tiling = renderer.sharedMaterial.mainTextureScale;
                Vector2 offset = renderer.sharedMaterial.mainTextureOffset;
                pixelUV = new Vector2((pixelUV.x * tiling.x) + offset.x, (pixelUV.y * tiling.y) + offset.y);

                // 3. Convert to Pixel Coordinates
                int x = Mathf.FloorToInt(pixelUV.x * tex.width);
                int y = Mathf.FloorToInt(pixelUV.y * tex.height);

                // 4. Get Color
                Color pixelColor = tex.GetPixel(x, y);

                // Check alpha (transparency)
                if (pixelColor.a < 0.1f) continue; 

                paintColor = pixelColor;
                return true;
            }
        }
        return false;
    }

    bool HandlePaint()
    {
        // Use Raycast even in 2D orthographic mode
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                Vector2 uv = hit.textureCoord;
                int x = (int)(uv.x * writableTextures[0].width);
                int y = (int)(uv.y * writableTextures[0].height);
                return PaintAt(x, y);
            }
            return false;
        }
        return false;
    }

    bool PaintAt(int centerX, int centerY)
    {
        bool changed = PaintCircle(centerX, centerY);

        if(lastPaintPoint != null && !Mouse.current.leftButton.wasPressedThisFrame)
        {
            int startX = centerX;
            int startY = centerY;
            int dx = Mathf.FloorToInt((lastPaintPoint.x - startX)/interpolateCount);
            int dy = Mathf.FloorToInt((lastPaintPoint.y - startY)/interpolateCount);

            for(int i=0; i<interpolateCount; i++)
            {
                changed |= PaintCircle(startX + dx, startY + dy);

                startX += dx;
                startY += dy;
            }
            
        }
        lastPaintPoint = new Vector2(centerX, centerY);
        
        if (changed) {
            for (int i=0; i < frameCount; i++)
            {
                writableTextures[i].Apply();
            }
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
                int px = x + i;
                int py = y + j;

                if (px < 0 || px >= width || py < 0 || py >= height) continue;

                // Circular brush check
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

    public float CalculateAccuracy(Texture2D target)
    {
        if (target == null) return 0;

        // Get the chameleon-side pixel data and dimensions
        Color[] playerPixels = writableTextures[0].GetPixels();
        Color[] maskPixels = frameTextures[0].GetPixels();
        int chamWidth = writableTextures[0].width;
        int chamHeight = writableTextures[0].height;

        // Get the dimensions of the target image
        int targetWidth = target.width;
        int targetHeight = target.height;

        float totalChameleonPixels = 0;
        float totalDiff = 0;

        // Iterate using a 2D loop (to avoid index misalignment)
        for (int y = 0; y < chamHeight; y++)
        {
            for (int x = 0; x < chamWidth; x++)
            {
                int index = y * chamWidth + x;

                // 1. Check whether this pixel belongs to the chameleon body (mask check)
                if (maskPixels[index].a > 0.1f)
                {
                    // 2. Compute UV coordinates (0.0–1.0)
                    float u = (float)x / chamWidth;
                    float v = (float)y / chamHeight;

                    // 3. Convert to pixel coordinates on the target image
                    int targetX = Mathf.Clamp((int)(u * targetWidth), 0, targetWidth - 1);
                    int targetY = Mathf.Clamp((int)(v * targetHeight), 0, targetHeight - 1);

                    // 4. Get the target pixel color
                    Color targetPixel = target.GetPixel(targetX, targetY);

                    // Exclude transparent areas of the target from scoring
                    if (targetPixel.a < 0.1f) continue;

                    totalChameleonPixels++;

                    // 5. Color comparison
                    // Strict exact-match comparison (current requirement)
                    float diff = (playerPixels[index] == targetPixel) ? 0.0f : 1.0f;

                    // If you want to evaluate based on color similarity instead:
                    /*
                    float rDiff = Mathf.Abs(playerPixels[index].r - targetPixel.r);
                    float gDiff = Mathf.Abs(playerPixels[index].g - targetPixel.g);
                    float bDiff = Mathf.Abs(playerPixels[index].b - targetPixel.b);
                    float diff = (rDiff + gDiff + bDiff) / 3f;
                    */

                    totalDiff += diff;
                }
            }
        }

        if (totalChameleonPixels == 0) return 0;

        // Calculate match accuracy (%)
        float averageError = totalDiff / totalChameleonPixels;
        return (1.0f - averageError) * 100f;
    }

}

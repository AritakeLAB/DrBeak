using UnityEngine;
using UnityEngine.InputSystem;

// Extracted only the calculation logic from ChameleonCamouflageGame
public class ChameleonCamouflageCalc : MonoBehaviour
{
    [Header("Textures")]
    public Texture2D[] frameTextures = new Texture2D[2];
    public MeshRenderer overlayMeshRenderer;
    public Texture2D[] overlayTextures = new Texture2D[2];
    public float animationSpeed = 0.5f;

    [Header("Paint Settings")]
    public Color paintColor = Color.red;
    public int brushSize = 8;

    private Texture2D[] writableTextures = new Texture2D[2];
    private MeshRenderer meshRenderer;
    private int currentFrame = 0;
    private float animTimer = 0f;
    private bool canPaint = true;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        for (int i = 0; i < 2; i++)
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
            currentFrame = (currentFrame + 1) % 2;
            meshRenderer.material.mainTexture = writableTextures[currentFrame];
            if (overlayMeshRenderer) overlayMeshRenderer.material.mainTexture = overlayTextures[currentFrame];
        }

        // 2. Paint Logic
        if (canPaint && Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            HandlePaint();
        }
    }

    public void SetPaintingEnabled(bool enabled) => canPaint = enabled;

    void HandlePaint()
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
                PaintAt(x, y);
            }
        }
    }

    void PaintAt(int centerX, int centerY)
    {
        int width = writableTextures[0].width;
        int height = writableTextures[0].height;
        bool changed = false;

        for (int i = -brushSize; i <= brushSize; i++)
        {
            for (int j = -brushSize; j <= brushSize; j++)
            {
                int px = centerX + i;
                int py = centerY + j;

                if (px < 0 || px >= width || py < 0 || py >= height) continue;

                // Circular brush check
                if (Vector2.SqrMagnitude(new Vector2(i, j)) > brushSize * brushSize) continue;

                for (int f = 0; f < 2; f++)
                {
                    if (frameTextures[f].GetPixel(px, py).a > 0.1f)
                    {
                        writableTextures[f].SetPixel(px, py, paintColor);
                        changed = true;
                    }
                }
            }
        }
        if (changed) { writableTextures[0].Apply(); writableTextures[1].Apply(); }
    }

    public float CalculateAccuracy(Texture2D target)
    {
        if (target == null) return 0;

        Color[] playerPixels = writableTextures[0].GetPixels();
        Color[] targetPixels = target.GetPixels();
        Color[] maskPixels = frameTextures[0].GetPixels();

        float totalChameleonPixels = 0;
        float totalDiff = 0;

        for (int i = 0; i < playerPixels.Length; i++)
        {
            if (maskPixels[i].a > 0.1f)
            {
                totalChameleonPixels++;
                float rDiff = Mathf.Abs(playerPixels[i].r - targetPixels[i].r);
                float gDiff = Mathf.Abs(playerPixels[i].g - targetPixels[i].g);
                float bDiff = Mathf.Abs(playerPixels[i].b - targetPixels[i].b);
                totalDiff += (rDiff + gDiff + bDiff) / 3f;
            }
        }

        return totalChameleonPixels == 0 ? 0 : (1.0f - (totalDiff / totalChameleonPixels)) * 100f;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using CriWare;

public class ChameleonCamouflageCalc : MonoBehaviour
{
    [Header("Audio")]
    public CriAtomSource footstepSource;
    public CriAtomSource brushSource;
    public CriAtomSource colorPickSource;

    [Header("Textures")]
    public Texture2D[] frameTextures;
    public MeshRenderer overlayMeshRenderer;
    public Texture2D[] overlayTextures;
    public float baseAnimationSpeed = 5.0f;

    [Header("Cursor")]
    public Texture2D brushCursor;
    public Texture2D brushTipCursor;

    [Header("Paint Settings")]
    public Color paintColor = Color.white;
    public int brushSize = 3;
    public int interpolateCount = 4;

    private Texture2D[] writableTextures;
    private MeshRenderer meshRenderer;
    private int currentFrame = 0;
    private int frameCount = 0;
    private float animTimer = 0f;
    private bool canPaint = true;
    private bool isAnimating = true;
    private bool isBrushing = false;

    public bool IsBrushing => isBrushing;

    private Vector2 lastPaintPoint;
    private Vector3 lastPosition;

    public bool AccuracyDirty { get; private set; } = true;
    public void ConsumeAccuracyDirty() => AccuracyDirty = false;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        frameCount = frameTextures.Length;
        writableTextures = new Texture2D[frameCount];
        lastPosition = transform.position;

        for (int i = 0; i < frameCount; i++)
        {
            writableTextures[i] = new Texture2D(
                frameTextures[i].width,
                frameTextures[i].height,
                TextureFormat.RGBA32,
                false
            );

            writableTextures[i].SetPixels(frameTextures[i].GetPixels());
            writableTextures[i].Apply();
        }

        RefreshTextures();
        UpdateCursorColor();
    }

    void Update()
    {
        HandleAnimation();
        HandleBrushStop();
        HandlePainting();
    }

    void HandleAnimation()
    {
        if (!isAnimating) return;

        float distanceMoved = (transform.position - lastPosition).magnitude;
        animTimer += distanceMoved * baseAnimationSpeed;

        if (animTimer >= 1.0f)
        {
            animTimer = 0f;
            currentFrame = (currentFrame + 1) % frameCount;
            RefreshTextures();

            if (currentFrame % 2 == 0 && footstepSource != null)
                footstepSource.Play();
        }

        lastPosition = transform.position;
    }

    void HandlePainting()
    {
        if (!canPaint || Mouse.current == null)
            return;

        if (Mouse.current.leftButton.isPressed)
        {
            if (!HandlePaint())
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                    HandleColorPick();
            }
        }
    }

    void HandleBrushStop()
    {
        if (isBrushing && (Mouse.current == null || !Mouse.current.leftButton.isPressed))
        {
            brushSource?.Stop();
            isBrushing = false;
        }
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

                if (!isBrushing)
                {
                    brushSource?.Play();
                    isBrushing = true;
                }

                return PaintAt(x, y);
            }
        }

        return false;
    }

    bool HandleColorPick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f).OrderBy(h => h.distance).ToArray();

        foreach (RaycastHit hit in hits)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            MeshCollider meshCollider = hit.collider as MeshCollider;

            if (renderer == null || meshCollider == null)
                continue;

            Texture2D tex = renderer.sharedMaterial.mainTexture as Texture2D;
            if (tex == null)
                continue;

            Vector2 pixelUV = hit.textureCoord;
            int x = Mathf.FloorToInt(pixelUV.x * tex.width);
            int y = Mathf.FloorToInt(pixelUV.y * tex.height);

            Color pixelColor = tex.GetPixel(x, y);
            if (pixelColor.a < 0.1f)
                continue;

            paintColor = pixelColor;
            UpdateCursorColor();
            colorPickSource?.Play();
            return true;
        }

        return false;
    }

    void UpdateCursorColor()
    {
        if (brushCursor == null || brushTipCursor == null)
            return;

        int width = brushCursor.width;
        int height = brushCursor.height;

        Texture2D combined = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color[] brushPixels = brushCursor.GetPixels();
        Color[] tipPixels = brushTipCursor.GetPixels();
        Color[] result = new Color[brushPixels.Length];

        for (int i = 0; i < brushPixels.Length; i++)
        {
            Color tintedTip = tipPixels[i];

            if (tipPixels[i].a > 0.0f)
                tintedTip = paintColor;

            result[i] = Color.Lerp(tintedTip, brushPixels[i], brushPixels[i].a);
        }

        combined.SetPixels(result);
        combined.Apply();

        Cursor.SetCursor(combined, new Vector2(width / 2f, height / 2f), CursorMode.ForceSoftware);
    }

    bool PaintAt(int centerX, int centerY)
    {
        bool changed = PaintCircle(centerX, centerY);

        if (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            int dx = Mathf.FloorToInt((lastPaintPoint.x - centerX) / (float)interpolateCount);
            int dy = Mathf.FloorToInt((lastPaintPoint.y - centerY) / (float)interpolateCount);

            for (int i = 0; i < interpolateCount; i++)
                changed |= PaintCircle(centerX + dx * i, centerY + dy * i);
        }

        lastPaintPoint = new Vector2(centerX, centerY);

        if (changed)
        {
            foreach (var tex in writableTextures)
                tex.Apply();

            AccuracyDirty = true;
        }

        return changed;
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

                if (px < 0 || px >= width || py < 0 || py >= height)
                    continue;

                if (Vector2.SqrMagnitude(new Vector2(i, j)) > brushSize * brushSize)
                    continue;

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

    void RefreshTextures()
    {
        meshRenderer.material.mainTexture = writableTextures[currentFrame];

        if (overlayMeshRenderer != null && overlayTextures.Length > currentFrame)
            overlayMeshRenderer.material.mainTexture = overlayTextures[currentFrame];
    }

    public void SetAnimating(bool enabled) => isAnimating = enabled;
    public void SetPaintingEnabled(bool enabled) => canPaint = enabled;

    public float CalculateAccuracy(Texture2D target)
    {
        if (target == null) return 0;

        // パフォーマンスのため、配列を一括取得
        Color[] playerPixels = writableTextures[0].GetPixels();
        Color[] maskPixels = frameTextures[0].GetPixels();
        Color[] targetPixels = target.GetPixels();

        float totalWeight = 0f;
        float weightedMatchScore = 0f;

        for (int i = 0; i < playerPixels.Length; i++)
        {
            if (maskPixels[i].a > 0.1f)
            {
                float weight = targetPixels[i].a;

                if (weight > 0f)
                {
                    totalWeight += weight;
                    if (playerPixels[i] == targetPixels[i])
                        weightedMatchScore += weight;
                }
            }
        }
        if (totalWeight <= 0.0001f) return 0f;
        Debug.Log((weightedMatchScore / totalWeight) * 100f);
        return (weightedMatchScore / totalWeight) * 100f;
    }
}
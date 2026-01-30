using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ChameleonCamouflageGame : MonoBehaviour
{
    [Header("Textures")]
    public Texture2D[] frameTextures = new Texture2D[2]; // Original chameleon source art
    public Texture2D targetTexture; // Reference image to match (256x256)
    public float animationSpeed = 0.5f;

    [Header("Paint Settings")]
    public Color paintColor = Color.red;
    public int brushSize = 5;

    [Header("Objects & Positions")]
    public Transform targetTransform; // Transform for the target image on the right
    public Vector3 centerPosition = Vector3.zero; // Target coordinates for the center
    public float moveDuration = 1.0f; // Time taken to move to center

    private Texture2D[] writableTextures = new Texture2D[2];
    private MeshRenderer meshRenderer;
    private int currentFrame = 0;
    private float animTimer = 0f;
    private bool isJudging = false;

    [Header("Timer")]
    public Timer timer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // Initialize writable textures
        for (int i = 0; i < 2; i++)
        {
            writableTextures[i] = new Texture2D(frameTextures[i].width, frameTextures[i].height, TextureFormat.RGBA32, false);
            // Copy pixels from the original source
            writableTextures[i].SetPixels(frameTextures[i].GetPixels());
            writableTextures[i].Apply();
        }

        meshRenderer.material.mainTexture = writableTextures[0];
    }

    void Update()
    {
        if (isJudging) return; // Disable controls during the judgment sequence

        // 1. Animation Logic
        animTimer += Time.deltaTime;
        if (animTimer >= animationSpeed)
        {
            animTimer = 0f;
            currentFrame = (currentFrame + 1) % 2;
            meshRenderer.material.mainTexture = writableTextures[currentFrame];
        }

        // 2. Paint Logic (New Input System)
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            HandlePaint();
        }

        // 3. Color Selection (Shortcut keys)
        if (Keyboard.current.digit1Key.wasPressedThisFrame) paintColor = Color.red;
        if (Keyboard.current.digit2Key.wasPressedThisFrame) paintColor = Color.green;
        if (Keyboard.current.digit3Key.wasPressedThisFrame) paintColor = Color.blue;

        // 4. Start judgment sequence with Space key
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            //------new add from Yuan-----
            OnTimeUpOrJudge();


            StartCoroutine(JudgeSequence());

        }

        //------new add from Yuan-----
        //judging when times up
        if (!isJudging && timer != null && timer.IsFinished)
        {
            OnTimeUpOrJudge();
            return;
        }
    }

    void HandlePaint()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
        {
            Vector2 uv = hit.textureCoord;
            int x = (int)(uv.x * writableTextures[0].width);
            int y = (int)(uv.y * writableTextures[0].height);
            PaintAt(x, y);
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

                for (int f = 0; f < 2; f++)
                {
                    // Check alpha of the original frame to use as a mask
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

    // Sequence triggered when Space key is pressed
    IEnumerator JudgeSequence()
    {
        isJudging = true;

        // Freeze animation on the first frame
        currentFrame = 0;
        meshRenderer.material.mainTexture = writableTextures[0];

        // Move objects to the center
        Vector3 startPosCam = transform.position;
        Vector3 startPosTarget = targetTransform.position;
        float elapsed = 0;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            // Smooth movement using SmoothStep interpolation
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPosCam, centerPosition, t);
            targetTransform.position = Vector3.Lerp(startPosTarget, centerPosition, t);
            yield return null;
        }

        // Execute accuracy check
        float accuracy = CalculateAccuracy();
        Debug.Log($"Accuracy Rate: {accuracy:F2}%");

        // UI display logic can be added here
    }

    float CalculateAccuracy()
    {
        Color[] playerPixels = writableTextures[0].GetPixels();
        Color[] targetPixels = targetTexture.GetPixels();
        Color[] maskPixels = frameTextures[0].GetPixels();

        float totalChameleonPixels = 0;
        float totalDiff = 0;

        for (int i = 0; i < playerPixels.Length; i++)
        {
            // Only compare pixels where the chameleon actually exists (based on mask)
            if (maskPixels[i].a > 0.1f)
            {
                totalChameleonPixels++;

                // Calculate color difference (RGB distance)
                float rDiff = Mathf.Abs(playerPixels[i].r - targetPixels[i].r);
                float gDiff = Mathf.Abs(playerPixels[i].g - targetPixels[i].g);
                float bDiff = Mathf.Abs(playerPixels[i].b - targetPixels[i].b);

                // Average difference across 3 channels (0.0 to 1.0)
                float diff = (rDiff + gDiff + bDiff) / 3f;
                totalDiff += diff;
            }
        }

        if (totalChameleonPixels == 0) return 0;

        // Accuracy (%) = (1.0 - Average Error) * 100
        float averageError = totalDiff / totalChameleonPixels;
        return (1.0f - averageError) * 100f;
    }


    //------new add from Yuan-----
    #region Timer
    public void OnTimeUpOrJudge()
    {
        if (isJudging) return;


        if (timer != null) timer.StopTimer();

        StartCoroutine(JudgeThenNextRound());
    }

    public void StartNewRound()//new round setting(like adding clear color)
    {
        
        isJudging = false;

    }

    IEnumerator JudgeThenNextRound()
    {
        yield return StartCoroutine(JudgeSequence());

        yield return new WaitForSeconds(3f);

        StartNewRound();
    }
    #endregion
}
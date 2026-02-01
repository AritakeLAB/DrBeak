using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class judgeBar : MonoBehaviour
{
    [Header("References")]
    public GameDirector director;
    public ChameleonCamouflageCalc painter;

    [Header("UI Elements")]
    public Slider accuracySlider;          // Value range: 0..1
    public Image fillImage;                // The image component of the slider's Fill area
    public RectTransform barRect;          // Slider background (used to calculate width)
    public RectTransform targetMarker;     // UI element indicating the required threshold

    [Header("Color Settings")]
    public Color safeColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;

    [Header("Refresh Settings")]
    public float minRefreshInterval = 0.15f;  // Throttling: prevents excessive pixel calculations
    private float _timer = 0f;

    void Start()
    {
        // Initialize slider range
        accuracySlider.minValue = 0f;
        accuracySlider.maxValue = 1f;

        UpdateTargetMarker();   // Position the target marker initially
        ForceUpdateAccuracy();  // Initial calculation
    }

    void Update()
    {
        if (director == null || painter == null) return;

        // Update target marker position (could change per checkpoint)
        UpdateTargetMarker();

        // Refresh
        _timer += Time.deltaTime;
        if (!painter.AccuracyDirty && _timer < minRefreshInterval) return;
        _timer = 0f;

        ForceUpdateAccuracy();
        painter.ConsumeAccuracyDirty();
    }

    void ForceUpdateAccuracy()
    {
        Texture2D target = director.CurrentTargetTexture;
        float accuracy = painter.CalculateAccuracy(target); // Returns 0..100

        // Update slider value
        accuracySlider.value = Mathf.Clamp01(accuracy / 100f);

        // Update bar color based on survival conditions
        UpdateBarColor(accuracy);
    }

    void UpdateBarColor(float accuracy)
    {
        if (fillImage == null) return;

        float potentialVisibility = 100f - accuracy;

        // 1. Red: Accuracy is too low (below 50%) to pass the checkpoint
        if (accuracy < 50f)
        {
            fillImage.color = dangerColor;
        }
        // 2. Yellow: Accuracy is over 50%, but cumulative visibility will exceed 100%
        else if (director.totalVisibility + potentialVisibility >= 100f)
        {
            fillImage.color = warningColor;
        }
        // 3. Green: Safe state
        else
        {
            fillImage.color = safeColor;
        }
    }

    void UpdateTargetMarker()
    {
        if (barRect == null || targetMarker == null || director == null) return;

        float targetAcc = Mathf.Clamp(director.CurrentTargetAccuracy, 0f, 100f);
        float t = targetAcc / 100f;

        // Calculate horizontal position relative to barRect width
        float width = barRect.rect.width;

        // Assumes barRect pivot/anchor is centered. Map [0..1] to [-width/2, +width/2]
        float x = Mathf.Lerp(-width * 0.5f, width * 0.5f, t);

        Vector2 pos = targetMarker.anchoredPosition;
        pos.x = x;
        targetMarker.anchoredPosition = pos;
    }

    public void SetCheckpointTarget(Texture2D targetTex, float targetAcc)
    {
        // Update marker position immediately for the new checkpoint
        UpdateTargetMarker();

        // Refresh accuracy calculation for the new target
        ForceUpdateAccuracy();
        painter.ConsumeAccuracyDirty(); // Prevent redundant refresh in the next frame
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class judgeBar : MonoBehaviour
{
    [Header("Refs")]
    public GameDirector director;
    public ChameleonCamouflageCalc painter;

    [Header("UI")]
    public Slider accuracySlider;          // 0..1
    public RectTransform barRect;          // 进度条背景（用来算宽度）
    public RectTransform targetMarker;     // 目标点
    [Header("Refresh")]
    public float minRefreshInterval = 0.15f;  // 节流：避免疯狂算像素
    private float _timer = 0f;

    void Start()
    {
        // 初始化（如果你slider范围不是0..1，这里保证一下）
        accuracySlider.minValue = 0f;
        accuracySlider.maxValue = 1f;

        UpdateTargetMarker();   // 先摆好目标点
        ForceUpdateAccuracy();  // 初始算一次
    }

    void Update()
    {
        if (director == null || painter == null) return;

        // 目标点可能会随关卡变：如果你想每帧都更新也行，但一般不需要
        // 这里做一个轻量更新（也可以改成事件驱动）
        UpdateTargetMarker();

        // 只有“玩家确实画过”才考虑刷新
        if (!painter.AccuracyDirty) return;

        _timer += Time.deltaTime;
        if (_timer < minRefreshInterval) return;
        _timer = 0f;

        ForceUpdateAccuracy();
        painter.ConsumeAccuracyDirty();
    }

    void ForceUpdateAccuracy()
    {
        Texture2D target = director.CurrentTargetTexture;
        float accuracy = painter.CalculateAccuracy(target); // 0..100

        accuracySlider.value = Mathf.Clamp01(accuracy / 100f);
    }

    void UpdateTargetMarker()
    {
        if (barRect == null || targetMarker == null || director == null) return;

        float targetAcc = Mathf.Clamp(director.CurrentTargetAccuracy, 0f, 100f);
        float t = targetAcc / 100f;

        // 把目标点放到 barRect 的局部坐标里
        float width = barRect.rect.width;

        // barRect pivot 通常在中间/左边都可能，这里用 anchoredPosition 直接算最稳：
        // 假设 barRect 的 anchor/pivot 是默认居中，那我们把marker放到 [-width/2, +width/2]
        float x = Mathf.Lerp(-width * 0.5f, width * 0.5f, t);

        Vector2 pos = targetMarker.anchoredPosition;
        pos.x = x;
        targetMarker.anchoredPosition = pos;
    }

    public void SetCheckpointTarget(Texture2D targetTex, float targetAcc)
    {

        // 目标点立刻更新
        UpdateTargetMarker();

        // 如果希望进入下一段路时进度条也立刻刷新一次（可选）
        ForceUpdateAccuracy();
        painter.ConsumeAccuracyDirty(); // 避免下一帧又重复刷新
    }
}

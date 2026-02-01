using UnityEngine;
using System.Collections;

public class GirlController : MonoBehaviour
{
    public enum GirlState { Idle, LookUp }

    [Header("Animation Frames")]
    public Sprite[] idleFrames;   // Girl_Idle の30枚をドラッグ&ドロップ
    public Sprite[] lookUpFrames; // Girl_Look_Up の30枚をドラッグ&ドロップ

    [Header("Settings")]
    public float fps = 8f; // 1秒間に30枚再生

    private SpriteRenderer spriteRenderer;
    private GirlState currentState = GirlState.Idle;
    private int frameIndex;
    private float timer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        float frameDuration = 1f / fps;

        if (timer >= frameDuration)
        {
            timer -= frameDuration;
            UpdateAnimation();
        }
    }

    void UpdateAnimation()
    {
        Sprite[] currentArray = (currentState == GirlState.Idle) ? idleFrames : lookUpFrames;

        if (currentArray == null || currentArray.Length == 0) return;

        // 次のフレームへ（ループ再生）
        frameIndex = (frameIndex + 1) % currentArray.Length;
        spriteRenderer.sprite = currentArray[frameIndex];
    }

    // 外部（GameDirector）から状態を切り替えるメソッド
    public void SetState(GirlState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        frameIndex = 0; // 切り替え時に最初のフレームに戻す
        timer = 0;
    }
}
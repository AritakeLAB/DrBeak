using UnityEngine;

public class GirlController : MonoBehaviour
{
    public enum GirlState { Idle, LookUp }

    [Header("Animation Textures")]
    public Texture2D[] idleTextures;   // Girl_Idle (30枚)
    public Texture2D[] lookUpTextures; // Girl_Look_Up (30枚)

    [Header("Settings")]
    public float fps = 30f;

    private MeshRenderer meshRenderer;
    private Material targetMaterial;
    private GirlState currentState = GirlState.Idle;
    private int frameIndex;
    private float timer;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        // マテリアルのインスタンスを取得 (元のAssetを書き換えないように)
        targetMaterial = meshRenderer.material;
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
        Texture2D[] currentArray = (currentState == GirlState.Idle) ? idleTextures : lookUpTextures;

        if (currentArray == null || currentArray.Length == 0) return;

        // 次のフレームのテクスチャをセット
        frameIndex = (frameIndex + 1) % currentArray.Length;
        targetMaterial.mainTexture = currentArray[frameIndex];
    }

    // GameDirectorから呼ぶ状態切り替え
    public void SetState(GirlState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        frameIndex = 0;
        timer = 0;

        // 切り替え時に1枚目を即座に反映
        UpdateAnimation();
    }
}
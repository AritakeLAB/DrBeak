using UnityEngine;

public class TextureAnimation : MonoBehaviour
{
    public Texture2D[] animatedTextures;
    public float animationSpeed = 1.0f;

    private MeshRenderer meshRenderer;
    private int currentFrame = 0;
    private int frameCount;
    private float animTimer = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        frameCount = animatedTextures.Length;
    }

    // Update is called once per frame
    void Update()
    {
    
        animTimer += Time.deltaTime;
        if (animTimer >= animationSpeed)
        {
            animTimer = 0f;
            currentFrame = (currentFrame + 1) % frameCount;
            meshRenderer.material.mainTexture = animatedTextures[currentFrame];
        }
    }
}

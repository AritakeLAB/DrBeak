using UnityEngine;

public class FlashingGhost : MonoBehaviour
{

    [Header("AlphaSetting")]
    public float maxA= 1f;  //max alpha
    public float minA = 0.2f;   //min alpha
    public float period = 1.5f; //loop time

    private SpriteRenderer sr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        ChangeColor();
    }

    void ChangeColor()
    {
        float t = (Mathf.Sin(Time.time * (2f * Mathf.PI / period)) + 1f) * 0.5f;
        float a = Mathf.Lerp(minA, maxA, t);

        var c = sr.color;
        c.a = a;
        sr.color = c;
    }
}

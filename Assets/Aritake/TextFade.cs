using TMPro;
using UnityEngine;

public class TextFade : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float frequency = 1.0f;
    public float amplitude = 1.0f;

    void Awake()
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        float alpha = Mathf.Lerp(0.2f, 1f, (Mathf.Sin(Time.time * frequency) + 1f) * 0.5f);
        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }
}

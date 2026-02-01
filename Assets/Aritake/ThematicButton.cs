using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ThematicButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public Image buttonImage;
    public TextMeshProUGUI buttonText;

    [Header("Style Settings")]
    public Color normalColor = Color.gray;     // 通常時（鉛筆書き風）
    public Color hoverColor = Color.red;      // ホバー時（色が塗られた状態）
    public float scaleEffect = 1.1f;          // 少し膨らむ演出

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        buttonImage.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 色が塗られ、少し大きくなる
        buttonImage.CrossFadeColor(hoverColor, 0.2f, true, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 色が抜けて元に戻る
        buttonImage.CrossFadeColor(normalColor, 0.2f, true, true);
        transform.localScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 押し込んだ時の「ペチャッ」とした感触
        transform.localScale = originalScale * 0.95f;
    }
}
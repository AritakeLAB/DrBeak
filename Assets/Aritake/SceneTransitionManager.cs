using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("UI Reference")]
    public CanvasGroup fadeCanvasGroup; // 全面真っ白なImageを持つCanvasGroup
    public float fadeDuration = 0.6f;

    private void Awake()
    {
        // 最初は真っ白な状態からスタート
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;
            StartCoroutine(Fade(0f)); // フェードイン
        }
    }

    public IEnumerator FadeOutRoutine() => Fade(1f);
    public IEnumerator FadeInRoutine() => Fade(0f);

    private IEnumerator Fade(float targetAlpha)
    {
        fadeCanvasGroup.blocksRaycasts = true;
        float startAlpha = fadeCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // TimeScaleの影響を受けない
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
        if (targetAlpha <= 0) fadeCanvasGroup.blocksRaycasts = false;
    }
}
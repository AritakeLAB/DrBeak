using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CriWare;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("UI Reference")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.6f;

    [Header("Optional Audio Fade")]
    public CriAtomSource musicToFade;       // menu or music
    public CriAtomSource ambienceToFade;    // ambience source

    private void Awake()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;
            StartCoroutine(Fade(0f));
        }
    }

    public IEnumerator FadeOutRoutine()
    {
        yield return Fade(1f);
    }

    public IEnumerator FadeInRoutine()
    {
        yield return Fade(0f);
    }

    private IEnumerator Fade(float targetAlpha)
    {
        fadeCanvasGroup.blocksRaycasts = true;

        float startAlpha = fadeCanvasGroup.alpha;
        float elapsed = 0f;

        float startMusicVolume = 1f;
        float startAmbienceVolume = 1f;

        if (musicToFade != null)
            startMusicVolume = musicToFade.volume;

        if (ambienceToFade != null)
            startAmbienceVolume = ambienceToFade.volume;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;

            // Screen fade
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            // Audio fade only when fading OUT
            if (targetAlpha == 1f)
            {
                if (musicToFade != null)
                    musicToFade.volume = Mathf.Lerp(startMusicVolume, 0f, t);

                if (ambienceToFade != null)
                    ambienceToFade.volume = Mathf.Lerp(startAmbienceVolume, 0f, t);
            }

            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;

        if (targetAlpha == 1f)
        {
            if (musicToFade != null)
            {
                musicToFade.volume = 0f;
                musicToFade.Stop();
            }

            if (ambienceToFade != null)
            {
                ambienceToFade.volume = 0f;
                ambienceToFade.Stop();
            }
        }

        if (targetAlpha <= 0f)
            fadeCanvasGroup.blocksRaycasts = false;
    }
}
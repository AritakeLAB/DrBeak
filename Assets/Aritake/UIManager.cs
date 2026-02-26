using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using CriWare;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Managers")]
    public SceneTransitionManager transition;
    public TutorialManager tutorialManager;
    public StoryManager storyManager;

    [Header("UI Panels")]
    public GameObject menuPanel;
    public GameObject levelSelectPanel;
    public GameObject gameOverPanel;
    public GameObject resultPanel;
    public GameObject hudPanel;

    [Header("UI Audio Sources")]
    public CriAtomSource menuOpenSource;
    public CriAtomSource menuConfirmSource;
    public CriAtomSource leaderboardSource;

    [Header("Menu Music")]
    public CriAtomSource menuMusicSource;
    public string menuLowPassAisacName = "Menu_Music_LowPass";
    public float filterFadeDuration = 1.0f;

    [Header("Text References")]
    public TextMeshProUGUI resultScoreText;
    public TextMeshProUGUI resultRankText;
    public TextMeshProUGUI gameOverReasonText;

    private bool isTransitioning = false;
    private float currentFilterValue = 0f;

    //  Prevent double UI sound trigger
    private float lastUISoundTime = 0f;
    private float uiSoundCooldown = 0.1f;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;

        ShowMenuImmediate();
        StartCoroutine(InitializeMenuMusic());
    }

    private IEnumerator InitializeMenuMusic()
    {
        yield return null;

        if (menuMusicSource == null)
            yield break;

        menuMusicSource.Play();
        yield return null;

        if (menuMusicSource.player != null)
        {
            currentFilterValue = 0f;
            menuMusicSource.player.SetAisacControl(menuLowPassAisacName, 0f);
        }
    }

    // =================================================
    // SAFE UI SOUND (ANTI DOUBLE TRIGGER)
    // =================================================

    private void PlayUI(CriAtomSource source)
    {
        if (source == null) return;

        if (Time.unscaledTime - lastUISoundTime < uiSoundCooldown)
            return;

        lastUISoundTime = Time.unscaledTime;

        source.Stop();
        source.Play();
    }

    // =================================================
    // PANEL CONTROL
    // =================================================

    public void ShowMenuImmediate()
    {
        HideAll();
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    public void OpenLevelSelect()
    {
        if (isTransitioning) return;

        PlayUI(menuOpenSource);

        if (menuPanel != null) menuPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);

        // Smooth lowpass fade when entering level select
        StartCoroutine(FilterFade(1f));
    }

    // =================================================
    // START GAME FLOW
    // =================================================

    public void OnLevelSelected(string levelName)
    {
        if (isTransitioning) return;

        isTransitioning = true;

        PlayUI(menuConfirmSource);

        StartCoroutine(StartGameRoutine(levelName));
    }

    private IEnumerator StartGameRoutine(string levelName)
    {
        if (transition != null)
            yield return transition.FadeOutRoutine();

        SceneManager.LoadScene(levelName);
    }

    // =================================================
    // FILTER FADE
    // =================================================

    private IEnumerator FilterFade(float target)
    {
        if (menuMusicSource == null || menuMusicSource.player == null)
            yield break;

        float start = currentFilterValue;
        float time = 0f;

        while (time < filterFadeDuration)
        {
            time += Time.deltaTime;
            currentFilterValue = Mathf.Lerp(start, target, time / filterFadeDuration);

            menuMusicSource.player.SetAisacControl(menuLowPassAisacName, currentFilterValue);
            yield return null;
        }

        currentFilterValue = target;
        menuMusicSource.player.SetAisacControl(menuLowPassAisacName, currentFilterValue);
    }

    // =================================================
    // GAME OVER / RESULT
    // =================================================

    public void ShowGameOver(string reason)
    {
        StartCoroutine(EndGameRoutine(gameOverPanel, () =>
        {
            if (gameOverReasonText != null)
                gameOverReasonText.text = reason;
        }));
    }

    public void ShowResult(float finalVisibility)
    {
        StartCoroutine(EndGameRoutine(resultPanel, () =>
        {
            if (resultScoreText != null)
                resultScoreText.text = (100f - finalVisibility).ToString("F1") + "% Camouflage";

            if (resultRankText != null)
            {
                resultRankText.text = GetRank(finalVisibility);
                resultRankText.color = GetRankColor(resultRankText.text);
            }

            PlayUI(leaderboardSource);
        }));
    }

    private IEnumerator EndGameRoutine(GameObject targetPanel, System.Action onPanelReady)
    {
        yield return new WaitForSecondsRealtime(1f);

        if (transition != null)
            yield return transition.FadeOutRoutine();

        HideAll();
        if (targetPanel != null) targetPanel.SetActive(true);
        onPanelReady?.Invoke();

        if (transition != null)
            yield return transition.FadeInRoutine();
    }

    public void BackToTitle()
    {
        if (isTransitioning) return;

        isTransitioning = true;

        PlayUI(menuConfirmSource);

        StartCoroutine(BackToTitleRoutine());
    }

    private IEnumerator BackToTitleRoutine()
    {
        yield return StartCoroutine(FilterFade(0f));

        if (transition != null)
            yield return transition.FadeOutRoutine();

        SceneManager.LoadScene(1);
    }

    private void HideAll()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
    }

    // =================================================
    // RANK LOGIC
    // =================================================

    private string GetRank(float visibility)
    {
        if (visibility < 30) return "S: Perfect Mimicry";
        if (visibility < 50) return "A: Professional Stealth";
        if (visibility < 70) return "B: Mediocre Camouflage";
        if (visibility < 80) return "C: Barely Hidden";
        return "F: Total Exposure";
    }

    private Color GetRankColor(string rankText)
    {
        if (rankText.StartsWith("S")) return Color.cyan;
        if (rankText.StartsWith("A")) return Color.green;
        if (rankText.StartsWith("B")) return Color.yellow;
        return Color.red;
    }

    public void OpenTutorial()
    {
        tutorialManager?.StartTutorial();
    }

    public void OpenStory()
    {
        storyManager?.StartStory();
    }
}
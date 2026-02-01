using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Managers")]
    public SceneTransitionManager transition;
    public TutorialManager tutorialManager;

    [Header("UI Panels")]
    public GameObject menuPanel;
    public GameObject levelSelectPanel;
    public GameObject gameOverPanel;
    public GameObject resultPanel;
    public GameObject hudPanel; // ゲーム中のスコア表示など


    [Header("Text References")]
    public TextMeshProUGUI resultScoreText;
    public TextMeshProUGUI resultRankText;
    public TextMeshProUGUI gameOverReasonText;

    private void Awake()
    {
        Time.timeScale = 1;
        Instance = this;
        ShowMenuImmediate();
    }

    // --- 画面切り替えメソッド ---

    public void ShowMenuImmediate()
    {
        HideAll();
        menuPanel.SetActive(true);
    }

    public void OpenLevelSelect()
    {
        menuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    public void OnLevelSelected(string levelName)
    {
        StartCoroutine(StartGameRoutine(levelName));
    }

    public void ShowGameOver(string reason)
    {
        StartCoroutine(EndGameRoutine(gameOverPanel, () => {
            gameOverReasonText.text = reason;
        }));
    }

    public void ShowResult(float finalVisibility)
    {
        StartCoroutine(EndGameRoutine(resultPanel, () => {
            resultScoreText.text = $"Visibility: {finalVisibility:F1}%";
            resultRankText.text = GetRank(finalVisibility);
            resultRankText.color = GetRankColor(resultRankText.text);
        }));
    }

    // --- ルーチン処理 ---

    private IEnumerator StartGameRoutine(string levelName)
    {
        yield return transition.FadeOutRoutine();
        SceneManager.LoadScene(levelName);
    }

    private IEnumerator EndGameRoutine(GameObject targetPanel, System.Action onPanelReady)
    {
        yield return new WaitForSecondsRealtime(1.0f); // 判定後の余韻
        yield return transition.FadeOutRoutine();

        HideAll();
        targetPanel.SetActive(true);
        onPanelReady?.Invoke();

        yield return transition.FadeInRoutine();
    }

    public void BackToTitle()
    {
        StartCoroutine(BackToTitleRoutine());
    }

    private IEnumerator BackToTitleRoutine()
    {
        yield return transition.FadeOutRoutine();
        SceneManager.LoadScene(1); // タイトルシーン
    }

    private void HideAll()
    {
        menuPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        resultPanel.SetActive(false);
        if (hudPanel) hudPanel.SetActive(false);
    }

    // --- 演出用補助メソッド ---

    private string GetRank(float visibility)
    {
        if (visibility < 10) return "S: Perfect Mimicry";
        if (visibility < 30) return "A: Professional Stealth";
        if (visibility < 60) return "B: Mediocre Camouflage";
        if (visibility < 90) return "C: Barely Hidden";
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
        tutorialManager.StartTutorial();
    }
}
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
    public GameObject hudPanel; // �Q�[�����̃X�R�A�\���Ȃ�
    public CriAtomSource clickSource;


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

    // --- ��ʐ؂�ւ����\�b�h ---

    public void ClickSound()
    {
        clickSource.Play();
    }
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
            resultScoreText.text = $"{100f - finalVisibility:F1}% Camouflage";
            resultRankText.text = GetRank(finalVisibility);
            resultRankText.color = GetRankColor(resultRankText.text);
        }));
    }

    // --- ���[�`������ ---

    private IEnumerator StartGameRoutine(string levelName)
    {
        yield return transition.FadeOutRoutine();
        SceneManager.LoadScene(levelName);
    }

    private IEnumerator EndGameRoutine(GameObject targetPanel, System.Action onPanelReady)
    {
        yield return new WaitForSecondsRealtime(1.0f); // �����̗]�C
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
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        SceneManager.LoadScene(1); // �^�C�g���V�[��
    }

    private void HideAll()
    {
        menuPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        resultPanel.SetActive(false);
        if (hudPanel) hudPanel.SetActive(false);
    }

    // --- ���o�p�⏕���\�b�h ---

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
        tutorialManager.StartTutorial();
    }

    public void OpenStory()
    {
        storyManager.StartStory();
    }
}
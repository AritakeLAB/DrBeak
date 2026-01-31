using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject menuScreen;
    public GameObject levelSelectScreen;
    public GameObject gameOverScreen;
    public GameObject resultScreen;

    [Header("Result/GameOver Texts")]
    public Text resultScoreText;
    public Text gameOverReasonText;

    public void ShowMenu()
    {
        HideAll();
        menuScreen.SetActive(true);
        Time.timeScale = 0;
    }

    public void ShowLevelSelect()
    {
        HideAll();
        levelSelectScreen.SetActive(true);
    }

    public void StartGame(string levelName)
    {
        GameData.SelectedLevelName = levelName;
        HideAll();
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowGameOver(string reason)
    {
        HideAll();
        gameOverScreen.SetActive(true);
        gameOverReasonText.text = reason;
        Time.timeScale = 0;
    }

    public void ShowResult(float finalScore)
    {
        HideAll();
        resultScreen.SetActive(true);
        resultScoreText.text = $"Total Visibility: {finalScore:F1}%";
        Time.timeScale = 0;
    }

    public void BackToTitle()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    private void HideAll()
    {
        menuScreen.SetActive(false);
        levelSelectScreen.SetActive(false);
        gameOverScreen.SetActive(false);
        resultScreen.SetActive(false);
    }
}
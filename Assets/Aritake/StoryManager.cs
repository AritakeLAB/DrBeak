using UnityEngine;
using UnityEngine.UI;

public class StoryManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tutorialPanel; // チュートリアル全体のパネル
    public Image displayImage;       // 画像を表示するUI Image

    [Header("Tutorial Assets")]
    public Sprite[] tutorialSlides;  // チュートリアル画像（n枚）をセット

    private int currentIndex = 0;

    // チュートリアルを開始する
    public void StartStory()
    {
        if (tutorialSlides.Length == 0) return;

        currentIndex = 0;
        tutorialPanel.SetActive(true);
        UpdateDisplay();
    }

    // クリックされた時に呼ばれる（フルスクリーンのボタンなどにアサイン）
    public void OnClickNext()
    {
        currentIndex++;

        if (currentIndex < tutorialSlides.Length)
        {
            UpdateDisplay();
        }
        else
        {
            CloseTutorial();
        }
    }

    private void UpdateDisplay()
    {
        displayImage.sprite = tutorialSlides[currentIndex];
    }

    private void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
        // 必要に応じてUIManager経由でSelectLevel画面を再表示させる
    }
}
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class Timer : MonoBehaviour
{
    [Header("Time")]
    [Min(0f)] public float startSeconds = 60f;
    public bool autoStart = true;

    [Header("UI")]
    public TextMeshPro tmpText;

    public float Remaining => remaining;
    public bool IsRunning => running;
    public bool IsFinished => finished;

    public event Action OnFinished;

    float remaining;
    bool running;
    bool finished;

    void Start()
    {
        remaining = startSeconds;
        finished = false;
        running = false;
        UpdateUI(remaining);
        if (autoStart) StartCountdown(startSeconds);
    }

    public void StartCountdown(float seconds)
    {
        startSeconds = Mathf.Max(0f, seconds);
        remaining = startSeconds;
        finished = false;
        running = true;
        UpdateUI(remaining);
    }

    public void StopTimer()   // “停止计时”（暂停在当前剩余时间）
    {
        running = false;
    }

    public void ResumeTimer()
    {
        if (!finished) running = true;
    }

   

    void Update()
    {
        if (!running || finished) return;

        remaining -= Time.deltaTime;

        if (remaining <= 0f)
        {
            remaining = 0f;
            UpdateUI(remaining);
            Finish();
            return;
        }

        UpdateUI(remaining);
    }

    void Finish()
    {
        finished = true;
        running = false;
        OnFinished?.Invoke();

    }

    void UpdateUI(float seconds)
    {
        int total = Mathf.CeilToInt(seconds);
        int m = total / 60;
        int s = total % 60;
        string text = $"{m:00}:{s:00}";

        if (tmpText != null) tmpText.text = text;
    }
}

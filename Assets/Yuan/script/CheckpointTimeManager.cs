using UnityEngine;
using System.Collections.Generic;

public class CheckpointTimeManager : MonoBehaviour
{
    public static CheckpointTimeManager Instance { get; private set; }

    [Header("References")]
    public Timer timer;
    public ChameleonCamouflageGame game; //judge when times up

    [Header("Checkpoints (in order)")]
    public List<CheckpointTrigger> checkpoints = new List<CheckpointTrigger>();

    int currentIndex = -1;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        if (timer != null) timer.OnFinished += HandleTimeUp;
    }

    void OnDisable()
    {
        if (timer != null) timer.OnFinished -= HandleTimeUp;
    }

    // 被 CheckpointTrigger 调用
    public void OnReachCheckpoint(CheckpointTrigger cp)
    {
        if (timer == null) return;

        int idx = checkpoints.IndexOf(cp);
        if (idx < 0)
        {
            Debug.LogWarning("该检查点不在 checkpoints List 中！");
            return;
        }

        
        if (idx != currentIndex + 1) return;

        currentIndex = idx;

        float sec = Mathf.Max(0f, cp.countdownSeconds);
        timer.StartCountdown(sec);

        Debug.Log($"到达检查点 {currentIndex + 1}，倒计时开始：{sec} 秒");
    }

    void HandleTimeUp()
    {
        // 时间到 → 进入判定
        if (game != null)
        {
            game.OnTimeUpOrJudge();
        }
        else
        {
            Debug.LogWarning("CheckpointTimerManager: 未绑定 ChameleonCamouflageGame");
        }
    }
}

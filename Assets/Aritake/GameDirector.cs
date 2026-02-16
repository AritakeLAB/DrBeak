using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CriWare;

public class GameDirector : MonoBehaviour
{
    [System.Serializable]
    public struct Checkpoint
    {
        public string name;
        public Transform stopPoint;
        public Texture2D targetTexture;

        [Range(0, 100)]
        public float targetAccuracy;
    }

    [Header("Stage Settings")]
    public List<Checkpoint> checkpoints;
    public Transform goalPoint;
    public float moveSpeed = 2f;
    public float escapeSpeedMultiplier = 1.5f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Game State")]
    public float totalVisibility = 0f;
    private int currentCheckpointIdx = 0;
    private bool isGameOver = false;
    private bool isFollowingCamera = true;

    [Header("References")]
    public ChameleonCamouflageCalc painter;
    public Transform cameraTransform;
    public UIManager uiManager;
    public GirlController girl;

    [Header("Audio")]
    public CriAtomSource musicSource;
    public CriAtomSource sfxSource;

    [Header("Audio Settings")]
    public string transitionAisacName = "Checkpoint_Lowcut_Filter_Transition";
    public float transitionEffectDuration = 1.0f;

    private Vector3 cameraOffset;

    public Texture2D CurrentTargetTexture { get; private set; }
    public float CurrentTargetAccuracy { get; private set; } = 100f;
    public bool IsInJudgmentPhase { get; private set; } = false;

    void Start()
    {
        cameraOffset = cameraTransform.position - painter.transform.position;

        if (musicSource != null)
        {
            musicSource.Play("Music_Level_1");

            // Ensure filter starts OFF
            musicSource.SetAisacControl(transitionAisacName, 0.0f);

            Debug.Log("Music_Level_1 started");
        }
        else
        {
            Debug.LogError("MusicSource not assigned!");
        }

        StartCoroutine(MainGameLoop());
    }

    void LateUpdate()
    {
        if (isFollowingCamera)
        {
            Vector3 targetCamPos = painter.transform.position + cameraOffset;
            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                targetCamPos,
                Time.deltaTime * 5f
            );
        }
    }

    IEnumerator MainGameLoop()
    {
        while (currentCheckpointIdx < checkpoints.Count)
        {
            Checkpoint cp = checkpoints[currentCheckpointIdx];

            CurrentTargetTexture = cp.targetTexture;
            CurrentTargetAccuracy = cp.targetAccuracy;
            IsInJudgmentPhase = false;

            painter.SetAnimating(true);
            painter.SetPaintingEnabled(true);
            girl.SetState(GirlController.GirlState.Idle);

            yield return StartCoroutine(
                MoveToPoint(cp.stopPoint.position, moveSpeed)
            );

            painter.SetAnimating(false);
            painter.SetPaintingEnabled(false);
            girl.SetState(GirlController.GirlState.LookUp);
            IsInJudgmentPhase = true;

            yield return StartCoroutine(ProcessJudgment(cp));

            if (isGameOver)
                yield break;

            currentCheckpointIdx++;
        }

        isFollowingCamera = false;
        painter.SetPaintingEnabled(false);
        painter.SetAnimating(true);
        girl.SetState(GirlController.GirlState.Idle);

        yield return StartCoroutine(
            MoveToPoint(goalPoint.position, moveSpeed * escapeSpeedMultiplier)
        );

        uiManager.ShowResult(totalVisibility);
    }

    IEnumerator MoveToPoint(Vector3 targetPos, float speed)
    {
        Vector3 startPos = painter.transform.position;
        float distance = Vector3.Distance(startPos, targetPos);
        float elapsed = 0f;
        float duration = distance / speed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = easeCurve.Evaluate(elapsed / duration);
            painter.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        painter.transform.position = targetPos;
    }

    IEnumerator ProcessJudgment(Checkpoint cp)
    {
        yield return new WaitForSeconds(1.5f);

        float accuracy = painter.CalculateAccuracy(cp.targetTexture);
        float visibility = 100f - accuracy;
        totalVisibility += visibility;

        if (accuracy < 50f || totalVisibility > 100f)
        {
            isGameOver = true;

            string reason = accuracy < 50f
                ? "Too different from background!"
                : "Cumulative visibility exceeded 100!";

            uiManager.ShowGameOver(reason);

            if (sfxSource != null)
            {
                sfxSource.player.SetSelectorLabel("CHECKPOINT", "LOSE");
                sfxSource.Play("CHECKPOINT_FX");
            }

            yield break;
        }

        // WIN SFX
        if (sfxSource != null)
        {
            sfxSource.player.SetSelectorLabel("CHECKPOINT", "WIN");
            sfxSource.Play("CHECKPOINT_FX");
        }

        // ===== MUSIC TRANSITION EFFECT =====
        if (musicSource != null)
        {
            // Turn filter ON
            musicSource.SetAisacControl(transitionAisacName, 1.0f);

            // Trigger block switch
            musicSource.player.SetSelectorLabel("MUSIC_SWITCH", "ToBlockB");
            musicSource.Play("Music_Switch");

            Debug.Log("Music_Switch triggered + Filter ON");
        }

        // Wait during transition
        yield return new WaitForSeconds(transitionEffectDuration);

        // Turn filter OFF
        if (musicSource != null)
        {
            musicSource.SetAisacControl(transitionAisacName, 0.0f);
            Debug.Log("Filter OFF");
        }
    }
}

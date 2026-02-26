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
    public CriAtomSource ambientSource;
    public CriAtomSource sfxSource;
    public string ambientCueName = "Amb_Level01_Room";

    [Header("Fade In Settings")]
    public float fadeInDuration = 2.0f;
    public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float musicStartVolume = 1f;
    private float ambientStartVolume = 1f;

    [Header("Music Progression")]
    public List<string> progressionLabels;

    [Header("Health Bar Feedback")]
    public CriAtomSource healthBarSource;
    public string healthBarCueName = "Health_Bar";
    public string healthSelectorName = "HEALTH_BAR";
    public string healthBarAisacName = "Health_Bar_Pitch";
    public float directionThreshold = 0.002f;

    private float previousNormalized = -1f;
    private int lastDirection = 0;

    private Vector3 cameraOffset;

    public Texture2D CurrentTargetTexture { get; private set; }
    public float CurrentTargetAccuracy { get; private set; } = 100f;
    public bool IsInJudgmentPhase { get; private set; } = false;

    void Start()
    {
        cameraOffset = cameraTransform.position - painter.transform.position;

        if (musicSource != null)
        {
            musicStartVolume = musicSource.volume;
            musicSource.volume = 0f;
            musicSource.Play("Music_Level_1");
        }

        if (ambientSource != null)
        {
            ambientStartVolume = ambientSource.volume;
            ambientSource.volume = 0f;
            ambientSource.Play(ambientCueName);
        }

        StartCoroutine(FadeInAudio());
        StartCoroutine(MainGameLoop());
    }

    IEnumerator FadeInAudio()
    {
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = fadeInCurve.Evaluate(elapsed / fadeInDuration);

            if (musicSource != null)
                musicSource.volume = Mathf.Lerp(0f, musicStartVolume, t);

            if (ambientSource != null)
                ambientSource.volume = Mathf.Lerp(0f, ambientStartVolume, t);

            yield return null;
        }

        if (musicSource != null)
            musicSource.volume = musicStartVolume;

        if (ambientSource != null)
            ambientSource.volume = ambientStartVolume;
    }

    void Update()
    {
        if (painter == null) return;
        if (CurrentTargetTexture == null) return;
        if (healthBarSource == null) return;
        if (IsInJudgmentPhase) return;

        float accuracy = painter.CalculateAccuracy(CurrentTargetTexture);
        float normalized = Mathf.Clamp01(accuracy / 100f);

        healthBarSource.player.SetAisacControl(
            healthBarAisacName,
            normalized
        );

        if (previousNormalized < 0f)
        {
            previousNormalized = normalized;
            return;
        }

        float delta = normalized - previousNormalized;
        int currentDirection = 0;

        if (delta > directionThreshold)
            currentDirection = 1;
        else if (delta < -directionThreshold)
            currentDirection = -1;

        if (currentDirection != 0 && currentDirection != lastDirection)
        {
            if (currentDirection == 1)
                healthBarSource.player.SetSelectorLabel(healthSelectorName, "UP");
            else
                healthBarSource.player.SetSelectorLabel(healthSelectorName, "DOWN");

            healthBarSource.Play(healthBarCueName);
            lastDirection = currentDirection;
        }

        if (currentDirection == 0)
            lastDirection = 0;

        previousNormalized = normalized;
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

        TriggerFinalMusic(true);
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

            if (sfxSource != null)
            {
                sfxSource.player.SetSelectorLabel("CHECKPOINT", "LOSE");
                sfxSource.Play("CHECKPOINT_FX");
            }

            TriggerFinalMusic(false);
            uiManager.ShowGameOver("You Lose");
            yield break;
        }

        if (sfxSource != null)
        {
            sfxSource.player.SetSelectorLabel("CHECKPOINT", "WIN");
            sfxSource.Play("CHECKPOINT_FX");
        }

        if (musicSource != null && currentCheckpointIdx < progressionLabels.Count)
        {
            string nextLabel = progressionLabels[currentCheckpointIdx];
            musicSource.player.SetSelectorLabel("MUSIC_SWITCH", nextLabel);
            musicSource.Play("Music_Switch");
        }
    }

    void TriggerFinalMusic(bool playerWon)
    {
        if (musicSource == null) return;

        string finalLabel = playerWon ? "ToBlockWin" : "ToBlockLose";
        musicSource.player.SetSelectorLabel("MUSIC_SWITCH", finalLabel);
        musicSource.Play("Music_Switch");
    }
}
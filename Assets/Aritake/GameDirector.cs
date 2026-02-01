using CriWare;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class GameDirector : MonoBehaviour
{
    [System.Serializable]
    public struct Checkpoint
    {
        public string name;
        public Transform stopPoint;      // Position where the chameleon stops
        public Texture2D targetTexture;  // Correct reference image for this checkpoint

        [Range(0, 100)]
        public float targetAccuracy;     // Target score for this checkpoint
    }

    [Header("Stage Settings")]
    public List<Checkpoint> checkpoints;
    public Transform goalPoint; // Place this far off-screen (to the right)
    public float moveSpeed = 2f;
    public float escapeSpeedMultiplier = 1.5f; // Run slightly faster during escape
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Game State")]
    public float totalVisibility = 0f;
    private int currentCheckpointIdx = 0;
    private bool isGameOver = false;
    private bool isFollowingCamera = true; // Camera follow flag

    [Header("References")]
    public ChameleonCamouflageCalc painter;
    public Transform cameraTransform;
    public UIManager uiManager;

    [Header("Shared Scene Objects")]
    public GirlController girl; // Assign the single Girl instance placed in the scene via Inspector

    private Vector3 cameraOffset;
    private CriAtomSource atomSource;

    public Texture2D CurrentTargetTexture { get; private set; }
    public float CurrentTargetAccuracy { get; private set; } = 100f;
    public bool IsInJudgmentPhase { get; private set; } = false;

    void Start()
    {
        atomSource = GetComponent<CriAtomSource>();

        // Store the initial offset between the camera and the chameleon
        cameraOffset = cameraTransform.position - painter.transform.position;

        StartCoroutine(MainGameLoop());
    }

    void LateUpdate()
    {
        // The camera follows only when the flag is enabled
        if (isFollowingCamera)
        {
            Vector3 targetCamPos = painter.transform.position + cameraOffset;
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetCamPos, Time.deltaTime * 5f);
        }
    }

    IEnumerator MainGameLoop()
    {
        // 1. Loop through all checkpoints
        while (currentCheckpointIdx < checkpoints.Count)
        {
            Checkpoint cp = checkpoints[currentCheckpointIdx];

            CurrentTargetTexture = cp.targetTexture;
            CurrentTargetAccuracy = cp.targetAccuracy;
            IsInJudgmentPhase = false;

            // Movement: enable animation and move forward
            painter.SetAnimating(true);
            painter.SetPaintingEnabled(true);
            girl.SetState(GirlController.GirlState.Idle);
            yield return StartCoroutine(MoveToPoint(cp.stopPoint.position, moveSpeed));

            // Arrival: stop animation on the first frame and enter judgment phase
            painter.SetAnimating(false);
            painter.SetPaintingEnabled(false);
            girl.SetState(GirlController.GirlState.LookUp);
            IsInJudgmentPhase = true;
            yield return StartCoroutine(ProcessJudgment(cp));

            if (isGameOver) yield break;

            currentCheckpointIdx++;
        }

        // 2. Escape sequence (after passing the final checkpoint)
        Debug.Log("<color=lime>Escape Sequence Started!</color>");

        // Stop camera following
        isFollowingCamera = false;

        // Disable painting, enable animation (full sprint)
        painter.SetPaintingEnabled(false);
        painter.SetAnimating(true);
        girl.SetState(GirlController.GirlState.Idle);

        // Move toward the goal point (off-screen)
        // Since this is an escape, increasing speed and using linear movement
        // enhances the feeling of “getting away”
        easeCurve = new AnimationCurve(
            new Keyframe(1f, 1f, 2f, 0f),
            new Keyframe(0f, 0f, 0f, 0f)
        );

        yield return StartCoroutine(MoveToPoint(goalPoint.position, moveSpeed * escapeSpeedMultiplier));

        // 3. Show result
        Debug.Log($"<color=yellow>STAGE CLEAR!</color> Final Score (Visibility): {totalVisibility:F1}%");
        uiManager.ShowResult(totalVisibility);
    }

    // Movement coroutine (updated to allow speed specification)
    IEnumerator MoveToPoint(Vector3 targetPos, float speed)
    {
        Vector3 startPos = painter.transform.position;
        float distance = Vector3.Distance(startPos, targetPos);
        float elapsed = 0;

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
        Debug.Log($"<color=cyan>Checkpoint: {cp.name} - Human is looking!</color>");

        yield return new WaitForSeconds(1.5f); // Dramatic pause before judgment

        float accuracy = painter.CalculateAccuracy(cp.targetTexture);
        float visibility = 100f - accuracy;
        totalVisibility += visibility;

        Debug.Log($"Accuracy: {accuracy:F1}% | Visibility Added: {visibility:F1}% | Total: {totalVisibility:F1}%");

        if (accuracy < 50f || totalVisibility > 100f)
        {
            isGameOver = true;
            string reason = accuracy < 50f
                ? "Too different from background!"
                : "Cumulative visibility exceeded 100!";
            uiManager.ShowGameOver(reason);

            if (atomSource != null)
            {
                atomSource.player.SetSelectorLabel("CHECKPOINT", "LOSE");
                atomSource.Play("CHECKPOINT_FX");
            }
            yield break;
        }

        if (atomSource != null)
        {
            atomSource.player.SetSelectorLabel("CHECKPOINT", "WIN");
            atomSource.Play("CHECKPOINT_FX");
        }

        yield return new WaitForSeconds(1f);
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CriWare;

public class GameDirector : MonoBehaviour
{
    [System.Serializable]
    public struct Checkpoint
    {
        public string name;
        public Transform stopPoint;      // Position where the chameleon stops
        public Texture2D targetTexture;  // Correct reference image at this checkpoint
    }

    [Header("Stage Settings")]
    public List<Checkpoint> checkpoints;
    public Transform goalPoint;
    public float moveSpeed = 2f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Game State")]
    public float totalVisibility = 0f; // Accumulated �gvisibility�h (100 - Accuracy)
    private int currentCheckpointIdx = 0;
    private bool isGameOver = false;

    [Header("References")]
    public ChameleonCamouflageCalc painter;
    public Transform cameraTransform;

    private Vector3 cameraOffset;

    public UIManager uiManager;

    private CriAtomSource atomSource;

    void Start()
    {
        atomSource = GetComponent<CriAtomSource>();
        // Keep the distance to the 2D orthographic camera
        cameraOffset = cameraTransform.position - painter.transform.position;
        StartCoroutine(MainGameLoop());
    }

    void LateUpdate()
    {
        // Smoothly follow the chameleon�fs X position with the camera
        Vector3 targetCamPos = painter.transform.position + cameraOffset;
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetCamPos, Time.deltaTime * 5f);
    }

    IEnumerator MainGameLoop()
    {
        while (currentCheckpointIdx < checkpoints.Count)
        {
            Checkpoint cp = checkpoints[currentCheckpointIdx];

            // 1. Movement phase (move to the next checkpoint�fs stopPoint)
            painter.SetPaintingEnabled(true); // Allow painting while moving (time-limit element)
            yield return StartCoroutine(MoveToPoint(cp.stopPoint.position));

            // 2. Arrival & judgment phase
            painter.SetPaintingEnabled(false);
            yield return StartCoroutine(ProcessJudgment(cp));

            if (isGameOver) yield break;

            currentCheckpointIdx++;
        }

        // 3. Move to the goal
        yield return StartCoroutine(MoveToPoint(goalPoint.position));
        Debug.Log($"<color=yellow>STAGE CLEAR!</color> Final Score (Visibility): {totalVisibility:F1}%");
        uiManager.ShowResult(totalVisibility);
    }

    IEnumerator MoveToPoint(Vector3 targetPos)
    {
        Vector3 startPos = painter.transform.position;
        float distance = Vector3.Distance(startPos, targetPos);
        float elapsed = 0;

        // Duration based on distance (constant speed with easing curve applied)
        float duration = distance / moveSpeed;

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

        // Call the judgment method on the chameleon side
        float accuracy = painter.CalculateAccuracy(cp.targetTexture);
        float visibility = 100f - accuracy;
        totalVisibility += visibility;

        Debug.Log($"Accuracy: {accuracy:F1}% | Visibility Added: {visibility:F1}% | Total: {totalVisibility:F1}%");

        // Condition checks
        if (accuracy < 50f)
        {
            isGameOver = true;
            //uiManager.ShowGameOver("Too different from background!");
            atomSource.player.SetSelectorLabel("CHECKPOINT", "LOSE");
            atomSource.Play("CHECKPOINT_FX");
            yield break;
        }
        if (totalVisibility > 100f)
        {
            isGameOver = true;
            //uiManager.ShowGameOver("Cumulative visibility exceeded 100%!");
            atomSource.player.SetSelectorLabel("CHECKPOINT", "LOSE");
            atomSource.Play("CHECKPOINT_FX");
            yield break;
        }
        atomSource.player.SetSelectorLabel("CHECKPOINT", "WIN");
        atomSource.Play("CHECKPOINT_FX");
        yield return new WaitForSeconds(1f);
    }
}

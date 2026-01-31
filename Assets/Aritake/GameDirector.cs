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
        public Transform stopPoint;      // カメレオンが止まる位置
        public Texture2D targetTexture;  // このチェックポイントの正解画像
        public GameObject humanCharacter; // 振り返る人間

        [Range(0, 100)]
        public float targetAccuracy; // チェックポイントの目標スコア
    }

    [Header("Stage Settings")]
    public List<Checkpoint> checkpoints;
    public Transform goalPoint; // 画面外（右側遠く）に配置してください
    public float moveSpeed = 2f;
    public float escapeSpeedMultiplier = 1.5f; // 脱出時は少し速く走る
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Game State")]
    public float totalVisibility = 0f;
    private int currentCheckpointIdx = 0;
    private bool isGameOver = false;
    private bool isFollowingCamera = true; // カメラ追従フラグ

    [Header("References")]
    public ChameleonCamouflageCalc painter;
    public Transform cameraTransform;
    public UIManager uiManager;

    private Vector3 cameraOffset;
    private CriAtomSource atomSource;

    public Texture2D CurrentTargetTexture { get; private set; }
    public float CurrentTargetAccuracy { get; private set; } = 100f;
    public bool IsInJudgmentPhase { get; private set; } = false;

    void Start()
    {
        atomSource = GetComponent<CriAtomSource>();

        // カメラとカメレオンの初期オフセットを保持
        cameraOffset = cameraTransform.position - painter.transform.position;

        StartCoroutine(MainGameLoop());
    }

    void LateUpdate()
    {
        // フラグがONの時だけカメラが追従する
        if (isFollowingCamera)
        {
            Vector3 targetCamPos = painter.transform.position + cameraOffset;
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetCamPos, Time.deltaTime * 5f);
        }
    }

    IEnumerator MainGameLoop()
    {
        // 1. 各チェックポイントを巡るループ
        while (currentCheckpointIdx < checkpoints.Count)
        {
            Checkpoint cp = checkpoints[currentCheckpointIdx];

            CurrentTargetTexture = cp.targetTexture;
            CurrentTargetAccuracy = cp.targetAccuracy;
            IsInJudgmentPhase = false;

            // 移動：アニメーションをONにして進む
            painter.SetAnimating(true);
            painter.SetPaintingEnabled(true);
            yield return StartCoroutine(MoveToPoint(cp.stopPoint.position, moveSpeed));

            // 到着：アニメーションを1コマ目で止めて判定へ
            painter.SetAnimating(false);
            painter.SetPaintingEnabled(false);
            IsInJudgmentPhase = true;
            yield return StartCoroutine(ProcessJudgment(cp));

            if (isGameOver) yield break;

            currentCheckpointIdx++;
        }

        // 2. 脱出シーケンス（最後のチェックポイント通過後）
        Debug.Log("<color=lime>Escape Sequence Started!</color>");

        // カメラの追従を止める
        isFollowingCamera = false;

        // ペイント禁止、アニメーションON（全力疾走）
        painter.SetPaintingEnabled(false);
        painter.SetAnimating(true);

        // ゴール地点（画面外）へ向かって移動
        // 脱出なので速度を少し上げ、イーズなしの直線移動にすると「逃げ切る」感じが出ます
        yield return StartCoroutine(MoveToPoint(goalPoint.position, moveSpeed * escapeSpeedMultiplier));

        // 3. リザルト表示
        Debug.Log($"<color=yellow>STAGE CLEAR!</color> Final Score (Visibility): {totalVisibility:F1}%");
        uiManager.ShowResult(totalVisibility);
    }

    // 移動用コルーチン（速度指定を可能に変更）
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

        yield return new WaitForSeconds(1.5f); // 判定前のタメ

        float accuracy = painter.CalculateAccuracy(cp.targetTexture);
        float visibility = 100f - accuracy;
        totalVisibility += visibility;

        Debug.Log($"Accuracy: {accuracy:F1}% | Visibility Added: {visibility:F1}% | Total: {totalVisibility:F1}%");

        if (accuracy < 50f || totalVisibility > 100f)
        {
            isGameOver = true;
            string reason = accuracy < 50f ? "Too different from background!" : "Cumulative visibility exceeded 100%!";
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
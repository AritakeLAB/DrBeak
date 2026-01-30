using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [Min(0f)] public float countdownSeconds = 60f;
    public bool triggerOnce = true;

    bool used = false;

    void OnTriggerEnter(Collider other)
    {
        if (used && triggerOnce) return;
        if (!other.CompareTag("Player")) return;

        used = true;
        CheckpointTimeManager.Instance?.OnReachCheckpoint(this);
    }
}

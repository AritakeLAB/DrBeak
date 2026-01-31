using UnityEngine;

/// <summary>
/// Plays a CRI Atom UI sound (ex: button click)
/// Attach this to a GameObject that has a CriAtomSource.
/// </summary>
public class UIButtonSound : MonoBehaviour
{
    [Header("CRI Atom")]
    [SerializeField] private CriAtomSource atomSource;

    [Header("Cue Settings")]
    [SerializeField] private string cueName = "UI_Click";

    void Awake()
    {
        // Auto-find CriAtomSource if not assigned
        if (atomSource == null)
        {
            atomSource = GetComponent<CriAtomSource>();
        }

        if (atomSource == null)
        {
            Debug.LogError("[UIButtonSound] No CriAtomSource found!");
            return;
        }

        atomSource.cueName = cueName;
    }

    /// <summary>
    /// Call this from a Unity Button OnClick()
    /// </summary>
    public void PlayClick()
    {
        if (atomSource == null) return;

        atomSource.Play();
    }

    /// <summary>
    /// Optional: use for hover / select sounds later
    /// </summary>
    public void PlayCustom(string newCueName)
    {
        if (atomSource == null) return;

        atomSource.cueName = newCueName;
        atomSource.Play();
    }
}
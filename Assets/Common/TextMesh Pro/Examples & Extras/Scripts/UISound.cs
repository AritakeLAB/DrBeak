using UnityEngine;
using UnityEngine.UI;
using CriWare;

[RequireComponent(typeof(Button))]
public class CriUIButtonSound : MonoBehaviour
{
    [Header("CRI")]
    [SerializeField] private CriAtomSource atomSource;
    [SerializeField] private string cueName = "UI_MENU_CONFIRM";

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();

        if (atomSource == null)
            atomSource = GetComponent<CriAtomSource>();

        button.onClick.AddListener(PlayClick);
    }

    void PlayClick()
    {
        if (atomSource == null) return;

        atomSource.cueName = cueName;
        atomSource.Play();
    }
}
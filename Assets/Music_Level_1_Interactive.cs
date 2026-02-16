using UnityEngine;
using CriWare;

public class Music_Level_1_Interactive : MonoBehaviour
{
    private bool triggered = false;
    private CriAtomSource musicSwitchSource;

    void Start()
    {
        musicSwitchSource = GameObject.Find("Music_Switch")
            .GetComponent<CriAtomSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;

            // Set selector BEFORE playing
            musicSwitchSource.player.SetSelectorLabel(
                "MUSIC_SWITCH",
                "ToBlockB"
            );

            musicSwitchSource.Play();

            Debug.Log("Switched to Block B");
        }
    }
}

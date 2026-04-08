using UnityEngine;
using UnityEngine.SceneManagement;

public class InitAction : MonoBehaviour
{
    public int id = 0;
    void Start()
    {
    }

    private void LateUpdate()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        SceneManager.LoadScene(id);
    }
}

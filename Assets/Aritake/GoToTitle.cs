using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToTitle : MonoBehaviour
{
    public int id = 0;
    void Start()
    {
        SceneManager.LoadScene(id);
    }
}

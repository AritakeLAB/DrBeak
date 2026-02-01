using UnityEngine;
using UnityEngine.SceneManagement;
// 新しいInput Systemを使うための宣言を追加
using UnityEngine.InputSystem;

public class EscAction : MonoBehaviour
{
    public enum ActionType { JumpToScene1, QuitGame }
    public ActionType actionType;

    void Update()
    {
        // キーボードが存在し、かつEscキーが「このフレームで押された」か判定
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (actionType == ActionType.JumpToScene1)
            {
                SceneManager.LoadScene(1);
            }
            else if (actionType == ActionType.QuitGame)
            {
                Debug.Log("Quit Game");
                Application.Quit();
            }
        }
    }
}
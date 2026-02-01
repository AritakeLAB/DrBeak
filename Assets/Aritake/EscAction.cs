using UnityEngine;
using UnityEngine.SceneManagement;

public class EscAction : MonoBehaviour
{
    public enum ActionType { JumpToScene1, QuitGame }
    public ActionType actionType;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (actionType == ActionType.JumpToScene1)
            {
                // インデックス1のシーンへ（通常はMenuやTitle）
                SceneManager.LoadScene(1);
            }
            else if (actionType == ActionType.QuitGame)
            {
                // ゲームを終了（ビルド後のみ有効。エディタ上では無視される）
                Debug.Log("Quit Game");
                Application.Quit();
            }
        }
    }
}
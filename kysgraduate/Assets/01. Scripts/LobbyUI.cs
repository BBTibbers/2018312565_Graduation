using UnityEngine;
using UnityEngine.SceneManagement; // ← 씬 이동에 필요

public class LobbyUI : MonoBehaviour
{
    // 버튼에서 이 함수를 호출하면 됨
    public void GoToMainGame()
    {
        SceneManager.LoadScene("MainScene");
    }
}

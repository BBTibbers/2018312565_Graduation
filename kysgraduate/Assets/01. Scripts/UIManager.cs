using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _gameOverPanel;

    private void Start()
    {
        GridManager.Instance.OnFinished += ShowGameOver;
    }

    private void ShowGameOver()
    {
        _gameOverPanel.SetActive(true);
    }
}

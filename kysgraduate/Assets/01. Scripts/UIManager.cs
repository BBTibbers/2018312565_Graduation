using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _winPanel;

    [Header("SFX")]
    [SerializeField] private AudioClip gameOverSfx;
    [SerializeField] private AudioClip winSfx;
    [SerializeField] private float sfxVolume = 1f;

    private AudioSource _audio;
    private bool _isPaused; // 중복 Pause 방지

    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.spatialBlend = 0f; // 2D 사운드 (UI용)
    }

    private void Start()
    {
        GameManager.Instance.OnLose += ShowGameOver;
        GameManager.Instance.OnWin += ShowWin;
    }

    private void ShowGameOver()
    {
        _gameOverPanel.SetActive(true);
        if (gameOverSfx) ;// _audio.PlayOneShot(gameOverSfx, sfxVolume);
        PauseGame();
    }

    private void ShowWin()
    {
        _winPanel.SetActive(true);
        if (winSfx) _audio.PlayOneShot(winSfx, sfxVolume);
        PauseGame();
    }

    private void PauseGame()
    {
        if (_isPaused) return;
        _isPaused = true;

        // 게임 로직/물리/애니메이션(Scaled) 정지, UI 입력은 그대로 동작
        Time.timeScale = 0f;

        // (선택) UI 애니메이션이 있다면 Animator.updateMode = UnscaledTime 로 설정해줘
        // (선택) DOTween 사용 시 SetUpdate(true)로 Unscaled 업데이트 권장
    }

    public void GoToLobby()
    {
        // 씬 이동 전 timeScale 원복 필수
        Time.timeScale = 1f;
        _isPaused = false;
        SceneManager.LoadScene("Lobby");
    }

    private void OnDisable()
    {
        // 혹시 모를 잔여 정지 상태 방지 (씬 전환/오브젝트 파괴 시)
        if (_isPaused)
        {
            Time.timeScale = 1f;
            _isPaused = false;
        }
    }
}

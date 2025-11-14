using UnityEngine;

public class CellClickHandler : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera _cam; // 비워두면 자동 MainCamera
    [SerializeField] private LayerMask _cellLayer = ~0;

    [Header("SFX")]
    [SerializeField] private AudioClip clickSfx;
    [SerializeField] private float volume = 1f;
    private AudioSource _audio;

    private void Awake()
    {
        if (_cam == null) _cam = Camera.main;

        // 자동으로 AudioSource 붙여줌 (중복 방지)
        _audio = GetComponent<AudioSource>();
        if (_audio == null)
            _audio = gameObject.AddComponent<AudioSource>();

        _audio.playOnAwake = false;
        _audio.spatialBlend = 0f; // UI 클릭 느낌 → 2D 사운드
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            PlayClickSound();
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 1000f, _cellLayer))
            {
                var cell = hit.collider.GetComponent<Cell>();
                if (cell != null)
                {
                    cell.ToggleFlag();
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            PlayClickSound();
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 1000f, _cellLayer))
            {
                var cell = hit.collider.GetComponent<Cell>();
                if (cell != null) cell.Open();
            }
        }
    }

    private void PlayClickSound()
    {
        if (clickSfx != null)
            _audio.PlayOneShot(clickSfx, volume);
    }
}

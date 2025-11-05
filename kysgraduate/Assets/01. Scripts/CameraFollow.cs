using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private PlayerMove _player;
    private Vector3 _topPosition = new Vector3(0f, 10f, -5f);

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_player == null)
            _player = FindFirstObjectByType<PlayerMove>();

        transform.position = _player.transform.position + _topPosition;
    }
}

using UnityEngine;

/// <summary>
/// 정해진 구간을 반복해서 이동하는 플랫폼 스크립트입니다.
/// 플레이어가 플랫폼 위에 올라타면 함께 이동합니다.
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform[] waypoints; // 이동 경로 지점들
    [SerializeField] private float speed = 3.0f;    // 이동 속도
    [SerializeField] private float waitTime = 1.0f; // 도착 시 대기 시간
    [SerializeField] private bool loop = true;      // 루프 여부

    private int _currentIndex = 0;
    private float _waitTimer = 0f;
    private bool _isWaiting = false;
    private bool _isMovingForward = true;

    private Vector3 _previousPosition;
    private Vector3 _movementDelta;

    private void Start()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
            _previousPosition = transform.position;
        }
    }

    private void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        HandleMovement();
    }

    private void HandleMovement()
    {
        if (_isWaiting)
        {
            _waitTimer += Time.fixedDeltaTime;
            if (_waitTimer >= waitTime)
            {
                _isWaiting = false;
                _waitTimer = 0f;
                UpdateNextWaypoint();
            }
            _movementDelta = Vector3.zero;
            return;
        }

        Vector3 targetPosition = waypoints[_currentIndex].position;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.fixedDeltaTime);

        _movementDelta = transform.position - _previousPosition;
        _previousPosition = transform.position;

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            _isWaiting = true;
        }
    }

    private void UpdateNextWaypoint()
    {
        if (loop)
        {
            _currentIndex = (_currentIndex + 1) % waypoints.Length;
        }
        else
        {
            if (_isMovingForward)
            {
                _currentIndex++;
                if (_currentIndex >= waypoints.Length)
                {
                    _currentIndex = waypoints.Length - 2;
                    _isMovingForward = false;
                }
            }
            else
            {
                _currentIndex--;
                if (_currentIndex < 0)
                {
                    _currentIndex = 1;
                    _isMovingForward = true;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어를 플랫폼의 자식으로 설정하여 함께 이동하게 함
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어가 플랫폼에서 벗어나면 부모 관계 해제
            // null로 설정하면 최상위로 가므로, 원래 플레이어의 부모가 있었다면 그에 맞춰 수정이 필요할 수 있음
            other.transform.SetParent(null);
            
            // 만약 플레이어가 씬 로딩 시 특정 오브젝트 아래에 생성된다면 아래와 같이 처리 가능
            // GameObject playerRoot = GameObject.Find("PlayerRoot");
            // if(playerRoot != null) other.transform.SetParent(playerRoot.transform);
        }
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Gizmos.DrawSphere(waypoints[i].position, 0.3f);
            
            if (i < waypoints.Length - 1)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
            else if (loop)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
            }
        }
    }
}

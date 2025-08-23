using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BH_Lib.AssetManager;
using BH_Lib.DI;
using UnityEngine;

/// <summary>
/// 플레이어의 입력을 받아 캐릭터를 제어하는 컨트롤러 (InputSystem 사용)
/// AssetManager를 통해 InputReader를 비동기로 로딩
/// </summary>

[Register(typeof(PlayerController),LifetimeScope.Transient)]
public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private string _inputReaderKey = "InputReader";
    [Inject] private AssetManager _assetManager { get; set; }        

    private InputReader _inputReader;
    private IMovable _movable;
    private IAttacker _attacker;

    private Vector2 _moveDirection;
    private bool _isInputReaderLoaded = false;

    private async void Awake()
    {
        _movable = GetComponent<IMovable>();
        _attacker = GetComponent<IAttacker>();

        await LoadInputReaderAsync();
    }

    private async Task LoadInputReaderAsync()
    {
        _inputReader = await _assetManager.LoadAssetAsync<InputReader>(_inputReaderKey, gameObject);
        
        if (_inputReader != null)
        {
            _isInputReaderLoaded = true;
            SubscribeToInputEvents();
            Debug.Log("InputReader 비동기 로딩 완료");
        }
        else
        {
            Debug.LogError($"InputReader 로딩 실패: {_inputReaderKey}");
        }
    }


    private void OnEnable()
    {
        if (_isInputReaderLoaded)
        {
            SubscribeToInputEvents();
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromInputEvents();
    }

    private void Update()
    {
        // InputReader가 로딩된 후에만 이동 처리
        if (_isInputReaderLoaded && _movable != null)
        {
            // 매 프레임 이동 방향으로 움직임
            // 입력 값은 이벤트 핸들러에서 갱신
            Vector3 move = new Vector3(_moveDirection.x, 0, _moveDirection.y);
            _movable.Move(move);
        }
    }

    private void SubscribeToInputEvents()
    {
        if (_inputReader != null)
        {
            _inputReader.MoveEvent += HandleMove;
            _inputReader.AttackEvent += HandleAttack;
        }
    }

    private void UnsubscribeFromInputEvents()
    {
        if (_inputReader != null)
        {
            _inputReader.MoveEvent -= HandleMove;
            _inputReader.AttackEvent -= HandleAttack;
        }
    }

    private void HandleMove(Vector2 direction)
    {
        _moveDirection = direction;
    }

    private void HandleAttack()
    {
        // TODO: 공격 로직 구현
        // 예시: 타겟 탐지 후 공격
        Debug.Log("플레이어 공격 입력 감지");
    }

    private void OnDestroy()
    {
        // 컴포넌트 파괴 시 이벤트 구독 해제
        UnsubscribeFromInputEvents();
    }
}


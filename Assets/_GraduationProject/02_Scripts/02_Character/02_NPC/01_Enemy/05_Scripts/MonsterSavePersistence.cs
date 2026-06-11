using UnityEngine;

/// <summary>
/// 몬스터의 사망 상태를 저장하고 리스폰을 관리하는 새로운 독립 컴포넌트입니다.
/// 기존 Enemy 관련 코드를 수정하지 않고 기능을 확장합니다.
/// </summary>
public class MonsterSavePersistence : MonoBehaviour
{
    [Header("Save Settings")]
    [SerializeField] private string _monsterId;
    public string MonsterId => _monsterId;

    private Enemy _enemy;
    private EnemyHealth _health;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _health = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(_monsterId))
        {
            Debug.LogError($"[MonsterSavePersistence] {gameObject.name}의 MonsterId가 설정되지 않았습니다! ID가 없으면 저장이 불가능합니다.");
            return;
        }

        // 데이터 매니저를 통해 이미 죽은 상태인지 확인
        if (DataManager.Instance != null && DataManager.Instance.GetGameData() != null)
        {
            bool isDead = DataManager.Instance.GetGameData().IsMonsterDead(_monsterId);
            if (isDead)
            {
                gameObject.SetActive(false);
                return;
            }
        }

        // 사망 이벤트 구독 (실행 순서 문제 방지를 위해 직접 참조 사용)
        if (_health != null)
        {
            _health.OnDied -= HandleMonsterDied; // 중복 구독 방지
            _health.OnDied += HandleMonsterDied;
        }
        else
        {
            Debug.LogError($"[MonsterSavePersistence] {gameObject.name}에서 EnemyHealth를 찾을 수 없습니다!");
        }
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (_health != null)
        {
            _health.OnDied -= HandleMonsterDied;
        }
    }

    /// <summary>
    /// 몬스터 사망 시 호출되어 DataManager에 상태를 기록합니다.
    /// </summary>
    private void HandleMonsterDied()
    {
        if (!string.IsNullOrEmpty(_monsterId) && DataManager.Instance != null && DataManager.Instance.GetGameData() != null)
        {
            DataManager.Instance.GetGameData().AddDeadMonster(_monsterId);
            Debug.Log($"<color=red>[MonsterSavePersistence] 몬스터 사망 기록됨: {_monsterId}</color>");
            
            // 몬스터 사망 시 즉시 세이브를 원한다면 아래 주석을 해제하세요.
            // DataManager.Instance.SaveGame();
        }
    }

    /// <summary>
    /// 인스펙터 또는 매니저를 통해 고유 ID를 부여합니다.
    /// 가독성을 위해 오브젝트 이름을 접두어로 포함합니다.
    /// </summary>
    [ContextMenu("Generate Unique ID")]
    public void SetRandomID()
    {
        _monsterId = $"{gameObject.name}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    // 기존 메서드와의 호환성을 위한 래퍼
    private void GenerateId() => SetRandomID();
}

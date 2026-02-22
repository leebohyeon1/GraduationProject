using UnityEngine;

/// <summary>
/// ddokddoki 엔티티에 부착하는 AI 컨트롤러 컴포넌트.
/// DdokddokiBrain을 생성하고 매 프레임 Tick을 호출합니다.
/// </summary>
public class DdokddokiController : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;

    public DdokddokiBrain Brain { get; private set; }

    private void Start()
    {
        if (_playerTransform == null)
        {
            Debug.LogWarning("[DdokddokiController] Player Transform이 설정되지 않았습니다.");
        }

        Brain = new DdokddokiBrain(this, transform, _playerTransform, transform.position);
        Brain.Initialize();
    }

    private void Update()
    {
        Brain?.Tick(Time.deltaTime);
    }

    private void OnDestroy()
    {
        Brain?.Cleanup();
    }

    /// <summary>외부에서 전투 상태를 시작합니다.</summary>
    public void EnterCombat(bool combat = true)
    {
        Brain?.EnterCombat(combat);
    }

    /// <summary>외부에서 스턴 상태를 설정합니다.</summary>
    public void SetStunned(bool stunned)
    {
        Brain?.SetStunned(stunned);
    }
}

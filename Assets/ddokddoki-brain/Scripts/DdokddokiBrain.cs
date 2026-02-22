using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ddokddoki 엔티티의 AI 브레인 클래스.
/// BlackBoard를 통해 상태를 관리하고 시야 감지 및 스킬 쿨타임을 처리합니다.
/// </summary>
public class DdokddokiBrain
{
    public BlackBoard blackboard { get; private set; }

    private MonoBehaviour _owner;
    private Transform _ownerTransform;
    private Transform _playerTransform;

    private Dictionary<string, float> _lastUsedSkillTimes = new Dictionary<string, float>();

    private Coroutine _tickCoroutine;

    /// <summary>전투 중 여부</summary>
    public bool IsCombat { get; private set; } = false;

    /// <summary>스턴 여부</summary>
    public bool IsStunned { get; private set; } = false;

    /// <param name="owner">이 브레인을 소유한 MonoBehaviour (코루틴 실행용)</param>
    /// <param name="ownerTransform">소유자의 Transform</param>
    /// <param name="playerTransform">플레이어의 Transform</param>
    /// <param name="homePosition">소유자의 시작 위치</param>
    public DdokddokiBrain(MonoBehaviour owner, Transform ownerTransform, Transform playerTransform, Vector3 homePosition)
    {
        _owner = owner;
        _ownerTransform = ownerTransform;
        _playerTransform = playerTransform;

        blackboard = new BlackBoard();
        blackboard.SetValue(DdokddokiBlackboardKeys.HomePosition.ToKey(), homePosition);
        blackboard.SetValue(DdokddokiBlackboardKeys.Self.ToKey(), owner.gameObject);
    }

    /// <summary>
    /// 브레인을 초기화하고 백그라운드 Tick 코루틴을 시작합니다.
    /// DdokddokiController.Start()에서 호출하세요.
    /// </summary>
    public void Initialize()
    {
        _tickCoroutine = _owner.StartCoroutine(TickCoroutine());
    }

    /// <summary>코루틴을 중지하고 브레인 리소스를 해제합니다.</summary>
    public void Cleanup()
    {
        if (_tickCoroutine != null)
        {
            _owner.StopCoroutine(_tickCoroutine);
            _tickCoroutine = null;
        }
    }

    /// <summary>매 프레임 호출. 필요 시 추가 로직을 여기서 처리합니다.</summary>
    public void Tick(float deltaTime)
    {
    }

    private IEnumerator TickCoroutine()
    {
        while (true)
        {
            if (_playerTransform != null)
            {
                UpdateVision();
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void UpdateVision()
    {
        float distance = Vector3.Distance(_ownerTransform.position, _playerTransform.position);
        blackboard.SetValue(DdokddokiBlackboardKeys.DistanceBetween.ToKey(), distance);

        bool playerLooking = IsPlayerLookingAtMe();
        blackboard.SetValue(DdokddokiBlackboardKeys.OnPlayerLooking.ToKey(), playerLooking);

        blackboard.SetValue(DdokddokiBlackboardKeys.LastPlayerPos.ToKey(), _playerTransform.position);
    }

    private bool IsPlayerLookingAtMe()
    {
        Vector3 toSelf = _ownerTransform.position - _playerTransform.position;
        return Vector3.Angle(_playerTransform.forward, toSelf.normalized) <= 35f;
    }

    #region Combat

    public void EnterCombat(bool combat = true)
    {
        IsCombat = combat;
        blackboard.SetValue(DdokddokiBlackboardKeys.IsPlayerDetected.ToKey(), IsCombat);
    }

    public void SetStunned(bool stunned)
    {
        IsStunned = stunned;
    }

    #endregion

    #region Skill Cooldown

    public bool IsSkillReady(string skillName, float cooldownDuration)
    {
        if (_lastUsedSkillTimes.TryGetValue(skillName, out float lastUsedTime))
        {
            return Time.time >= lastUsedTime + cooldownDuration;
        }
        return true;
    }

    public void StartSkillCooldown(string skillName)
    {
        _lastUsedSkillTimes[skillName] = Time.time;
    }

    public float GetLastSkillUseTime(string skillName)
    {
        if (_lastUsedSkillTimes.TryGetValue(skillName, out float time))
        {
            return time;
        }
        return -1f;
    }

    #endregion
}

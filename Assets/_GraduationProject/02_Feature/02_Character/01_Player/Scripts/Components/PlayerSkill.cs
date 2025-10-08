using DG.Tweening;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public enum SkillType
{
    None = -1,
    Flash = 0,
    Boost = 1,
    TimeStop = 2
}

public class PlayerSkill : MonoBehaviour
{
    #region Private Fields
    private PlayerStats _stats;
    private PlayerEvents _events;
    private PlayerInputHandler _inputHandler;

    private FlashSkillSO _flashSkillSO;
    private BoostSkillSO _boostSkillSO;
    private TimeStopSkillSO _timeStopSkillSO;

    [SerializeField] private SkillType _currentSkillType;

    #endregion

    public void Initialize(PlayerStats stats, PlayerEvents events,
        PlayerInputHandler inputHandler, PlayerDataBaseSO dataBaseSO)
    {
        _stats = stats;
        _events = events;
        _inputHandler = inputHandler;

        _flashSkillSO = dataBaseSO.FlashSkill;
        _boostSkillSO = dataBaseSO.BoostSkill;
        _timeStopSkillSO = dataBaseSO.TimeStopSkill;
    }

    public void Tick()
    {
        for (int i = 0; i < _stats.SkillData.IsMainSkillsUnlock.Count; i++)
        {
            if (_stats.SkillData.SkillCount[i] >= _stats.SkillData.SkillMaxCount[i])
            {
                continue;
            }

            _stats.SkillData.SkillCoolDownTimer[i] += Time.deltaTime;

            if (_stats.SkillData.SkillCoolDownTimer[i] >= _stats.SkillData.SkillCoolDown[i])
            {
                _stats.SkillData.SkillCount[i] = 
                    Mathf.Clamp(_stats.SkillData.SkillCount[i] + 1, 0, _stats.SkillData.SkillMaxCount[i]);

                _stats.SkillData.SkillCoolDownTimer[i] = 0f;
            }
        }
    }

    /// <summary>
    /// ��ų Ÿ���� �����մϴ�
    /// </summary>
    /// <param name="skillType">��ų Ÿ��</param>
    public void SetSkill(SkillType skillType)
    {
        if (skillType == _currentSkillType
            && _stats.SkillData.IsMainSkillsUnlock[(int)skillType])
        {
            return;
        }

        _currentSkillType = skillType;
    }

    /// <summary>
    /// ��ų Ÿ�Կ� ���� ��ų�� ����մϴ�.
    /// </summary>
    public void UseSkill()
    {
        if (_currentSkillType == SkillType.None
            || _stats.SkillData.SkillCount[(int)_currentSkillType] <= 0)
        {
            return;
        }

        switch (_currentSkillType)
        {
            case SkillType.Flash:
                Flash();
                break;

            case SkillType.Boost:
                Boots();
                break;

            case SkillType.TimeStop:
                break;
        }

        _stats.SkillData.SkillCount[(int)_currentSkillType]--;
    }

    private void Flash()
    {
        if(_stats.CurrentMana < _flashSkillSO.SkillCost)
        {
            return;
        }

        float distance = _flashSkillSO.MoveDistance;
        if (Physics.Raycast(transform.position, _inputHandler.MoveInput, out RaycastHit hitInfo,
            _flashSkillSO.MoveDistance, _stats.ObstacleLayerMask))
        {
            distance = hitInfo.distance - (GetComponent<Collider>().bounds.size.z / 2);
        }

        Vector2 input = _inputHandler.MoveInput * distance;
        _events.TriggerFlashSkillStart(input);
    }

    private void Boots()
    {
        _stats.IsBoost = true;

        Sequence sequence = DOTween.Sequence();
        sequence.SetDelay(_boostSkillSO.Duration);
        sequence.AppendCallback(() => { _stats.IsBoost = false; });
        
    }
}

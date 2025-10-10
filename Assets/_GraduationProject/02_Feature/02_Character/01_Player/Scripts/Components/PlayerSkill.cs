using BH_Lib.Log;
using DG.Tweening;
using System;
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

public class PlayerSkill : MonoBehaviour, IDisposable
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

    #region Properties
    public PlayerSkillData SkillData => _stats.SkillData;
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

        _events.OnFlashFinish += HandleFlashFinsh;
    }

    public void Dispose()
    {
        _events.OnFlashFinish -= HandleFlashFinsh;
    }

    public void Tick()
    {
        for (int i = 0; i < SkillData.IsMainSkillsUnlock.Count; i++)
        {
            if (SkillData.SkillCount[i] >= SkillData.SkillMaxCount[i])
            {
                continue;
            }

            SkillData.SkillCoolDownTimer[i] += Time.deltaTime;

            if (SkillData.SkillCoolDownTimer[i] >= SkillData.SkillCoolDown[i])
            {
                SkillData.SkillCount[i] = 
                    Mathf.Clamp(SkillData.SkillCount[i] + 1, 0, SkillData.SkillMaxCount[i]);

                SkillData.SkillCoolDownTimer[i] = 0f;
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
            && SkillData.IsMainSkillsUnlock[(int)skillType])
        {
            return;
        }

        _currentSkillType = skillType;
    }
    #region Enchant
    public void EnchantSkill(SkillType skillType, int level = -1)
    {
        Log.Print(1);
        int skillTypeindex = (int)skillType;

        if (level > -1)
        {
            SkillData.IsSubSkillsUnlock[skillTypeindex][level] = false;

            switch (skillType)
            {
                case SkillType.Flash:
                    EnchantFlash(level);
                    break;
                case SkillType.Boost: 
                    EnchantBoost(level);
                    break;
            }

            return;
        }

        for (int i = 0; i < SkillData.IsSubSkillsUnlock[skillTypeindex].Count; i++)
        {
            if (SkillData.IsSubSkillsUnlock[skillTypeindex][i])
            {
                continue;
            }

            SkillData.IsSubSkillsUnlock[skillTypeindex][i] = false;

            switch (skillType)
            {
                case SkillType.Flash:
                    EnchantFlash(i);
                    break;
                case SkillType.Boost:
                    EnchantBoost(level);
                    break;

            }
        }
    }

    public void EnchantFlash(int level)
    {
        if (level == 0)
        {
            SkillData.SkillCoolDown[0] -= _flashSkillSO.DecreaseCoolDownAmount;
        }
        else if (level == 1)
        {
            SkillData.SkillMaxCount[0] += _flashSkillSO.IncreaseCountAmount;
        }
        else if (level == 2)
        {
            SkillData.SetMaxLevelFlash(true);
        }
    }

    public void EnchantBoost(int level)
    {
        if (level == 0)
        {
            SkillData.SetBoostRangeMultiply(_boostSkillSO.IncreaseAttackRangeAmount);
        }
        else if(level == 1)
        {
            SkillData.SetBoostDamageMultiply(_boostSkillSO.IncreaseAttackDamageAmount);
        }
        else if (level == 2)
        {
            SkillData.SetMaxLevelBoost(true);
        }
    }
    #endregion

    /// <summary>
    /// ��ų Ÿ�Կ� ���� ��ų�� ����մϴ�.
    /// </summary>
    public void UseSkill()
    {
        if (_currentSkillType == SkillType.None
            || SkillData.SkillCount[(int)_currentSkillType] <= 0)
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

        SkillData.SkillCount[(int)_currentSkillType]--;
    }

    #region Flash
    private void Flash()
    {
        if (_stats.CurrentMana < _flashSkillSO.SkillCost)
        {
            return;
        }

        float distance = _flashSkillSO.MoveDistance;
        Vector3 moveDirection;

        if (_inputHandler.MoveInput == Vector2.zero)
        {
            moveDirection = transform.forward;
            Log.Print(moveDirection);
        }
        else
        {
            moveDirection = new Vector3(_inputHandler.MoveInput.x, 0, _inputHandler.MoveInput.y).normalized;
        }

        if (Physics.Raycast(transform.position, moveDirection, out RaycastHit hitInfo,
            _flashSkillSO.MoveDistance, _stats.ObstacleLayerMask))
        {
            distance = hitInfo.distance - (GetComponent<Collider>().bounds.size.z / 2);
        }

        Vector2 velocity = new Vector3(_inputHandler.MoveInput.x, 0, _inputHandler.MoveInput.y).normalized;
        _events.TriggerFlashSkillStart(velocity, distance);
    }

    private void PerformFlashDamage(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, _flashSkillSO.FlashAttackRadius);
        foreach (Collider collider in colliders)
        {
            if(collider.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(_flashSkillSO.FlashDamage);
            }
        }
    }
    #endregion

    #region Boosts
    private void Boots()
    {
        _stats.IsBoost = true;
        _events.TriggerBoostSkillStart();

        Sequence sequence = DOTween.Sequence();
        sequence.SetDelay(_boostSkillSO.Duration);
        sequence.AppendCallback(() => { _stats.IsBoost = false; });
        
    }
    #endregion

    #region eventHandler
    private void HandleFlashFinsh(Vector3 position)
    {
        if(SkillData.IsMaxLevelFlash)
        {
            PerformFlashDamage(position);
        }
    }

    #endregion
}

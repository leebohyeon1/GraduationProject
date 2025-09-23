using BH_Lib.Log;
using MoreMountains.Feedbacks;
using refactor;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerFeedbackType
{
    Move_FB, MoveStop_FB, DodgeStart_FB, 
    DodgeFinish_FB, Landing_FB,

    TakeDamage_Nomal_FB,
    TakeDamage_Strong_FB,
    TakeDamage_Defend_FB,

    FirstAttackStart_FB, SecondAttackStart_FB,
    ThirdAttackStart_FB, MeleeAttackHit_FB,

    ChargeStart_FB, ChargeCancel_FB,
    ChargeFinish_FB, ChargeAttackStart_FB, 
    ChargeAttackFinish_FB, Tier1ChargeAttackHit_FB,
    Tier2ChargeAttackHit_FB, Tier3ChargeAttackHit_FB,

    RangeAttackChargeStart_FB, RangeAttackCharging_FB,
    RangeAttackChargeCancel_FB, RangeAttackChargeFinish_FB,
    RangeAttackStart_FB, RangeAttackHit_FB,

    ParryStart_FB, ParrySuccess_FB, CounterAttackStart_FB, 
    Tier1CounterAttackFirstHit_FB, Tier2CounterAttackFirstHit_FB, Tier3CounterAttackFirstHit_FB,
    Tier1CounterAttackSecondHit_FB, Tier2CounterAttackSecondHit_FB, Tier3CounterAttackSecondHit_FB,
    CounterAttackFinish_FB,

    Tier1Up_FB, Tier2Up_FB, Tier3Up_FB,
    Tier1Down_FB, Tier2Down_FB, Tier3Down_FB,

    OverHeatStart_FB, OverHeatFinish_FB,

    Tier1_FB, Tier2_FB, Tier3_FB, OverHeat_FB
}

public class PlayerEvents : MonoBehaviour
{
    [SerializeField] private FeedbackPlayer<PlayerFeedbackType> _feedbackPlayer;

    public FeedbackPlayer<PlayerFeedbackType> Feedback => _feedbackPlayer;

    #region EffectPoint
    /// <summary>
    /// �ٰŸ� ���� ���� ���� ��ġ
    /// </summary>
    [SerializeField] private Transform _firstAttackStartEffectPoint;
    [SerializeField] private Transform _secondAttackStartEffectPoint;
    [SerializeField] private Transform _thirdAttackStartEffectPoint;

    /// <summary>
    /// ��¡ ���� ���� ��ġ
    /// </summary>
    [SerializeField] private Transform _chargeEffectPoint;

    /// <summary>
    /// ���Ÿ� ���� ���� ��ġ
    /// </summary>
    [SerializeField] private Transform _rangedAttackPoint;
    #endregion

    #region Events
    public event Action<bool> OnBattleStateChaged;

    public event Action OnDodgeFinish;

    public event Action OnAttackPerform;
    public event Action<Collider> OnAttackAffect;
    public event Action OnAttackFinish;

    public event Action<Collider> OnChargeAttackAffect;

    public event Action<Transform> OnRangedAttackStart;
    public event Action<Collider> OnRangedAttackAffect;
    public event Action OnRangedAttackFinish;

    public event Action OnParryPerform;
    public event Action<Collider> OnParryAffect;

    public event Action OnTier1Up, OnTier2Up, OnTier3Up, OnOverHeatStart;
    public event Action OnTier1Down, OnTier2Down, OnTier3Down, OnOverHeatFinish;
    #endregion



    private void Start()
    {
        _feedbackPlayer.Initialize();
    }

    #region EventHandler
    /// <summary>
    /// ���� ���°� �ٲ� �� ȣ��
    /// </summary>
    /// <param name="isbattleState">���� ���� ����</param>
    public void TriggerBattleStateChanged(bool isbattleState)
    {
        OnBattleStateChaged.Invoke(isbattleState);
    }


    /// <summary>
    /// �̵� �� ���� ���� ���̴� ���� ������ ȿ��
    /// (�ִϸ��̼� Ʈ����) 
    /// </summary>
    public void TriggerFootstep()
    {
        _feedbackPlayer.PlayFeedback(PlayerFeedbackType.Move_FB, transform.position);
    }


    /// <summary>
    /// ������ ���� �� ������ ȿ��
    /// (�ִϸ��̼� Ʈ����)
    /// </summary>
    public void TriggerDodgeFinish()
    {
        OnDodgeFinish.Invoke();
        _feedbackPlayer.PlayFeedback(PlayerFeedbackType.DodgeFinish_FB, transform.position);
    }


    /// <summary>
    /// ù ��° ���� ���� ���� �� ������ ȿ��
    /// </summary>
    public void TriggerFirstAttackStart()
    {
        if (_firstAttackStartEffectPoint != null)
        {
            Log.PrintWarning("ù ��° ���� ��ġ ����");
            _feedbackPlayer.PlayFeedback(PlayerFeedbackType.FirstAttackStart_FB, _firstAttackStartEffectPoint.position);
        }
    }
    /// <summary>
    /// �� ��° ���� ���� ���� �� ������ ȿ��
    /// </summary>
    public void TriggerSecondAttackStart()
    {
        if (_secondAttackStartEffectPoint != null)
        {
            Log.PrintWarning("�� ��° ���� ��ġ ����");
            _feedbackPlayer.PlayFeedback(PlayerFeedbackType.SecondAttackStart_FB, transform.position);
        }
    }
    /// <summary>
    /// �� ��° ���� ���� ���� �� ������ ȿ��
    /// </summary>
    public void TriggerThirdAttackStart()
    {
        if (_thirdAttackStartEffectPoint != null)
        {
            Log.PrintWarning("�� ��° ���� ��ġ ����");
            _feedbackPlayer.PlayFeedback(PlayerFeedbackType.ThirdAttackStart_FB, _thirdAttackStartEffectPoint.position);
        }
    }
    /// <summary>
    /// ���� ���� �� ȿ��
    /// (�ִϸ��̼� Ʈ����)
    /// </summary>
    public void TriggerAttackPerform()
    {
        OnAttackPerform.Invoke();
    }
    /// <summary>
    /// ���� �������� ���� �ǰ� ȿ��
    /// </summary>
    /// <param name="collider">�浹�� ������Ʈ</param>
    public void TriggerAttackAffect(Collider collider)
    {
        OnAttackAffect.Invoke(collider);
        _feedbackPlayer.PlayFeedback(PlayerFeedbackType.MeleeAttackHit_FB, collider.transform.position);
    }
    /// <summary>
    /// ���� ���� ���� �� ȿ��
    /// (�ִϸ��̼� Ʈ����)
    /// </summary>
    public void TriggerAttackFinish()
    {
        OnAttackFinish.Invoke();
    }


    /// <summary>
    /// ��¡ ���� �� ȿ��
    /// </summary>
    public void TriggerChargeStart()
    {
        if(_chargeEffectPoint != null)
        {
            Log.PrintWarning("��¡ ����Ʈ ��ġ ����");
            _feedbackPlayer.PlayFeedback(PlayerFeedbackType.ChargeStart_FB, _chargeEffectPoint.position);
        }  
    }
    /// <summary>
    /// �ּ� ��¡ �Ϸ� �� ȿ�� 
    /// </summary>
    public void TriggerChargeFinish()
    {
        if (_chargeEffectPoint != null)
        {
            Log.PrintWarning("��¡ ����Ʈ ��ġ ����");
            _feedbackPlayer.PlayFeedback(PlayerFeedbackType.ChargeFinish_FB, _chargeEffectPoint.position);
        }
    }
    /// <summary>
    /// ���� �������� ���� �ǰ� ȿ��
    /// </summary>
    /// <param name="collider">�浹�� ������Ʈ</param>
    public void TriggerChargeAttackAffect(Collider collider, int tier)
    {
        OnChargeAttackAffect.Invoke(collider);

        switch(tier)
        {
            case 1:
                _feedbackPlayer.PlayFeedback(PlayerFeedbackType.Tier1ChargeAttackHit_FB, collider.transform.position);
                break;
            case 2:
                _feedbackPlayer.PlayFeedback(PlayerFeedbackType.Tier2ChargeAttackHit_FB, collider.transform.position);
                break ;
            case 3:
                _feedbackPlayer.PlayFeedback(PlayerFeedbackType.Tier3ChargeAttackHit_FB, collider.transform.position);
                break ;
        }   
    }


            
    /// <summary>
    /// ���Ÿ� ���� ��¡ �Ϸ� �� ȿ��
    /// </summary>
    public void TriggerRangedChargeFinish()
    {
        if (_chargeEffectPoint != null)
        {
            _feedbackPlayer.PlayFeedback(PlayerFeedbackType.RangeAttackChargeFinish_FB, _chargeEffectPoint.position);
        }
    }
    /// <summary>
    /// ���Ÿ� ���� ���� �� ȿ��
    /// </summary>
    public void TriggerRangedAttackStart()
    {
        OnRangedAttackStart.Invoke(_rangedAttackPoint);
    }
    /// <summary>
    /// ���Ÿ� ���� �� ȿ��
    /// </summary>
    /// <param name="collider">�浹�� ������Ʈ</param>
    public void TriggerRangedAttackAffect(Collider collider)
    {
        OnRangedAttackAffect.Invoke(collider);

        if (collider != null)
        {
            _feedbackPlayer.PlayFeedback(PlayerFeedbackType.RangeAttackHit_FB, collider.transform.position);
        }
    }
    /// <summary>
    /// ���Ÿ� ���� ���� �� ȿ��
    /// (�ִϸ��̼� Ʈ����)
    /// </summary>
    public void TriggerRangedAttackFinish()
    {
        OnRangedAttackFinish.Invoke();
    }


    /// <summary>
    /// �и� ���� �� ȿ��
    /// (�ִϸ��̼� Ʈ����)
    /// </summary>
    public void TriggerParryPerform()
    {
        OnParryPerform?.Invoke();
    }
    /// <summary>
    /// �и� ���� �� ȿ��
    /// </summary>
    /// <param name="collider">�и� ���� ������Ʈ</param>
    public void TriggerParryAffect(Collider collider)
    {
        OnParryAffect?.Invoke(collider);
         
        if (collider != null)
        {
            _feedbackPlayer.PlayFeedback(PlayerFeedbackType.ParrySuccess_FB, collider.transform.position);
        }
    }


    /// <summary>
    /// Ƽ� �ö� �� ȿ��
    /// </summary>
    /// <param name="tier">���� Ƽ��</param>
    public void TriggerTierUp(int tier)
    {
        switch (tier)
        {
            case 1:
                OnTier1Up?.Invoke(); 
                break;
            case 2: 
                OnTier2Up?.Invoke(); 
                break;
            case 3: 
                OnTier3Up?.Invoke();
                break;
            case 4:
                OnOverHeatStart?.Invoke(); 
                break;
        }
    }
    /// <summary>
    /// Ƽ� ������ �� ȿ��
    /// </summary>
    /// <param name="tier">���� Ƽ��</param>
    public void TriggerTierDown(int tier)
    {
        switch (tier)
        {
            case 0:
                OnTier1Down.Invoke();
                break;
            case 1:
                OnTier2Down.Invoke();
                break;
            case 2:
                OnTier3Down.Invoke();
                break;
            case 3:
                OnOverHeatFinish.Invoke();
                break;
        }
    }

    #endregion
}


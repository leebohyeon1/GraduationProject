using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;
using System;

public class Service_BattleAwareness : ServiceNode
{
    Animator _animator;
    private int _threatTagHash = Animator.StringToHash("Threat");
    private int _chanceTagHash = Animator.StringToHash("Chance");
    public float Threat_Angle = 25f;
    public string Vuln_Key = "IsPlayerVulnerable";
    public string Aim_Key = "IsPlayerAimingMe"; 

    public override void OnEnter()
    {
        base.OnEnter();
        _animator = runner.player.Animator;
    }
    protected override void OnServiceLogic()
    {
        if(!runner._aiController._aiBrain._isCombat)return;
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        //플레이어 - 내 위치 벡터 내위치에서 플레이어 바라보기
        Vector3 directionToPlayer = (runner.transform.position - runner.player.transform.position).normalized;
        //내 앞벡터와 플레이어 방향 벡터의 내적
        float dot = Vector3.Dot(runner.player.transform.forward, directionToPlayer);
        //각도 비교
        runner._aiController._aiBrain.blackboard.SetValue("ZigZag", dot);
        //태그 확인중
        if (stateInfo.tagHash == _threatTagHash && (dot >= Mathf.Cos(Threat_Angle * Mathf.Deg2Rad)) )
        {
            runner._aiController._aiBrain.blackboard.SetValue(Aim_Key, true);
        }
        else
        {
            runner._aiController._aiBrain.blackboard.SetValue(Aim_Key, false);
        }




        if(stateInfo.tagHash == _chanceTagHash )
        {
            runner._aiController._aiBrain.blackboard.SetValue(Vuln_Key, true);
        }
        else
        {
            
                runner._aiController._aiBrain.blackboard.SetValue(Vuln_Key, false);
        }
    }
    public override Node Clone()
    {
        return Instantiate(this);
    }
}

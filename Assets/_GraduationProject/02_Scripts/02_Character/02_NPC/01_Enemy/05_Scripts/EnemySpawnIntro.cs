using UnityEngine;
using System.Collections;

public class EnemySpawnIntro : MonoBehaviour
{
    public string stateName;
    public float activationDistance=18f;
    public float minimumDuration=0.5f;
    public bool Completed {get;private set;}
    public bool Playing {get;private set;}
    private Enemy _enemy;
    private Coroutine _routine;
    private void Awake()=>_enemy=GetComponent<Enemy>();
    private void OnEnable(){Completed=false;Playing=false;}
    private void Update()
    {
        if(Completed || Playing || !_enemy || !_enemy.player || !_enemy.animator) return;
        if(!_enemy._initializer || !_enemy._initializer.IsInitialized("Final") || _enemy.Movement==null || !_enemy._aiController || _enemy._aiController._aiBrain==null) return;
        if(Vector3.Distance(transform.position,_enemy.player.transform.position)<=activationDistance) _routine=StartCoroutine(PlayIntro());
    }
    private IEnumerator PlayIntro()
    {
        Playing=true;
        var animator=_enemy.animator;
        _enemy.Movement.StopMovement();
        int hash=Animator.StringToHash(stateName);
        if(!animator.HasState(0,hash)) {Debug.LogError($"Missing intro state {stateName} on {name}"); Playing=false; yield break;}
        animator.Play(hash,0,0);
        yield return null;
        float elapsed=0;
        while(elapsed<minimumDuration || (animator.GetCurrentAnimatorStateInfo(0).fullPathHash==hash && animator.GetCurrentAnimatorStateInfo(0).normalizedTime<1))
        {elapsed+=Time.deltaTime; yield return null;}
        Completed=true;Playing=false;_routine=null;
        if(!_enemy.EnemyHealth.IsDead) {
            _enemy._stateController.SetLock(false);
            _enemy.SetState(EnemyStateController.EnemyState.Idle);
            _enemy.animHandler.FinishAction();
            if (_enemy.aIPath) { _enemy.aIPath.enabled=true; _enemy.aIPath.canMove=true; _enemy.aIPath.isStopped=false; }
            _enemy._aiController.CombatEnter();
            if (_enemy.groupAi) { _enemy.groupAi.CombatAll(); _enemy.groupAi.EngageCombatAll(); }
        }
    }
    private void OnDisable(){if(_routine!=null)StopCoroutine(_routine);_routine=null;Playing=false;}
}

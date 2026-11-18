using UnityEngine;
using System.Collections;
using UnityEngine.Playables;

public class EnemySpawnIntro : MonoBehaviour
{
    public string stateName;
    public float activationDistance=18f;
    public float minimumDuration=0.5f;
    public bool Completed {get;private set;}
    public bool Playing {get;private set;}
    public PlayableDirector timelineDirector {get;private set;}
    public bool TimelineStarted {get;private set;}
    public bool TimelineFinished {get;private set;}
    public float PlaybackElapsed => timelineDirector ? Mathf.Max(_timelineElapsed,(float)timelineDirector.time) : Mathf.Max(0f, Time.time - _startedAt);
    private Enemy _enemy;
    private Coroutine _routine;
    private float _startedAt;
    private AnimatorCullingMode _previousCulling;
    private bool _ownsCulling;
    private bool _subscribedToTimeline;
    private float _timelineElapsed;
    private void Awake()=>_enemy=GetComponent<Enemy>();
    private void OnEnable(){Completed=false;Playing=false;SubscribeTimeline();}
    public void BindTimeline(PlayableDirector director)
    {
        if(timelineDirector!=director)
        {
            UnsubscribeTimeline();timelineDirector=director;
            TimelineStarted=false;TimelineFinished=false;_timelineElapsed=0f;
        }
        SubscribeTimeline();
    }
    private void SubscribeTimeline()
    {
        if(!timelineDirector || _subscribedToTimeline || !isActiveAndEnabled)return;
        timelineDirector.played+=OnTimelinePlayed;
        timelineDirector.stopped+=OnTimelineStopped;
        _subscribedToTimeline=true;
        if(timelineDirector.state==PlayState.Playing)OnTimelinePlayed(timelineDirector);
    }
    private void UnsubscribeTimeline()
    {
        if(timelineDirector && _subscribedToTimeline)
        {timelineDirector.played-=OnTimelinePlayed;timelineDirector.stopped-=OnTimelineStopped;}
        _subscribedToTimeline=false;
    }
    private void OnTimelinePlayed(PlayableDirector director)
    {
        TimelineStarted=true;TimelineFinished=false;_timelineElapsed=(float)director.time;
    }
    private void OnTimelineStopped(PlayableDirector director)
    {
        if(!TimelineStarted)return;
        TimelineFinished=true;
        _timelineElapsed=Mathf.Max(_timelineElapsed,(float)director.duration);
    }
    private void Update()
    {
        // Record playback before the deferred initialization gate: a slow machine may initialize after the 3.3s Timeline ends.
        if(timelineDirector && timelineDirector.state==PlayState.Playing)
        {TimelineStarted=true;_timelineElapsed=Mathf.Max(_timelineElapsed,(float)timelineDirector.time);}
        if(Completed || Playing || !_enemy || !_enemy.player || !_enemy.animator) return;
        if(!_enemy._initializer || !_enemy._initializer.IsInitialized("Final") || _enemy.Movement==null || !_enemy._aiController || _enemy._aiController._aiBrain==null) return;
        if(timelineDirector)
        {
            if(TimelineStarted) _routine=StartCoroutine(PlayIntro());
        }
        else if(Vector3.Distance(transform.position,_enemy.player.transform.position)<=activationDistance) _routine=StartCoroutine(PlayIntro());
    }
    private IEnumerator PlayIntro()
    {
        Playing=true;
        _startedAt=Time.time;
        var animator=_enemy.animator;
        _enemy.Movement.StopMovement();
        _previousCulling=animator.cullingMode;_ownsCulling=true;
        animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        int hash=Animator.StringToHash(stateName);
        if(timelineDirector)
        {
            // The side-dungeon Timeline already drives Spawn_f; do not play the clip a second time.
            while(timelineDirector && !TimelineFinished && timelineDirector.state==PlayState.Playing) yield return null;
        }
        else
        {
            if(!animator.HasState(0,hash)) {Debug.LogError($"Missing intro state {stateName} on {name}"); Playing=false; RestoreCulling(); yield break;}
            animator.Play(hash,0,0);
            yield return null;
            float elapsed=0;
            while(elapsed<minimumDuration || (animator.GetCurrentAnimatorStateInfo(0).fullPathHash==hash && animator.GetCurrentAnimatorStateInfo(0).normalizedTime<1))
            {elapsed+=Time.deltaTime; yield return null;}
        }
        Completed=true;Playing=false;_routine=null;
        RestoreCulling();
        if(!_enemy.EnemyHealth.IsDead) {
            _enemy._stateController.SetLock(false);
            _enemy.SetState(EnemyStateController.EnemyState.Idle);
            _enemy.animHandler.FinishAction();
            if (_enemy.aIPath) { _enemy.aIPath.enabled=true; _enemy.aIPath.canMove=true; _enemy.aIPath.isStopped=false; }
            _enemy._aiController.CombatEnter();
            if (_enemy.groupAi) { _enemy.groupAi.CombatAll(); _enemy.groupAi.EngageCombatAll(); }
        }
    }
    private void RestoreCulling(){if(_ownsCulling && _enemy && _enemy.animator)_enemy.animator.cullingMode=_previousCulling;_ownsCulling=false;}
    private void OnDisable(){if(_routine!=null)StopCoroutine(_routine);_routine=null;Playing=false;RestoreCulling();UnsubscribeTimeline();}
}

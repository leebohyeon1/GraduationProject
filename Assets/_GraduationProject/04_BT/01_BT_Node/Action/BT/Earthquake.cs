using UnityEngine;
using BehaviorTree;
using JetBrains.Annotations;

[CreateAssetMenu(fileName = "EarthquakeNode", menuName = "BehaviorTree/EarthquakeNode")]
public class Earthquake : Node
{
    [Header("earthquake Settings")]
    [SerializeField] float _maxRadius = 20f;
    [SerializeField] float _speed = 2f;
    [SerializeField] GameObject EarthquakeWave;
    public string AnimationName = "ShockWave";
    public override Node Clone()
    {
        return Instantiate(this);
    }
    public override void OnEnter()
    {
        base.OnEnter();
        runner.AnimationEvent(AnimationName);
        runner.SetState(Enemy.EnemyState.Attack);
    }
    protected override NodeState OnUpdate()
    {
        // IsName의 인자는 애니메이션 상태(State)의 이름이어야 합니다.
        if (Handler.IsHitWindowOpen)
        {
            Vector3 spawnPos = runner.transform.position + Vector3.up * 0.5f;
            Quaternion spawnRot = Quaternion.Euler(0, 0, 180);
            var Earth = Instantiate(EarthquakeWave, spawnPos, spawnRot);

            //var wave = Earth.GetComponent<DonutWave>();
            //wave.speed = _speed;
            //wave.maxRadius = _maxRadius;

            return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }
    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("Earthquake OnExit");
        runner.SetState(Enemy.EnemyState.Idle);
        Handler.CloseHitWindow();

    }

}

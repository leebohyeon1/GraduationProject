using UnityEngine;


public abstract class EnemyUseAnything : ScriptableObject
{
    public abstract T OnUpdate<T>(T runner) where T : Enemy;
    public abstract T OnEnter<T>(T runner) where T : Enemy;
    public abstract T OnExit<T>(T runner) where T : Enemy;
    public virtual bool UseSomeThing<T>(T runner) where T : Enemy
    {
        return true;
    }
    public abstract void Reset<T>(T runner) where T : Enemy;

    /// <summary>
    /// 특정 액션 이벤트 발생 시 호출됨 (이동공격, 특수한 공격 등에서 사용)
    /// OnEnter()보다 더 일찍 호출되어 초기 설정에 적합
    /// </summary>
    /// <param name="runner">Enemy 러너</param>
    /// <returns>runner (체이닝을 지원하기 위해 동일 타입 반환)</returns>
    public virtual T OnActionTriggered<T>(T runner) where T : Enemy
    {
        return runner;
    }
}




using UnityEngine;


public abstract class EnemyUseAnything : ScriptableObject {

    public abstract T OnUpdate<T>(T enemy) where T : Enemy;
    public abstract T OnEnter<T>(T enemy) where T : Enemy;
}
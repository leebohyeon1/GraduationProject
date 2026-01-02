using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class EnemyUseAnything : ScriptableObject
{
    public abstract T OnUpdate<T>(T runner) where T : Enemy;
    public abstract T OnEnter<T>(T runner) where T : Enemy;
    public abstract T OnExit<T>(T runner) where T : Enemy;
}




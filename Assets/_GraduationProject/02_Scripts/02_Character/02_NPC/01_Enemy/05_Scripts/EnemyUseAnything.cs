using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BehaviorTree;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders.Simulation;


public abstract class EnemyUseAnything : ScriptableObject
{
    public abstract T OnUpdate<T>(T runner) where T : Enemy;
    public abstract T OnEnter<T>(T runner) where T : Enemy;
    public abstract T OnExit<T>(T runner) where T : Enemy;
    public virtual bool UseSomeThing<T>(T runner,bool check) where T : Enemy
    {
        return true;
    }
}




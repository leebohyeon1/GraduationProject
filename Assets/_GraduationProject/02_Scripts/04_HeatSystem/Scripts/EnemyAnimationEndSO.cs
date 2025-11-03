using UnityEngine;

[CreateAssetMenu(fileName = "Enemy_AnimationEnd_SO", menuName = "HeatSystem/Enemy/Enemy_AnimationEnd_SO")]
public class EnemyAnimationEndSO : EnemyUseAnything
{
    public string animationName = "";
    public bool animationBool = true;
    public override T OnEnter<T>(T enemy)
    {
        enemy.AnimationBool(animationName, animationBool);
        return enemy;
    }



    public override T OnUpdate<T>(T enemy)
    {
        throw new System.NotImplementedException();
    }
}
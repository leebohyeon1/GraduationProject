using BH_Lib.Log;
using NUnit.Framework;
using UnityEngine;

public class TestEnemyProjectile : Projectile, IParryable
{
    public bool IsParryable => true;


    public bool Parry(GameObject parryInstigator)
    {
        if (IsParryable)
        {
            Log.PrintColor(Color.red, $"테스트 적 총알 패링 성공");

            Destroy(gameObject);
            return true;
        }
        else
        {
            return false;
        }

    }

}

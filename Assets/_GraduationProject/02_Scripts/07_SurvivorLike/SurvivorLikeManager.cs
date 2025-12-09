using BH_Lib.DI;
using UnityEngine;

[Register(LifetimeScope.Singleton)]
public class SurvivorLikeManager : MonoBehaviour
{
    [SerializeField] private SurvivorLikeWaveSO _currentWave;
    [SerializeField] private int _waveIndex;
}

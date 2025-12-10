using BH_Lib.AssetManager;
using BH_Lib.DI;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

[Register(LifetimeScope.Singleton)]
public class SurvivorLikeManager : MonoBehaviour
{
    [Header("Wave")]
    [SerializeField] private SurvivorLikeWaveSO _currentWave;
    [SerializeField] private int _waveIndex;
    [SerializeField] private float _nextWaveHoldDuration = 1f;
    private float _currentWaveHoldPercent = 0f;


    [Header("Spawn")]
    [SerializeField] private Transform[] _spawnPoints;

    [Header("Input")]
    [SerializeField] private InputReader _inputReader;


    private Dictionary<string, List<GameObject>> _enemyPool = new Dictionary<string, List<GameObject>>();

    private async void Start()
    {
        if(!_inputReader)
        {
            _inputReader = await DIContainer.Instance.Resolve<AssetManager>().
                LoadAssetAsync <InputReader>("InputReader", gameObject);
        }

        _inputReader.InteractHoldEvent += OnInteractHold;
        _inputReader.InteractCancelEvent += OnInteractCancel;
    }

    private void OnDestroy()
    {
        _inputReader.InteractHoldEvent -= OnInteractHold;
        _inputReader.InteractCancelEvent -= OnInteractCancel;
    }


    /// <summary>
    /// 다음 웨이브
    /// </summary>
    private void NextWave()
    {
        _currentWave = _currentWave.GetNextWave();

        int index = 0;
        int different = 0;
        foreach (var entri in _currentWave.Entries)
        {
            GameObject enemyPrefab = entri.EnemyPrefab;
            if (!_enemyPool.TryGetValue(enemyPrefab.name, out List<GameObject> pool))
            {
                _enemyPool.Add(enemyPrefab.name, new List<GameObject>());
            }

            // 스폰해야할 적 숫자와 현재 풀링되어 있는 적 숫자의 차
            different = entri.EnemyCount - pool.Count;
            // 스폰해야할 적 수가 많으면 적 소환
            if(different > 0)
            {
                for (index = 0; index < different; index++)
                {
                    GameObject newEnemy = Instantiate(enemyPrefab);
                    newEnemy.SetActive(false);
                    _enemyPool[enemyPrefab.name].Add(newEnemy);
                }
            }

            // 스폰해야할 수만큼 스폰
            for (index = 0; index < entri.EnemyCount; index++)
            {
                Spawn(pool[index]);
            }
        }

        _waveIndex++;
    }
    
    /// <summary>
    /// 적을 랜덤 위치에 스폰
    /// </summary>
    /// <param name="gameObject">스폰할 오브젝트</param>
    private void Spawn(GameObject gameObject)
    {
        int randomIndex = Random.Range(0, _spawnPoints.Length - 1);
        gameObject.transform.position = _spawnPoints[randomIndex].position;

        gameObject.SetActive(true);
    }

    private void UpdateHoldTimer(bool isHold)
    {
        DOTween.Kill(this);

        if(isHold)
        {
            DOTween.To(() => _currentWaveHoldPercent,
            (X) =>
            {
                _currentWaveHoldPercent = X;
            },
            1f,
            _nextWaveHoldDuration)
            .OnComplete(NextWave)
            .SetId(this);
        }
        else
        {
            float duration = _nextWaveHoldDuration * _currentWaveHoldPercent;
            DOTween.To(() => _currentWaveHoldPercent,
              (X) =>
              {
                  _currentWaveHoldPercent = X;
              },
              0f,
              duration)
              .SetId(this);
        }

    }

    #region Event
    private void OnInteractHold()
    {
        UpdateHoldTimer(true);
    }

    private void OnInteractCancel()
    {
        UpdateHoldTimer(false);
    }

    #endregion

}

using BH_Lib.AssetManager;
using BH_Lib.DI;
using BH_Lib.Log;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

[Register(LifetimeScope.Singleton)]
public class SurvivorLikeManager : MonoBehaviour
{
    [Header("Wave")]
    [SerializeField] private SurvivorLikeWaveSO _currentWave;
    [SerializeField] private int _waveIndex;
    [SerializeField] private float _nextWaveHoldDuration = 1f;
    private float _currentWaveHoldPercent = 0f;
    private bool _canSkipWave = true;

    [Header("Spawn")]
    [SerializeField] private Transform[] _spawnPoints;

    [Header("Input")]
    [SerializeField] private InputReader _inputReader;

    [Header("Events")]
    [SerializeField] private UpdateNextWaveHoldTimeEventSO UpdateNextWaveHoldTimeEvent;

    private Dictionary<string, List<GameObject>> _enemyPool = new Dictionary<string, List<GameObject>>();
    private List<GameObject> _arriveEnemyList = new List<GameObject>();   

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
        List<GameObject> pool = new List<GameObject>();
        foreach (var entri in _currentWave.Entries)
        {
            GameObject enemyPrefab = entri.EnemyPrefab;
            if (!_enemyPool.TryGetValue(enemyPrefab.name, out pool))
            {
                Log.PrintColor(Color.beige, "풀 생성");
                _enemyPool.Add(enemyPrefab.name, new List<GameObject>());
                pool = _enemyPool[enemyPrefab.name];
            }

            // 스폰해야할 적 숫자와 현재 풀링되어 있는 적 숫자의 차
            different = entri.EnemyCount - pool.Count;
            Log.Print("차이: " + different);

            // 스폰해야할 적 수가 많으면 적 소환
            if (different > 0)
            {
                for (index = 0; index < different; index++)
                {
                    Log.PrintColor(Color.beige, "오브젝트 생성");
                    GameObject newEnemy = Instantiate(enemyPrefab);
                    newEnemy.SetActive(false);
                    _enemyPool[enemyPrefab.name].Add(newEnemy);
                }
            }

            // 스폰해야할 수만큼 스폰
            for (index = 0; index < entri.EnemyCount; index++)
            {
                Log.PrintColor(Color.beige, "스폰");
                Spawn(pool[index], entri.StatMultiplier);
            }
        }

        _waveIndex++;

        _canSkipWave = false;

        _currentWaveHoldPercent = 0;
        UpdateNextWaveHoldTimeEvent.Publish(_currentWaveHoldPercent);
    }
    
    /// <summary>
    /// 적을 랜덤 위치에 스폰
    /// </summary>
    /// <param name="gameObject">스폰할 오브젝트</param>
    private void Spawn(GameObject gameObject, EnemyStatMultiplier statMultiplier)
    {
        gameObject.SetActive(true);

        Enemy enemy = gameObject.GetComponent<Enemy>(); 
        // 적 스텟 배율 적용
        gameObject.GetComponent<AiController>().Initialize(enemy, statMultiplier);
        gameObject.GetComponent<EnemyTakeDmg>().InitializeHealth(enemy, statMultiplier);
        
        // 서바이벌 콘텐츠 전용 컴포넌트 적용
        SurvivorLikeEnemyConfig config = gameObject.AddComponent<SurvivorLikeEnemyConfig>();
        config.Died += OnEnemyDied;
        
        // 스폰 위치 설정
        int randomIndex = Random.Range(0, _spawnPoints.Length - 1);
        gameObject.transform.position = _spawnPoints[randomIndex].position;

        // 살아있는 적 리스트에 등록
        _arriveEnemyList.Add(gameObject);
    }

    /// <summary>
    /// 웨이브 넘어가는 타이머 업데이트
    /// </summary>
    /// <param name="isHold"></param>
    private void UpdateHoldTimer(bool isHold)
    {
        if(!_canSkipWave)
        {
            return;
        }

        DOTween.Kill(this);

        if(isHold)
        {
            DOTween.To(() => _currentWaveHoldPercent,
            (X) =>
            {
                _currentWaveHoldPercent = X;
                UpdateNextWaveHoldTimeEvent.Publish(_currentWaveHoldPercent);
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
                  UpdateNextWaveHoldTimeEvent.Publish(_currentWaveHoldPercent);
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

    private void OnEnemyDied(GameObject gameObject)
    {
        _arriveEnemyList.Remove(gameObject);

        if(_arriveEnemyList.Count <= 0)
        {
            _canSkipWave = true;
        }

        gameObject.GetComponent<SurvivorLikeEnemyConfig>().Died -= OnEnemyDied;
    }

    #endregion

}

using BH_Lib.AssetManager;
using BH_Lib.DI;
using BH_Lib.Log;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Register(LifetimeScope.Singleton)]
public class SurvivorLikeManager : MonoBehaviour, IEventListener<AbilitySO>
{
    [Header("Wave")]
    [SerializeField] private SurvivorLikeWaveSO _currentWave;
    [SerializeField] private int _waveIndex;
    [SerializeField] private float _nextWaveHoldDuration = 1f;
    private float _currentWaveHoldPercent = 0f;
    private bool _canSkipWave = true;

    [Header("Spawn")]
    [SerializeField] private Transform[] _spawnPoints;

    [Header("Ability")] 
    [SerializeField] private AbilitySelectUI _abilitySelectUI;
    [SerializeField] private List<AbilityList> _abilityList;
    [SerializeField] private AbilitySelectedSO _abilitySelectSO;

    [Header("Input")]
    [SerializeField] private InputReader _inputReader;

    [Header("Events")]
    [SerializeField] private UpdateNextWaveHoldTimeEventSO UpdateNextWaveHoldTimeEvent;

    private Dictionary<string, List<GameObject>> _enemyPool = new Dictionary<string, List<GameObject>>();
    private List<GameObject> _arriveEnemyList = new List<GameObject>();

    private async void Start()
    {
        if (!_inputReader)
        {
            _inputReader = await DIContainer.Instance.Resolve<AssetManager>().
                LoadAssetAsync<InputReader>("InputReader", gameObject);
        }

        _inputReader.InteractHoldEvent += OnInteractHold;
        _inputReader.InteractCancelEvent += OnInteractCancel;
        _abilitySelectSO.Subscribe(this);

        // 능력 선택 UI에 매니저 할당
        if (_abilitySelectUI != null)
        {
            _abilitySelectUI.Manager = this;
        }
        else
        {
            Debug.LogError("AbilitySelectUI가 할당되지 않았습니다.");
        }
    }

    private void OnDestroy()
    {
        _inputReader.InteractHoldEvent -= OnInteractHold;
        _inputReader.InteractCancelEvent -= OnInteractCancel;
        _abilitySelectSO.Unsubscribe(this);
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

            different = entri.EnemyCount - pool.Count;
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

            for (index = 0; index < entri.EnemyCount; index++)
            {
                Log.PrintColor(Color.beige, "스폰");
                Spawn(pool[index], entri.StatMultiplier);
            }
        }

        _waveIndex++;
        _canSkipWave = false;
        _currentWaveHoldPercent = -1f;
        UpdateNextWaveHoldTimeEvent.Publish(-1f);
    }

    private void Spawn(GameObject gameObject, EnemyStatMultiplier statMultiplier)
    {
        gameObject.SetActive(true);

        Enemy enemy = gameObject.GetComponent<Enemy>();
        gameObject.GetComponent<AiController>().Initialize(enemy, statMultiplier);
        gameObject.GetComponent<EnemyHealth>().InitializeHealth(enemy, statMultiplier);

        SurvivorLikeEnemyConfig config = gameObject.AddComponent<SurvivorLikeEnemyConfig>();
        config.Died += OnEnemyDied;

        int randomIndex = Random.Range(0, _spawnPoints.Length);
        gameObject.transform.position = _spawnPoints[randomIndex].position;

        _arriveEnemyList.Add(gameObject);
    }

    private void UpdateHoldTimer(bool isHold)
    {
        if (!_canSkipWave)
        {
            return;
        }

        DOTween.Kill(this);

        if (isHold)
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

    /// <summary>
    /// 웨이브 종료 후 능력 선택을 시작합니다.
    /// </summary>
    private void StartAbilitySelection()
    {
        _inputReader.EnableUIActions();
        _inputReader.DisablePlayerActions();

        _canSkipWave = false;
        UpdateNextWaveHoldTimeEvent.Publish(-1f); // 다음 웨이브 UI 숨김

        // 능력 풀에서 랜덤하게 3개 선택 (중복 없이)
        int i = 0;
        List<AbilitySO> randomAbilities = new List<AbilitySO>();
        for (i = 0; i < 3; i++)
        {
            float rand = Random.Range(0f, 100f);
            
            foreach (var abilityList in _abilityList)
            {
                if (rand <= abilityList.Probability)
                {
                    AbilitySO randomAbility = abilityList.Abilities[Random.Range(0, abilityList.Abilities.Count)];
                    randomAbilities.Add(randomAbility);
                    break;
                }
            } 
        }

        _abilitySelectUI.Show(randomAbilities);
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
        gameObject.GetComponent<SurvivorLikeEnemyConfig>().Died -= OnEnemyDied;

        if (_arriveEnemyList.Count <= 0)
        {
            StartAbilitySelection();
        }
    }

    /// <summary>
    /// 플레이어가 능력을 선택했을 때 호출됩니다.
    /// </summary>
    public void OnEventTrigger(AbilitySO eventName)
    {
        _inputReader.EnablePlayerActions();
        _inputReader.DisableUIActions();

        // 다음 웨이브로 넘어갈 수 있도록 설정
        _canSkipWave = true;
        UpdateNextWaveHoldTimeEvent.Publish(0f);
    }

    #endregion
}

[Serializable]
public class AbilityList
{
    public float Probability;
    public List<AbilitySO> Abilities;
}   

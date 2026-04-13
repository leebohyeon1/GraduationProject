using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class MonsterWave : MonoBehaviour
{
    [Serializable]
    private class WaveSettings
    {
        [SerializeField] private Enemy[] enemies;
        public Enemy[] Enemies => enemies;
    }

    [Header("Debug")]
    [SerializeField] private bool _enableLogging = false;

    [Header("Escape")]
    [SerializeField] private GameObject escape;

    [Header("Trigger")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Wave")]
    [SerializeField] private WaveSettings[] waves;
    [SerializeField] private string spawnFeedbackName;
    [SerializeField] private string spawnAnimationTrigger;
    [SerializeField] private float nextWaveDelay = 2f;

    [Header("Spawn/Activation Budget")]
    [SerializeField] private int maxActivationsPerFrame = 2;
    [SerializeField] private float activationBatchDelay = 0f;
    [SerializeField] private bool prewarmAllWaveEnemiesOnStart = true;
    [SerializeField] private int prewarmPerFrame = 1;

    [Header("Spawn Options")]
    [SerializeField] private float feedbackDelay = 0.5f;
    [SerializeField] private float minDistance = 5f;

    private BoxCollider _boxCollider;
    private int _currentWaveIndex;
    private int _aliveInCurrentWave;
    private bool _isRunning;
    private Coroutine _nextWaveCoroutine;
    private Coroutine _waveActivationCoroutine;
    private Coroutine _prewarmCoroutine;
    private Action<Enemy> wave;
    private PlayerController player;
    private bool _prewarmCompleted;
    private bool _pendingStartAfterPrewarm;

    public UnityEvent OnWaveClear;

    private void Reset()
    {
        var box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
        if (!_boxCollider.isTrigger) _boxCollider.isTrigger = true;

        DeactivateWaveEnemies();

        wave += WaveAiController;
        wave += WaveTrigger;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.GetComponent<PlayerController>();
    }

    private void Start()
    {
        if (!prewarmAllWaveEnemiesOnStart)
        {
            _prewarmCompleted = true;
            return;
        }

        _prewarmCoroutine = StartCoroutine(PrewarmEnemiesRoutine());
    }

    private void OnDestroy()
    {
        if (_prewarmCoroutine != null) StopCoroutine(_prewarmCoroutine);
        if (_waveActivationCoroutine != null) StopCoroutine(_waveActivationCoroutine);
        if (_nextWaveCoroutine != null) StopCoroutine(_nextWaveCoroutine);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isRunning) return;
        if (!IsInLayerMask(other.gameObject.layer, playerLayer)) return;

        _isRunning = true;
        _boxCollider.enabled = false;
        _currentWaveIndex = 0;

        if (escape != null) escape.SetActive(true);

        if (_prewarmCompleted)
        {
            StartWave();
        }
        else
        {
            _pendingStartAfterPrewarm = true;
        }
    }

    private void WaveAiController(Enemy target)
    {
        // AI는 ActivateEnemyForWave에서 즉시 활성화 처리.
    }

    private void WaveTrigger(Enemy target)
    {
        EnemyAnimationBridge animationBridge = target.GetComponent<EnemyAnimationBridge>();
        if (animationBridge != null)
        {
            animationBridge.TriggerEvent(spawnAnimationTrigger, feedbackDelay);
        }
    }

    private void StartWave()
    {
        if (waves == null || waves.Length == 0)
        {
            FinishAllWaves();
            return;
        }

        if (_currentWaveIndex >= waves.Length)
        {
            FinishAllWaves();
            return;
        }

        WaveSettings settings = waves[_currentWaveIndex];
        Enemy[] waveEnemies = settings != null ? settings.Enemies : null;

        if (waveEnemies == null || waveEnemies.Length == 0)
        {
            _currentWaveIndex++;
            StartWave();
            return;
        }

        _aliveInCurrentWave = 0;
        if (_waveActivationCoroutine != null) StopCoroutine(_waveActivationCoroutine);
        _waveActivationCoroutine = StartCoroutine(ActivateWaveEnemiesRoutine(waveEnemies));
    }

    private IEnumerator PrewarmEnemiesRoutine()
    {
        int budget = Mathf.Max(1, prewarmPerFrame);
        int used = 0;

        if (waves != null)
        {
            for (int wi = 0; wi < waves.Length; wi++)
            {
                Enemy[] arr = waves[wi] != null ? waves[wi].Enemies : null;
                if (arr == null) continue;

                for (int i = 0; i < arr.Length; i++)
                {
                    Enemy target = arr[i];
                    if (target == null || target.gameObject.activeSelf) continue;

                    target.gameObject.SetActive(true);
                    yield return null; // Awake/Initialize 분산

                    var ai = target.GetComponent<AiController>();
                    if (ai != null) ai.enabled = false;
                    target.gameObject.SetActive(false);

                    used++;
                    if (used >= budget)
                    {
                        used = 0;
                        yield return null;
                    }
                }
            }
        }

        _prewarmCompleted = true;
        _prewarmCoroutine = null;

        if (_pendingStartAfterPrewarm && _isRunning)
        {
            _pendingStartAfterPrewarm = false;
            StartWave();
        }
    }

    private IEnumerator ActivateWaveEnemiesRoutine(Enemy[] waveEnemies)
    {
        int budgetPerFrame = Mathf.Max(1, maxActivationsPerFrame);
        int activatedThisFrame = 0;

        for (int i = 0; i < waveEnemies.Length; i++)
        {
            Enemy target = waveEnemies[i];
            if (target == null) continue;

            ActivateEnemyForWave(target);
            activatedThisFrame++;

            if (activatedThisFrame >= budgetPerFrame)
            {
                activatedThisFrame = 0;
                if (activationBatchDelay > 0f) yield return new WaitForSeconds(activationBatchDelay);
                else yield return null;
            }
        }

        _waveActivationCoroutine = null;

        if (_aliveInCurrentWave <= 0)
        {
            _currentWaveIndex++;
            StartWave();
        }
    }

    private void ActivateEnemyForWave(Enemy target)
    {
        target.gameObject.SetActive(true);
        var ai = target.GetComponent<AiController>();
        if (ai != null && !ai.enabled) ai.enabled = true;
        SetHealthBarVisible(target, true);
        EnsureMinDistanceFromPlayer(target);
        PlaySpawnFeedback(target);
        StartCoroutine(Timer(feedbackDelay, () => wave?.Invoke(target)));

        EnemyHealth health = target.EnemyHealth;
        if (health == null) health = target.GetComponent<EnemyHealth>();
        if (health == null) return;

        _aliveInCurrentWave++;

        Action handler = null;
        handler = () =>
        {
            health.OnDied -= handler;
            HandleEnemyDied();
        };
        health.OnDied += handler;
    }

    private void EnsureMinDistanceFromPlayer(Enemy target)
    {
        if (player == null) return;

        Vector3 playerPos = player.transform.position;
        float dist = Vector3.Distance(target.transform.position, playerPos);
        if (dist >= minDistance) return;

        Vector3 awayDir = (target.transform.position - playerPos).normalized;
        if (awayDir == Vector3.zero) awayDir = UnityEngine.Random.insideUnitSphere;
        awayDir.y = 0f;
        target.transform.position = playerPos + awayDir * minDistance;
    }

    private void PlaySpawnFeedback(Enemy target)
    {
        if (string.IsNullOrEmpty(spawnFeedbackName) && string.IsNullOrEmpty(spawnAnimationTrigger)) return;

        Enemy_AnimationEventHandler animationHandler = target.GetComponent<Enemy_AnimationEventHandler>();
        if (animationHandler != null && !string.IsNullOrEmpty(spawnFeedbackName))
        {
            animationHandler.PlayFeedback(spawnFeedbackName);
        }
    }

    private IEnumerator Timer(float delay, Action onComplete)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        onComplete?.Invoke();
    }

    private void HandleEnemyDied()
    {
        _aliveInCurrentWave = Mathf.Max(0, _aliveInCurrentWave - 1);
        if (_aliveInCurrentWave == 0)
        {
            _currentWaveIndex++;
            StartNextWaveWithDelay();
        }
    }

    private void FinishAllWaves()
    {
        if (escape != null) escape.SetActive(false);
        SetAllWaveEnemyHealthBars(false);
        OnWaveClear?.Invoke();
    }

    private static bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    private void StartNextWaveWithDelay()
    {
        if (_nextWaveCoroutine != null) StopCoroutine(_nextWaveCoroutine);

        float delay = Mathf.Max(0f, nextWaveDelay);
        _nextWaveCoroutine = StartCoroutine(NextWaveDelayRoutine(delay));
    }

    private IEnumerator NextWaveDelayRoutine(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        _nextWaveCoroutine = null;
        StartWave();
    }

    private void DeactivateWaveEnemies()
    {
        if (waves == null || waves.Length == 0) return;

        for (int i = 0; i < waves.Length; i++)
        {
            WaveSettings settings = waves[i];
            Enemy[] waveEnemies = settings != null ? settings.Enemies : null;
            if (waveEnemies == null || waveEnemies.Length == 0) continue;

            for (int j = 0; j < waveEnemies.Length; j++)
            {
                Enemy target = waveEnemies[j];
                if (target == null) continue;

                AiController aiController = target.GetComponent<AiController>();
                if (aiController != null) aiController.enabled = false;
                SetHealthBarVisible(target, false);
                target.gameObject.SetActive(false);
            }
        }
    }

    private void SetAllWaveEnemyHealthBars(bool visible)
    {
        if (waves == null) return;
        for (int i = 0; i < waves.Length; i++)
        {
            Enemy[] waveEnemies = waves[i] != null ? waves[i].Enemies : null;
            if (waveEnemies == null) continue;
            for (int j = 0; j < waveEnemies.Length; j++)
            {
                if (waveEnemies[j] != null) SetHealthBarVisible(waveEnemies[j], visible);
            }
        }
    }

    private static void SetHealthBarVisible(Enemy enemy, bool visible)
    {
        if (enemy == null) return;
        var billboards = enemy.GetComponentsInChildren<BillboardUI>(true);
        if (billboards == null || billboards.Length == 0) return;

        for (int i = 0; i < billboards.Length; i++)
        {
            var go = billboards[i].gameObject;
            if (go.activeSelf != visible) go.SetActive(visible);
        }
    }
}

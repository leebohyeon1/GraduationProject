using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어 레이어 감지 시 escape를 활성화하고, 웨이브 단위로 몬스터를 소환/진행하는 트리거 스크립트입니다.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class MonsterWave : MonoBehaviour
{
    [Serializable]
    private class WaveSettings
    {
        [SerializeField] private Enemy[] enemies;

        public Enemy[] Enemies => enemies;
    }

    [Header("Escape")]
    [SerializeField] private GameObject escape;

    [Header("Trigger")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Wave")]
    [SerializeField] private WaveSettings[] waves;

    [SerializeField] private string spawnFeedbackName;
    [SerializeField] private string spawnAnimationTrigger;

    [SerializeField] private float nextWaveDelay = 2f;

    private BoxCollider _boxCollider;
    private int _currentWaveIndex;
    private int _aliveInCurrentWave;
    private bool _isRunning;
    private Coroutine _nextWaveCoroutine;
    Action<Enemy> wave;
    [SerializeField] private float feedbackDelay = 0.5f;
    float minDistance;
    

    private void Reset()
    {
        var box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
        if (!_boxCollider.isTrigger)
        {
            _boxCollider.isTrigger = true;
            Debug.Log($"[EscapeWaveSpawner] {name}: BoxCollider.isTrigger를 true로 설정했습니다.");
        }

        DeactivateWaveEnemies();

        wave += waveAiController;
        wave += waveTrigger;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isRunning)
        {
            return;
        }

        if (!IsInLayerMask(other.gameObject.layer, playerLayer))
        {
            return;
        }

        _isRunning = true;
        _boxCollider.enabled = false;
        _currentWaveIndex = 0;

        Debug.Log($"[EscapeWaveSpawner] {name}: Player 감지. 웨이브 진행을 시작합니다.");

        if (escape != null)
        {
            escape.SetActive(true);
            Debug.Log($"[EscapeWaveSpawner] {name}: escape 활성화.");
        }
        else
        {
            Debug.LogWarning($"[EscapeWaveSpawner] {name}: escape가 비어있습니다.");
        }

        StartWave();
    }
    void waveAiController(Enemy target)
    {
        target.GetComponent<AiController>().enabled = true;
    }
    void waveTrigger(Enemy target)
    {
        EnemyAnimationBridge animationBridge = target.GetComponent<EnemyAnimationBridge>();
            if (animationBridge != null)
            {
                animationBridge.TriggerEvent(spawnAnimationTrigger, feedbackDelay);
            }
    }
    private void StartWave()
    {
        if (waves == null || waves.Length <= 0)
        {
            Debug.LogWarning($"[EscapeWaveSpawner] {name}: waves가 비어있습니다. 웨이브를 진행할 수 없습니다.");
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

        if (waveEnemies != null && waveEnemies.Length > 0)
        {
            _aliveInCurrentWave = 0;
            Debug.Log($"[EscapeWaveSpawner] {name}: Wave {_currentWaveIndex + 1}/{waves.Length} 시작. 기존 몬스터 활성화");

            for (int i = 0; i < waveEnemies.Length; i++)
            {
                Enemy target = waveEnemies[i];
                if (target == null)
                {
                    continue;
                }
                Vector3 playerPos = target.player.transform.position;
                float dist = Vector3.Distance(target.transform.position, playerPos);
                if (dist < minDistance)
                {
                    // 플레이어로부터 소환 지점까지의 방향
                    Vector3 awayDir = (target.transform.position - playerPos).normalized;
                    

                    // 최소 거리만큼 떨어진 위치로 재설정
                    target.transform.position = playerPos + (awayDir * minDistance);
                }
                target.gameObject.SetActive(true);
                if (!string.IsNullOrEmpty(spawnFeedbackName) || !string.IsNullOrEmpty(spawnAnimationTrigger))
                {
                    {
                        Enemy_AnimationEventHandler animationHandler = target.GetComponent<Enemy_AnimationEventHandler>();
                        if (animationHandler != null)
                        {
                            animationHandler.PlayFeedback(spawnFeedbackName);
                        }
                    }
                    
                }
                StartCoroutine(Timer(feedbackDelay, () => wave?.Invoke(target)));



                EnemyHealth health = target.EnemyHealth;
                if (health == null)
                {
                    health = target.GetComponent<EnemyHealth>();
                }

                if (health == null)
                {
                    Debug.LogWarning($"[EscapeWaveSpawner] {name}: 활성화 대상 Enemy({target.name})에 EnemyHealth가 없습니다.");
                    continue;
                }

                _aliveInCurrentWave++;

                Action handler = null;
                handler = () =>
                {
                    health.OnDied -= handler;
                    HandleEnemyDied();
                };
                health.OnDied += handler;
            }

            if (_aliveInCurrentWave <= 0)
            {
                Debug.LogWarning($"[EscapeWaveSpawner] {name}: Wave {_currentWaveIndex + 1}에 유효한 몬스터가 없어 즉시 다음 웨이브로 진행합니다.");
                _currentWaveIndex++;
                StartWave();
            }

            return;
        }

        Debug.LogWarning($"[EscapeWaveSpawner] {name}: wave {_currentWaveIndex + 1}에 활성화 대상이 없습니다. 다음 웨이브로 넘어갑니다.");
        _currentWaveIndex++;
        StartWave();
    }
    IEnumerator Timer(float delay, Action onComplete)
    {
        Debug.Log($"[EscapeWaveSpawner] {name}: {delay}초 후에 행동을 실행합니다.");
        yield return new WaitForSeconds(delay);
        Debug.Log($"[EscapeWaveSpawner] {name}: 지연된 행동을 실행합니다.");
        onComplete?.Invoke();
    }
    private void HandleEnemyDied()
    {
        _aliveInCurrentWave = Mathf.Max(0, _aliveInCurrentWave - 1);
        Debug.Log($"[EscapeWaveSpawner] {name}: Enemy 사망. 현재 Wave {_currentWaveIndex + 1} 남은 수 {_aliveInCurrentWave}");

        if (_aliveInCurrentWave == 0)
        {
            _currentWaveIndex++;
            StartNextWaveWithDelay();
        }
    }

    private void FinishAllWaves()
    {
        Debug.Log($"[EscapeWaveSpawner] {name}: 다음 웨이브가 없어 웨이브 진행을 종료합니다.");

        if (escape != null)
        {
            escape.SetActive(false);
            Debug.Log($"[EscapeWaveSpawner] {name}: escape 비활성화.");
        }
    }

    private static bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    private void StartNextWaveWithDelay()
    {
        if (_nextWaveCoroutine != null)
        {
            StopCoroutine(_nextWaveCoroutine);
        }

        float delay = Mathf.Max(0f, nextWaveDelay);
        _nextWaveCoroutine = StartCoroutine(NextWaveDelayRoutine(delay));
    }

    private IEnumerator NextWaveDelayRoutine(float delay)
    {
        Debug.Log($"[EscapeWaveSpawner] {name}: 다음 웨이브까지 {delay}초 대기합니다.");
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        _nextWaveCoroutine = null;
        StartWave();
    }

    private void DeactivateWaveEnemies()
    {
        if (waves == null || waves.Length == 0)
        {
            return;
        }

        for (int i = 0; i < waves.Length; i++)
        {
            WaveSettings settings = waves[i];
            Enemy[] waveEnemies = settings != null ? settings.Enemies : null;
            if (waveEnemies == null || waveEnemies.Length == 0)
            {
                continue;
            }

            for (int j = 0; j < waveEnemies.Length; j++)
            {
                Enemy target = waveEnemies[j];
                if (target == null)
                {
                    continue;
                }

                AiController aiController = target.GetComponent<AiController>();
                if (aiController != null)
                {
                    aiController.enabled = false;
                }

                target.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator PlaySpawnFeedbackAfterActivation(Enemy target, string feedbackName, string animationTrigger)
    {
        yield return null;
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            yield break;
        }

        if (!string.IsNullOrEmpty(feedbackName))
        {
            Enemy_AnimationEventHandler animationHandler = target.GetComponent<Enemy_AnimationEventHandler>();
            if (animationHandler != null)
            {
                animationHandler.PlayFeedback(feedbackName);
            }
        }

        if (!string.IsNullOrEmpty(animationTrigger))
        {
            EnemyAnimationBridge animationBridge = target.GetComponent<EnemyAnimationBridge>();
            if (animationBridge != null)
            {
                
                animationBridge.TriggerEvent(spawnAnimationTrigger, feedbackDelay);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyProjectile : MonoBehaviour
{
    private Vector3 _moveDirection;
    private float _moveSpeed;
    private float _lifeRemaining;
    private DamageData _data;
    private bool _isActive;
    private Collider _pendingPlayer;
    private float _contactDeadline;

    public GameObject Owner { get; private set; }
    public float MoveSpeed => _moveSpeed;
    public DamageData Data => _data;
    public Enemy _enemy { get; private set; }

    [SerializeField] private string feedbackname = "null";

    public void Setup(Enemy enemy, Vector3 dir, float speed, GameObject owner, DamageData data = default)
    {
        _enemy = enemy;
        _moveDirection = dir;
        _moveSpeed = speed;
        _data = data;
        _data.SourceEnemy = owner != null && owner.GetComponent<PlayerController>() ? null : enemy;
        Owner = owner;
        if (owner && owner.GetComponent<PlayerController>()) {
            _data.IsReflectedProjectile = true;
            _data.DeathKnockbackForce = Mathf.Max(_data.DeathKnockbackForce,16f);
            _data.DeathKnockbackDuration = .2f;
        }
        _data.AttackerTransform = transform;
        _lifeRemaining = 5f;
        _isActive = true;
        _pendingPlayer=null;
    }

    private void Update()
    {
        if (!_isActive)
        {
            return;
        }

        _lifeRemaining -= Time.deltaTime;
        if(_pendingPlayer) {
            if(Time.time>=_contactDeadline) {
                var health=_pendingPlayer.GetComponentInParent<PlayerHealth>();
                if(health)health.TakeDamage(_data);
                if(_enemy)_enemy.animHandler.PlayFeedbackAtPosition(feedbackname,transform.position,false);
                ReleaseSelf();
            }
            return;
        }
        if (_lifeRemaining <= 0f)
        {
            ReleaseSelf();
            return;
        }

        transform.position += _moveDirection * _moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive)
        {
            return;
        }

        if (Owner != null && (other.gameObject == Owner || other.transform.IsChildOf(Owner.transform)))
        {
            return;
        }

        if (other.TryGetComponent<IDamageable>(out var health))
        {
            if(other.GetComponentInParent<PlayerController>()) {
                if(!_pendingPlayer){_pendingPlayer=other;_contactDeadline=Time.time+.14f;}
                return;
            }
            _enemy.animHandler.PlayFeedbackAtPosition(feedbackname, transform.position);
            health.TakeDamage(_data);
            AttackOutcomeRecorder.RecordSuccessfulHit(_enemy?._aiController?._aiBrain?.blackboard);
            ReleaseSelf();
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            _enemy.animHandler.PlayFeedbackAtPosition(feedbackname, transform.position);
            ReleaseSelf();
        }
    }

    private void OnDisable()
    {
        _isActive = false;
        _lifeRemaining = 0f;
        Owner = null;
        _enemy = null;
    }

    private void ReleaseSelf()
    {
        if (!_isActive)
        {
            return;
        }

        _isActive = false;
        ProjectilePoolManager.ReleaseProjectile(gameObject);
    }
}

public static class ProjectilePoolManager
{
    private static readonly Dictionary<int, ObjectPool<GameObject>> Pools = new();
    private static readonly Dictionary<int, GameObject> Prefabs = new();
    private static readonly Dictionary<GameObject, int> InstanceKeys = new();

    public static GameObject GetProjectile(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            return null;
        }

        int key = prefab.GetInstanceID();
        if (!Pools.TryGetValue(key, out ObjectPool<GameObject> pool))
        {
            Prefabs[key] = prefab;
            pool = CreatePool(key);
            Pools[key] = pool;
        }

        GameObject instance = pool.Get();
        // Scene unload can destroy inactive pooled objects; discard stale entries.
        while (!instance) instance = pool.Get();
        ResetPhysics(instance);
        instance.transform.SetPositionAndRotation(position, rotation);
        return instance;
    }

    public static void ReleaseProjectile(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        if (InstanceKeys.TryGetValue(instance, out int key) && Pools.TryGetValue(key, out ObjectPool<GameObject> pool))
        {
            pool.Release(instance);
            return;
        }

        Object.Destroy(instance);
    }

    private static ObjectPool<GameObject> CreatePool(int key)
    {
        return new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject obj = Object.Instantiate(Prefabs[key]);
                InstanceKeys[obj] = key;
                return obj;
            },
            actionOnGet: obj =>
            {
                if (obj == null)
                {
                    return;
                }

                obj.transform.SetParent(null);
                obj.SetActive(true);
            },
            actionOnRelease: obj =>
            {
                if (obj == null)
                {
                    return;
                }

                ResetPhysics(obj);
                obj.transform.SetParent(null);
                obj.SetActive(false);
            },
            actionOnDestroy: obj =>
            {
                if (obj == null)
                {
                    return;
                }

                InstanceKeys.Remove(obj);
                Object.Destroy(obj);
            },
            collectionCheck: true,
            defaultCapacity: 8,
            maxSize: 128);
    }

    private static void ResetPhysics(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

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
    // The same prefab may be used by multiple additive scenes. Each scene owns
    // its pool so unloading one scene cannot leave stale objects or clear another.
    private static readonly Dictionary<long, ObjectPool<GameObject>> Pools = new();
    private static readonly Dictionary<long, GameObject> Prefabs = new();
    private static readonly Dictionary<GameObject, long> InstanceKeys = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetPools()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        foreach (var pool in Pools.Values) pool.Clear();
        Pools.Clear();
        Prefabs.Clear();
        InstanceKeys.Clear();
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        var keys = new List<long>();
        foreach (var key in Pools.Keys)
            if ((int)(key >> 32) == scene.handle) keys.Add(key);

        foreach (var key in keys)
        {
            Pools[key].Clear();
            Pools.Remove(key);
            Prefabs.Remove(key);
        }

        // Unity destroys active instances as well as inactive pool contents when
        // unloading their scene; remove their managed wrappers from the map too.
        var staleInstances = new List<GameObject>();
        foreach (var pair in InstanceKeys)
            if ((int)(pair.Value >> 32) == scene.handle || !pair.Key)
                staleInstances.Add(pair.Key);
        foreach (var instance in staleInstances) InstanceKeys.Remove(instance);
    }

    public static GameObject GetProjectile(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            return null;
        }

        long key = ((long)(int)SceneManager.GetActiveScene().handle << 32) | (uint)prefab.GetInstanceID();
        if (!Pools.TryGetValue(key, out ObjectPool<GameObject> pool))
        {
            Prefabs[key] = prefab;
            pool = CreatePool(key);
            Pools[key] = pool;
        }

        GameObject instance = pool.Get();
        // Scene unload can destroy inactive pooled objects; discard stale entries.
        while (!instance)
        {
            if (!ReferenceEquals(instance, null)) InstanceKeys.Remove(instance);
            instance = pool.Get();
        }
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

        if (InstanceKeys.TryGetValue(instance, out long key) && Pools.TryGetValue(key, out ObjectPool<GameObject> pool))
        {
            pool.Release(instance);
            return;
        }

        Object.Destroy(instance);
    }

    private static ObjectPool<GameObject> CreatePool(long key)
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
                if (!ReferenceEquals(obj, null)) InstanceKeys.Remove(obj);
                if (obj == null)
                {
                    return;
                }

                Object.Destroy(obj);
            },
            collectionCheck: true,
            defaultCapacity: 8,
            maxSize: 128);
    }

    private static void ResetPhysics(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}

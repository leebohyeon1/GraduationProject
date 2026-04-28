using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Vector3 _moveDirection;
    private float _moveSpeed;
    private DamageData _data;

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
        Owner = owner;
        _data.AttackerTransform = transform;

        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        transform.position += _moveDirection * _moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Owner != null && other.gameObject == Owner.gameObject)
        {
            return;
        }

        if (other.TryGetComponent<IDamageable>(out var health))
        {
            _enemy.animHandler.PlayFeedbackAtPosition(feedbackname, transform.position);
            health?.TakeDamage(_data);
            AttackOutcomeRecorder.RecordSuccessfulHit(_enemy?._aiController?._aiBrain?.blackboard);
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            _enemy.animHandler.PlayFeedbackAtPosition(feedbackname, transform.position);
            Destroy(gameObject);
        }
    }
}

using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Vector3 _moveDirection;
    private float _moveSpeed;
    private DamageData _data;
    public GameObject Owner { get; private set; }

    public float MoveSpeed => _moveSpeed;
    public DamageData Data => _data;
    
    public Enemy _enemy{get; private set;}

    [SerializeField] private string feedbackname = "null"; // 피격 효과 프리팹
    public void Setup(Enemy enemy, Vector3 dir, float speed, GameObject owner, DamageData data = default)
    {
        _enemy = enemy;
        _moveDirection = dir;
        _moveSpeed = speed;
        _data = data;
        Owner = owner;
        _data.AttackerTransform = this.transform;
        
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        transform.position += _moveDirection * _moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == Owner.gameObject) return;
        Debug.Log("충돌 감지: " + other.name);
        if (other.TryGetComponent<IDamageable>(out var health))
        {
            _enemy.animHandler.PlayFeedbackAtPosition(feedbackname, transform.position);

            Debug.Log("투사체 명중!");
            _enemy.animHandler.PlayFeedbackAtPosition(feedbackname, transform.position);
            Destroy(gameObject); 
            health?.TakeDamage(_data);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Wall") )
        {
            _enemy.animHandler.PlayFeedbackAtPosition(feedbackname, transform.position);

            Destroy(gameObject); // 벽에 닿으면 삭제
        }
    }
    
}
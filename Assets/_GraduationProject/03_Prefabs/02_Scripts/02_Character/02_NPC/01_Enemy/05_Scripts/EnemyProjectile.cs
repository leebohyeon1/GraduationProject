using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Vector3 _moveDirection;
    private float _moveSpeed;
    private DamageData _data;
    public GameObject Owner { get; private set; }

    public float MoveSpeed => _moveSpeed;
    public DamageData Data => _data;

    public void Setup(Vector3 dir, float speed, GameObject owner, DamageData data = default)
    {
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
            Debug.Log("투사체 명중!");
            health?.TakeDamage(_data);
            Destroy(gameObject); 
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Wall") )
        {
            Destroy(gameObject); // 벽에 닿으면 삭제
        }
    }
}
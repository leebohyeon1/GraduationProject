using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Vector3 moveDirection;
    private float moveSpeed;
    private DamageData _data;
    private Enemy _owner;

    public void Setup(Vector3 dir, float speed,Enemy owner, DamageData data = default)
    {
        moveDirection = dir;
        moveSpeed = speed;
        this._data = data;
        this._owner = owner;
        _data.AttackerTransform = this.transform;
        
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == _owner.gameObject) return;
        Debug.Log("충돌 감지: " + other.name);
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어 명중!");
            other.GetComponent<PlayerHealth>()?.TakeDamage(_data);
            Destroy(gameObject); 
        }
        else if (other.CompareTag("Wall") )
        {
            Destroy(gameObject); // 벽에 닿으면 삭제
        }
    }
}
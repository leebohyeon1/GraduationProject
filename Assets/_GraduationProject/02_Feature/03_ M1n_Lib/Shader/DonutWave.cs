using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위해 추가

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DonutWave : MonoBehaviour
{
    public int segments = 50;
    public float thickness = 0.3f;
    public float speed = 5f;
    public float maxRadius = 10f;
    public Material waveMaterial;

    private Mesh mesh;
    private float currentRadius = 1f;
    private Vector3[] vertices;
    private int[] triangles;

    private List<Collider> _hitColliders = new List<Collider>();

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        if (waveMaterial != null)
            GetComponent<MeshRenderer>().material = waveMaterial;

        CreateDonutMesh();
    }

    void Update()
    {
        currentRadius += speed * Time.deltaTime;
        transform.localScale = new Vector3(currentRadius, 1f, currentRadius);


        // 2. 월드 공간에서의 실제 안쪽 반지름과 바깥쪽 반지름을 계산합니다.
        //    (transform.localScale이 커지므로, thickness도 함께 커지는 것을 반영)
        float worldOuterRadius = currentRadius;
        float worldInnerRadius = currentRadius * (1f - thickness);

        // 바깥쪽 반지름 기준으로 충돌체를 찾음.
        Collider[] hits = Physics.OverlapSphere(transform.position, worldOuterRadius);
        foreach (var hit in hits)
        {
            if(_hitColliders.Contains(hit))
                continue; // 이미 처리된 충돌체는 무시

            if (hit.TryGetComponent<IDamageable>(out IDamageable player))
            {
                float distanceToCenter = Vector3.Distance(transform.position, hit.transform.position);

                if (distanceToCenter >= worldInnerRadius)
                {
                    // ToDo: 데미지 하드코딩 되어있음. 추후 수정 필요.
                    player.TakeDamage(5); // 데미지 주기 (공격자 정보가 없으므로 null 전달)
                    Debug.Log("Player Hit by DonutWave");
                    _hitColliders.Add(hit); //중복 공격 방지
                }
            }
        }
        

        if (currentRadius > maxRadius)
            Destroy(gameObject);
    }

    void CreateDonutMesh()
    {
        vertices = new Vector3[segments * 2];
        triangles = new int[segments * 6];
        float angleStep = 2 * Mathf.PI / segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep;
            float xOuter = Mathf.Cos(angle);
            float zOuter = Mathf.Sin(angle);
            vertices[i * 2] = new Vector3(xOuter, 0, zOuter);
            vertices[i * 2 + 1] = new Vector3(xOuter * (1f - thickness), 0, zOuter * (1f - thickness));
        }
        for (int i = 0; i < segments; i++)
        {
            int curOuter = i * 2;
            int curInner = i * 2 + 1;
            int nextOuter = (curOuter + 2) % (segments * 2);
            int nextInner = (curInner + 2) % (segments * 2);
            triangles[i * 6] = curOuter;
            triangles[i * 6 + 1] = nextOuter;
            triangles[i * 6 + 2] = nextInner;
            triangles[i * 6 + 3] = curOuter;
            triangles[i * 6 + 4] = nextInner;
            triangles[i * 6 + 5] = curInner;
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
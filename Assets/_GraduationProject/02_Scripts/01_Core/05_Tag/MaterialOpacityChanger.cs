using UnityEngine;

/// <summary>
/// 메테리얼의 Opacity(투명도) 값을 변경하기 위한 컴포넌트입니다.
/// TagHandler 등 유니티 이벤트(Unity Event)와 연결하여 쉽게 사용할 수 있습니다.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class MaterialOpacityChanger : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("체크 해제 시 오브젝트가 가진 '모든' 메테리얼의 투명도를 변경합니다.")]
    [SerializeField] private bool _targetSpecificMaterial = false;
    
    [Tooltip("투명도를 변경할 특정 메테리얼의 인덱스 (0부터 시작)")]
    [SerializeField] private int _materialIndex = 0;

    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    /// <summary>
    /// 현재 오브젝트 메테리얼의 Opacity 값을 변경합니다.
    /// 유니티 이벤트에서 호출하여 사용할 수 있습니다.
    /// </summary>
    /// <param name="opacity">변경할 투명도 값 (일반적으로 0.0 ~ 1.0)</param>
    public void SetOpacity(float opacity)
    {
        if (_renderer != null)
        {
            if (_targetSpecificMaterial)
            {
                // 특정 인덱스의 메테리얼만 변경
                if (_materialIndex >= 0 && _materialIndex < _renderer.materials.Length)
                {
                    ChangeOpacity(_renderer.materials[_materialIndex], opacity);
                }
                else
                {
                    Debug.LogWarning($"<color=red>[MaterialOpacityChanger]</color> {gameObject.name}: 설정한 인덱스({_materialIndex})가 범위를 벗어났습니다.");
                }
            }
            else
            {
                // 모든 메테리얼 변경
                foreach (Material mat in _renderer.materials)
                {
                    ChangeOpacity(mat, opacity);
                }
            }
        }
        else
        {
            Debug.LogWarning($"<color=red>[MaterialOpacityChanger]</color> {gameObject.name}: Renderer 컴포넌트를 찾을 수 없습니다.");
        }
    }

    private void ChangeOpacity(Material mat, float opacity)
    {
        // 쉐이더의 프로퍼티 이름이 _Opacity 또는 Opacity인 경우 처리
        if (mat.HasProperty("_Opacity"))
        {
            mat.SetFloat("_Opacity", opacity);
        }
        else if (mat.HasProperty("Opacity"))
        {
            mat.SetFloat("Opacity", opacity);
        }
        else if (mat.HasProperty("_BaseColor")) // URP 기본 쉐이더 대응용 백업
        {
            Color color = mat.GetColor("_BaseColor");
            color.a = opacity;
            mat.SetColor("_BaseColor", color);
        }
    }
}

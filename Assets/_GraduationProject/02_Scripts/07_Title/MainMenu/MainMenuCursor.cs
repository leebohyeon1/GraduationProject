using UnityEngine;
using UnityEngine.UI;

public class MainMenuCursor : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private RectTransform _cursorImage;
    [SerializeField] private float _smoothTime = 0.1f;

    private RectTransform _targetRect;
    private Vector3 _currentVelocity = Vector3.zero;

    // [추가] 마지막으로 선택된 버튼을 저장할 변수 (읽기 전용 프로퍼티)
    public GameObject LastSelectedButton { get; private set; }

    private void Update()
    {
        if (_targetRect == null) return;

        _cursorImage.position = Vector3.SmoothDamp(
            _cursorImage.position,
            _targetRect.position,
            ref _currentVelocity,
            _smoothTime
        );
    }

    /// <summary>
    /// 타겟 설정 (위치 정보 + 버튼 오브젝트 정보)
    /// </summary>
    public void SetTarget(RectTransform targetRect, GameObject buttonObject)
    {
        _targetRect = targetRect;
        LastSelectedButton = buttonObject; // [중요] 여기서 범인을 기억함!

        if (!_cursorImage.gameObject.activeSelf)
        {
            _cursorImage.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 강제 이동 (초기화용)
    /// </summary>
    public void ForcePosition(RectTransform targetRect, GameObject buttonObject)
    {
        _targetRect = targetRect;
        LastSelectedButton = buttonObject; // 여기서도 기억

        if (targetRect != null)
        {
            _cursorImage.position = targetRect.position;
        }

        if (!_cursorImage.gameObject.activeSelf)
            _cursorImage.gameObject.SetActive(true);
    }
}
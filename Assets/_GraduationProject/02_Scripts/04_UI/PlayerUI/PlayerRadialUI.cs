using System.Collections;
using UnityEngine;

public class PlayerRadialUI : MonoBehaviour
{
    [SerializeField] private Player _player;
    private Camera _mainCamera;
    private RectTransform _transform;

    [Header("FollowSetting")]
    [SerializeField] private Vector3 _followOffset;

    [Header("AcitveSetting")]
    [SerializeField] private float _activeDuration;

    private Coroutine _disableCoroutine;

    private void Awake()
    {
        _transform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        _player.Health.OnHealthChanged += HandleOnHealthChanged;
        _player.Stamina.OnStaminaChanged += HandleOnStaminaChanged;
        _mainCamera = Camera.main;

        gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if(gameObject.activeSelf)
        {
            _transform.position = _mainCamera.WorldToScreenPoint(_player.transform.position) + _followOffset;
        }
    }

    private void OnDestroy()
    {
        _player.Health.OnHealthChanged -= HandleOnHealthChanged;
        _player.Stamina.OnStaminaChanged -= HandleOnStaminaChanged;
    }

    /// <summary>
    /// 체력 변경 시 함수 호출
    /// </summary>
    private void HandleOnHealthChanged(int a, int b)
    {
        OnEventActivate((int) a, (int) b);
    }

    /// <summary>
    /// 스테미나 변경 시 함수 호출
    /// </summary>
    private void HandleOnStaminaChanged(float a, float b)
    {
        OnEventActivate(a, b);   
    }

    /// <summary>
    /// 활성화 이벤트
    /// </summary>
    private void OnEventActivate(float a, float b)
    {
        // 코루틴 작동 중일 경우
        if(_disableCoroutine != null)
        {
            StopCoroutine(_disableCoroutine);
        }

        if (!gameObject.activeSelf)
        {   
            gameObject.SetActive(true);
        }

        _disableCoroutine = StartCoroutine(CoDisable());
    }

    private IEnumerator CoDisable()
    {
        yield return new WaitForSeconds(_activeDuration);

        gameObject.SetActive(false);
        _disableCoroutine = null;
    }

}

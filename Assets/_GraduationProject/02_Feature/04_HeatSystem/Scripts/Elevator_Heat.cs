using DG.Tweening;
using UnityEngine;

public class Elevator_Heat : HeatSystem
{
    Sequence DecreaseHeat;
    bool isDecrease = false;
    [SerializeField]float _DecreaseTimer = 5f;
    float _timer;
    [SerializeField] GameObject _elevatorObject;
    [SerializeField]Transform _startTransform;
    [SerializeField] Transform _arriveTransform;

    float curTransformY = 0;

    void Start()
    {
        curTransformY = _startTransform.position.y;
    }
    void Update()
    {
        Decrease();
    }
    public override void ChangeHeat(int amount)
    {
        if (amount == 0 && IsHeatLock) return;
        isDecrease = false;
        int previousTier = GetTier();
        int previousHeat = p_currentHeat;
        p_currentHeat = Mathf.Clamp(p_currentHeat + amount, 0, MaxHeat);
        float heatRatio = (float)p_currentHeat / MaxHeat;
        Debug.Log($"Heat Ratio: {heatRatio}");
        float targetY = Mathf.Lerp(_startTransform.position.y, _arriveTransform.position.y, heatRatio);
        Vector3 targetPos = _elevatorObject.transform.position;
        targetPos.y = targetY;
        // _elevatorObject.transform.DOMoveY(curTransformY, 1f).SetEase(Ease.Linear);
        _elevatorObject.transform.DOKill(); 
        _elevatorObject.transform
        .DOMove(targetPos, 2)
        .SetEase(Ease.OutCubic);
        // _elevatorObject.GetComponent<Rigidbody>().MovePosition(Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.fixedDeltaTime));
        _timer = Time.time + _DecreaseTimer;
        DecreaseHeat?.Kill();
        DecreaseHeat = null;

        if (previousHeat != p_currentHeat)
        {
            TriggerOnHeatChanged(previousHeat);
        }

        int newTier = GetTier();
        if (previousTier != newTier)
        {
            TriggerOnTierChanged(previousTier);
        }

        Debug.Log($"Heat Changed: {previousHeat} -> {p_currentHeat}");
    }

    public int GetTier()
    {
        return -1;
    }

    void Decrease()
    {
        if (_timer < Time.time && DecreaseHeat == null)
        {
            Debug.Log("Decrease Heat");
            Debug.Log(_timer);
            DecreaseHeat = DOTween.Sequence()
            .AppendCallback(() => ChangeHeat(-10))
            .SetDelay(0.2f)
            .SetLoops(-1, LoopType.Restart);
            DecreaseHeat.Play();
        }
        
    }



}
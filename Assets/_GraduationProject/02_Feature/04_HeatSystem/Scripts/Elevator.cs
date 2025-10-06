using UnityEngine;
using DG.Tweening;
using System.Collections;
public class Elevator: HeatSystem
{
    [SerializeField] GameObject _elevatorObject;
    Transform _tempTransform;
    [SerializeField]Transform _startTransform;
    [SerializeField] Transform _arriveTransform;
    [SerializeField] float _arriveTime = 1.0f;

    // private void Update()
    // {
    //     if(CurrentHeat <= 0)
    //         return;
        
    //     Decrease = DOTween.Sequence()
    //         .AppendCallback(DecreaseHeat)
    //         .SetDelay(0.5f)
    //         .SetLoops(-1, LoopType.Restart);

    //     Decrease.Play();
    // }
    public void DecreaseHeat()
    {
        if(CurrentHeat <= 0)
        {
            return;
        }

        SourceMap sourceMap = p_sourceMapDataBase.GetSourceMap("Decrease", -1);
        int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
            
        ChangeHeat(deltaHeat);
    }
    protected override void OverHeat()
    {
        SetHeatLock(true);
        StartCoroutine(ElevatorMove(2f));
}
    IEnumerator ElevatorMove(float time)
    {
        transform.DORotate(new Vector3(0, 0, -30), 1.0f, RotateMode.Fast).SetEase(Ease.InOutSine);
        yield return new WaitForSeconds(time);
        yield return _elevatorObject.transform
        .DOMove(_arriveTransform.position, _arriveTime)
        .SetEase(Ease.Linear)
        .WaitForCompletion();

        ResetElevator();

        yield return new WaitForSeconds(0.5f);
        yield return _elevatorObject.transform
        .DOMove(_startTransform.position, _arriveTime)
        .SetEase(Ease.Linear)
        .WaitForCompletion();
    }
    void ResetElevator()
    {
        transform.DORotate(new Vector3(0, 0, 0), 1.0f, RotateMode.Fast).SetEase(Ease.InOutSine);
        SetHeatLock(false);
        SetHeat(0); 
    }
    
}
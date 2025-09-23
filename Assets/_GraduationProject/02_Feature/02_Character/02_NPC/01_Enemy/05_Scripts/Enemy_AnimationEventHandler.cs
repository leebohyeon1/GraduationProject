using UnityEngine;
public class Enemy_AnimationEventHandler : MonoBehaviour
{
    public bool IsActive { get; private set; }
    public bool IsHitWindowOpen { get; private set; }
    public bool IsActionFinished { get; private set; }
    public bool IsSound { get; private set; }
    public bool CanParry { get; private set; } // 패링 시스템도 여기로 통합

    public void OpenHitWindow()
    {
        IsHitWindowOpen = true;
    }
    public void CloseHitWindow()
    {
        IsHitWindowOpen = false;
    }
     public void FinishAction()
    {
        IsActionFinished = true;
    }

    public void StartSound()
    {
        IsSound = true;
    }

    public void EndSound()
    {
        IsSound = false;
    }

    public void OpenParryWindow()
    {
        CanParry = true;
    }

    public void CloseParryWindow()
    {
        CanParry = false;
    }
    public void ResetAllFlags()
    {
        IsActive = false;
        IsHitWindowOpen = false;
        IsActionFinished = false;
        IsSound = false;
        CanParry = false;
    }
}
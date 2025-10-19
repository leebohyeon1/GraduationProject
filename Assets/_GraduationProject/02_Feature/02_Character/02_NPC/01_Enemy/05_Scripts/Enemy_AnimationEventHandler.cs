using MoreMountains.Feedbacks;
using UnityEngine;
public class Enemy_AnimationEventHandler : MonoBehaviour
{
    public bool IsActive { get; private set; }
    public bool IsHitWindowOpen { get; private set; }
    public bool IsActionFinished { get; private set; }
    public bool IsSound { get; private set; }
    public bool IsSuperArmor { get; private set; }
    public MMF_Player[] attackFeedback;

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
    public void StartSuperArmor()
    {
        IsSuperArmor = true;
    }
    public void EndSuperArmor()
    {
        IsSuperArmor = false;
    }
    public void ResetAllFlags()
    {
        IsActive = false;
        IsHitWindowOpen = false;
        IsActionFinished = false;
        IsSound = false;
    }
    
}
using UnityEngine;

[CreateAssetMenu(fileName = "OnRegisterCutOutTarget", menuName = "Events/OnRegisterCutOutTarget")]
public class OnRegisterCutOutTargetSO : EventSO<CutOutTargetTransform>
{
    
}

public class CutOutTargetTransform
{
    public Transform Target;
    public bool IsRegister;

    public CutOutTargetTransform(Transform target, bool isRegister = true)
    {
        Target = target; 
        IsRegister = isRegister;
    }
}
using UnityEngine;


/// <summary>
/// 투명화 컨트롤러에 탐지 타겟을 등록하는 스크립터블 오브젝트 이벤트
/// </summary>
[CreateAssetMenu(fileName = "OnRegisterSeeThroughTarget", menuName = "Project/Events/OnRegisterSeeThroughTarget")]
public class OnRegisterSeeThroughTargetSO : EventSO<SeeThroughTargetTransform>
{
    
}

public class SeeThroughTargetTransform
{
    public Transform Target;
    public bool IsRegister;

    public SeeThroughTargetTransform(Transform target, bool isRegister = true)
    {
        Target = target; 
        IsRegister = isRegister;
    }
}
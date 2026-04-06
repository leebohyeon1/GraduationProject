using UnityEngine;

// 추상 클래스로 만들어서 단독으로 못 쓰게 함 (반드시 상속받아야 함)
public abstract class TitleView : MonoBehaviour
{
    // 이 뷰가 어떤 상태일 때 보여져야 하는지 정의
    public TitleState TargetState;

    /// <summary>
    /// 화면을 켭니다. (자식에서 override 가능)
    /// </summary>
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 화면을 끕니다.
    /// </summary>
    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}
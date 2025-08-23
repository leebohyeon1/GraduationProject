/// <summary>
/// 회복 효과를 받을 수 있는 오브젝트를 위한 인터페이스입니다.
/// </summary>
public interface IHealable
{
    /// <summary>
    /// 대상의 체력을 회복시킵니다.
    /// </summary>
    /// <param name="healAmount">회복시키고자 하는 양입니다.</param>
    void Heal(float healAmount);
}
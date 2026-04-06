public interface ISpecialAbility
{
    bool AbilityReady { get; }
    void Initialize(Enemy owner);
    void SetAbility(bool value);
}
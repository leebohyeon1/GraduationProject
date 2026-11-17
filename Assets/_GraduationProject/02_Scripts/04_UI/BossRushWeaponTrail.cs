using UnityEngine;

public class BossRushWeaponTrail : MonoBehaviour
{
    public TrailRenderer[] trails = System.Array.Empty<TrailRenderer>();
    public Renderer[] weapons = System.Array.Empty<Renderer>();
    private Enemy _owner;
    private bool _emitting;
    private bool _attackEnded;
    public void BeginAttack() { _attackEnded = false; }
    public void EndAttack() {
        _attackEnded = true;
        SetEmitting(false);
        foreach (var trail in trails) if (trail) trail.Clear();
    }

    private void Awake() { _owner = GetComponent<Enemy>(); }
    private static bool IsRush(AnimatorStateInfo state)
    {
        return state.IsTag("Boss_SB_Attack") &&
            (state.loop || state.normalizedTime < 1f);
    }
    private void LateUpdate()
    {
        bool active = false;
        if (!_attackEnded && _owner && _owner.animator && _owner.CurrentState == EnemyStateController.EnemyState.Attack)
        {
            var animator = _owner.animator;
            active = IsRush(animator.IsInTransition(0)
                ? animator.GetNextAnimatorStateInfo(0)
                : animator.GetCurrentAnimatorStateInfo(0));
        }
        SetEmitting(active);
    }
    public void SetEmitting(bool active)
    {
        bool beginning = active && !_emitting;
        _emitting = active;
        for (int i=0; i<trails.Length; i++)
        {
            if (!trails[i]) continue;
            if (beginning) trails[i].Clear();
            trails[i].emitting = active && i<weapons.Length && weapons[i] &&
                weapons[i].enabled && weapons[i].gameObject.activeInHierarchy && weapons[i].bounds.extents.sqrMagnitude > .04f;
            trails[i].enabled = trails[i].emitting;
        }
    }
    private void OnDisable() { SetEmitting(false); }
}

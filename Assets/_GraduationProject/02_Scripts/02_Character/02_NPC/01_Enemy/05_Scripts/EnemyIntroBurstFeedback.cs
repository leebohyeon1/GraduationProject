using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>Scene-bound presentation only; never opens a hitbox or applies damage.</summary>
public sealed class EnemyIntroBurstFeedback : MonoBehaviour
{
    public EnemySpawnIntro intro;
    public MMF_Player feedback;
    [Range(0f, 1f)] public float normalizedTriggerTime = .35f;
    public Vector3 worldOffset = Vector3.up;
    public bool snapToGround = true;
    public LayerMask groundLayers = (1 << 3) | (1 << 6);
    public float groundSurfaceOffset = .03f;
    public bool Played { get; private set; }
    public bool GroundFound { get; private set; }
    public Vector3 LastGroundPoint { get; private set; }
    public Vector3 LastBurstPosition { get; private set; }
    Enemy _enemy;
    int _stateHash;

    void Awake() { _enemy = GetComponent<Enemy>(); }
    void OnEnable() { Played = false; GroundFound = false; }
    void LateUpdate()
    {
        if (Played || !intro || !intro.Playing || !feedback || !_enemy || !_enemy.animator) return;
        if (_enemy.EnemyHealth != null && _enemy.EnemyHealth.IsDead) return;
        if (_stateHash == 0) _stateHash = Animator.StringToHash(intro.stateName);
        var state = _enemy.animator.GetCurrentAnimatorStateInfo(0);
        if (state.fullPathHash != _stateHash || state.normalizedTime < normalizedTriggerTime) return;
        Played = true;
        Vector3 position = transform.position + worldOffset;
        if (snapToGround)
        {
            Vector3 ground = transform.position + new Vector3(worldOffset.x, 0f, worldOffset.z);
            if (Physics.Raycast(ground + Vector3.up * 2f, Vector3.down, out var hit, 8f, groundLayers, QueryTriggerInteraction.Ignore))
            { ground = hit.point; GroundFound = true; }
            LastGroundPoint = ground;
            position = ground + Vector3.up * groundSurfaceOffset;
            // The copied Gordian feedback uses Transform mode. Move its owned anchor as well as supplying the play position.
            if (feedback.transform != transform && feedback.transform.IsChildOf(transform)) feedback.transform.position = position;
        }
        LastBurstPosition = position;
        feedback.PlayFeedbacks(position);
    }
}

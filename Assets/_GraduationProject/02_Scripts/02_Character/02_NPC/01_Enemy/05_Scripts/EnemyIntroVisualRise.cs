using UnityEngine;

/// <summary>Uses authored discovery curves for a visual entrance without moving the navigation/physics root.</summary>
[DefaultExecutionOrder(10000)]
public sealed class EnemyIntroVisualRise : MonoBehaviour
{
    public Transform modelRoot;
    public Task_Discover_Player motion;
    public float submergedDepth = 4.3f;
    public bool landFromAir;
    public float airborneHeight = 8f;
    public float landingDuration = .36666667f;
    public float CurrentWorldOffset { get; private set; }
    public float Elapsed { get; private set; }
    private EnemySpawnIntro _intro;
    private Enemy _enemy;
    private Vector3 _appliedLocalOffset;
    private float _startedAt;
    private bool _started;

    private void Awake()
    {
        _intro = GetComponent<EnemySpawnIntro>();
        _enemy = GetComponent<Enemy>();
        if (_intro)
        {
            var restore = GetComponent<EnemyIntroPoseRestore>();
            if (!restore) restore = gameObject.AddComponent<EnemyIntroPoseRestore>();
            restore.owner = this;
        }
    }

    private void OnEnable()
    {
        RemoveOffset();
        _started = false;
        Elapsed = 0f;
    }

    private void LateUpdate()
    {
        CurrentWorldOffset = 0f;
        if (!_intro || !_intro.enabled || _intro.Completed || !modelRoot || !modelRoot.parent || !motion) return;
        if (_enemy && _enemy.EnemyHealth != null && _enemy.EnemyHealth.IsDead) return;
        if (_intro.Playing && !_started) { _started = true; _startedAt = Time.time; }
        if (landFromAir)
        {
            // Timeline playback can start before deferred AI initialization completes.
            bool timelinePlaying = _intro.timelineDirector && _intro.timelineDirector.state == UnityEngine.Playables.PlayState.Playing;
            // A Director with wrap mode None resets its time to zero on Stop; never lift the landed model again.
            Elapsed = Mathf.Max(Elapsed, _started || timelinePlaying ? _intro.PlaybackElapsed : 0f);
            float progress = motion.downCurve.Evaluate(Mathf.Clamp01(Elapsed / Mathf.Max(.001f, landingDuration)));
            CurrentWorldOffset = Mathf.LerpUnclamped(airborneHeight, 0f, progress);
        }
        else
        {
            Elapsed = _started ? Mathf.Max(0f, Time.time - _startedAt) : 0f;
            float progress = motion.moveUpDistance > .0001f ? motion.EvaluateRiseOffset(Elapsed) / motion.moveUpDistance : 1f;
            CurrentWorldOffset = Mathf.LerpUnclamped(-submergedDepth, 0f, progress);
        }
        _appliedLocalOffset = modelRoot.parent.InverseTransformVector(Vector3.up * CurrentWorldOffset);
        modelRoot.localPosition += _appliedLocalOffset;
    }

    internal void RemoveOffset()
    {
        if (modelRoot && _appliedLocalOffset != Vector3.zero) modelRoot.localPosition -= _appliedLocalOffset;
        _appliedLocalOffset = Vector3.zero;
    }

    private void OnDisable() { RemoveOffset(); CurrentWorldOffset = 0f; }
    private void OnDestroy() => RemoveOffset();
}

using System;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Playables;

/// <summary>Observes real F activation/Timeline playback without triggering or restarting it.</summary>
[DefaultExecutionOrder(20000)]
public sealed class FIntroLandingRuntimeAudit : MonoBehaviour
{
    private Enemy _enemy;
    private Animator _animator;
    private EnemySpawnIntro _intro;
    private EnemyIntroVisualRise _landing;
    private CharacterController _controller;
    private SkinnedMeshRenderer[] _renderers;
    private readonly Dictionary<SkinnedMeshRenderer, Mesh> _meshes = new();
    private Vector3 _baselinePosition, _center;
    private float _height, _radius, _groundY, _controllerFootAboveGround;
    private bool _groundFound, _tracking, _controllerStable = true;
    private float _minimumOffset = float.PositiveInfinity, _maximumOffset;
    private float _maxRootDrift, _initialBottom, _landedBottom, _finalBottom, _finalOffset;
    private float _landingTime;
    private int _samples;
    private object[] _initialParts, _midParts, _landedParts, _finalParts;
    private GameObject _authoredPose;
    private SkinnedMeshRenderer[] _authoredRenderers;
    private AnimationClip _spawnClip;
    private float _authoredLandingBottom, _landingClipTime, _landingPoseError, _completionRootDrift;
    private bool _animatorEnabledAtLanding, _finalReadyAtLanding;
    private IntroFeedbackCounter _impactFeedback, _roarFeedback;
    public bool MidpointSeen { get; private set; }
    public bool CompletedObserved { get; private set; }
    public bool LandingObserved { get; private set; }

    public static FIntroLandingRuntimeAudit Observe(Enemy enemy)
    {
        var observer = enemy.gameObject.AddComponent<FIntroLandingRuntimeAudit>();
        observer._enemy = enemy;
        observer._landing = enemy.GetComponent<EnemyIntroVisualRise>();
        observer._controller = enemy.GetComponent<CharacterController>();
        if (!observer._landing || !observer._landing.landFromAir || !observer._landing.modelRoot || !observer._landing.motion || !observer._controller)
            throw new InvalidOperationException("F air landing prefab bindings are incomplete.");
        observer._baselinePosition = enemy.transform.position;
        observer._center = observer._controller.center;
        observer._height = observer._controller.height;
        observer._radius = observer._controller.radius;
        observer._renderers = enemy.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        var feedbacks = enemy.GetComponentsInChildren<MMF_Player>(true);
        observer._impactFeedback = new IntroFeedbackCounter(feedbacks.Single(f => f.name == "Monster_e_Attack03_BigImpact_FB"));
        observer._roarFeedback = new IntroFeedbackCounter(feedbacks.Single(f => f.name == "Mosnter_f_Roar_FB"));
        observer.CreateAuthoredPoseReference();
        observer.ResolveGround();
        // The encounter may still be inactive; EnemyIntroAirLanding installs the intro in Awake.
        return observer;
    }

    private void ResolveGround()
    {
        _groundFound = Physics.Raycast(_baselinePosition + Vector3.up * 3f, Vector3.down, out var hit, 8f, (1 << 3) | (1 << 6), QueryTriggerInteraction.Ignore);
        _groundY = _groundFound ? hit.point.y : _baselinePosition.y;
        _controllerFootAboveGround = transform.TransformPoint(_center).y - _height * Mathf.Abs(transform.lossyScale.y) * .5f - _groundY;
    }

    private void LateUpdate()
    {
        if (CompletedObserved || !_landing) return;
        if (!_intro) _intro = GetComponent<EnemySpawnIntro>();
        float feedbackTime = _intro ? _intro.PlaybackElapsed : 0f;
        _impactFeedback?.Sample(feedbackTime);
        _roarFeedback?.Sample(feedbackTime);
        if (!_intro) return;
        bool timelinePlaying = _intro.timelineDirector && _intro.timelineDirector.state == PlayState.Playing;
        if (!_intro.Playing && !_intro.Completed && !timelinePlaying) return;
        if (!_tracking)
        {
            _tracking = true;
            ResolveGround();
            _initialBottom = BakeVisualBounds(out _initialParts).min.y - _groundY;
        }
        _samples++;
        _minimumOffset = Mathf.Min(_minimumOffset, _landing.CurrentWorldOffset);
        _maximumOffset = Mathf.Max(_maximumOffset, _landing.CurrentWorldOffset);
        // Combat is permitted as soon as Completed changes; the first combat frame may legitimately move the root.
        if (!_intro.Completed) _maxRootDrift = Mathf.Max(_maxRootDrift, Vector3.Distance(_baselinePosition, transform.position));
        _controllerStable &= _controller.center == _center && Mathf.Approximately(_height, _controller.height) && Mathf.Approximately(_radius, _controller.radius);
        float fraction = _landing.CurrentWorldOffset / Mathf.Max(.01f, _landing.airborneHeight);
        if (!MidpointSeen && fraction > .2f && fraction < .8f)
        { MidpointSeen = true; BakeVisualBounds(out _midParts); }
        if (!LandingObserved && _landing.Elapsed >= _landing.landingDuration && Mathf.Abs(_landing.CurrentWorldOffset) < .001f)
        {
            _landingTime = _landing.Elapsed;
            _animatorEnabledAtLanding = _animator.enabled;
            _finalReadyAtLanding = _enemy._initializer && _enemy._initializer.IsInitialized("Final");
            _landedBottom = BakeVisualBounds(out _landedParts).min.y - _groundY;
            if (_intro.timelineDirector) _landingClipTime = _intro.PlaybackElapsed;
            else
            {
                var state = _animator.GetCurrentAnimatorStateInfo(0);
                _landingClipTime = state.normalizedTime * state.length;
            }
            _authoredLandingBottom = SampleAuthoredBottom(_landingClipTime);
            _landingPoseError = Mathf.Abs(_landedBottom - _authoredLandingBottom);
            LandingObserved = true;
        }
        if (_intro.Completed)
        {
            CompletedObserved = true;
            _completionRootDrift = Vector3.Distance(_baselinePosition, transform.position);
            _finalOffset = _landing.CurrentWorldOffset;
            _finalBottom = BakeVisualBounds(out _finalParts).min.y - _groundY;
        }
    }

    private sealed class IntroFeedbackCounter
    {
        private readonly MMF_Player _player;
        private readonly int _initial;
        private int _last, _resets;
        private float _firstPlaybackTime = -1f;
        public int Increments { get; private set; }
        public IntroFeedbackCounter(MMF_Player player) { _player = player; _initial = _last = player.PlayCount; }
        public void Sample(float elapsed)
        {
            int current = _player.PlayCount;
            // Player.PlayCount normally persists; also tolerate an initialization resetting a serialized counter to zero.
            int delta = current >= _last ? current - _last : current;
            if (current < _last) _resets++;
            if (delta > 0 && Increments == 0) _firstPlaybackTime = elapsed;
            Increments += delta; _last = current;
        }
        public object Snapshot() => new { feedback = _player.name, initialPlayerPlayCount = _initial,
            finalPlayerPlayCount = _last, playbackIncrements = Increments, counterResets = _resets, firstPlaybackTime = _firstPlaybackTime };
    }

    private void CreateAuthoredPoseReference()
    {
        // The scene component exists before EnemyInitializer assigns Enemy.animator.
        _animator = _enemy.GetComponent<Animator>();
        _spawnClip = _animator.runtimeAnimatorController.animationClips.FirstOrDefault(c => c.name == "Spawn_f");
        if (!_spawnClip) throw new InvalidOperationException("F authored Spawn_f reference clip is missing.");
        _authoredPose = new GameObject("QA_F_AuthoredPose_Only");
        _authoredPose.SetActive(false);
        _authoredPose.hideFlags = HideFlags.HideAndDontSave;
        var transforms = new Dictionary<Transform, Transform> { [transform] = _authoredPose.transform };
        var renderers = new List<SkinnedMeshRenderer>();
        foreach (var source in _renderers)
        {
            var copy = CopyPoseTransform(source.transform, transforms).gameObject.AddComponent<SkinnedMeshRenderer>();
            copy.sharedMesh = source.sharedMesh;
            copy.rootBone = source.rootBone ? CopyPoseTransform(source.rootBone, transforms) : null;
            copy.bones = source.bones.Select(b => b ? CopyPoseTransform(b, transforms) : null).ToArray();
            renderers.Add(copy);
        }
        _authoredRenderers = renderers.ToArray();
        // Player builds require an Animator for SampleAnimation on a non-legacy clip.
        // The reference stays inactive and has no controller, root motion or animation events.
        var sampler = _authoredPose.AddComponent<Animator>();
        sampler.avatar = _animator.avatar;
        sampler.runtimeAnimatorController = null;
        sampler.applyRootMotion = false;
        sampler.fireEvents = false;
    }

    private Transform CopyPoseTransform(Transform source, Dictionary<Transform, Transform> transforms)
    {
        if (transforms.TryGetValue(source, out var copy)) return copy;
        var parent = CopyPoseTransform(source.parent, transforms);
        copy = new GameObject(source.name).transform;
        copy.SetParent(parent, false);
        copy.localPosition = source.localPosition; copy.localRotation = source.localRotation; copy.localScale = source.localScale;
        transforms[source] = copy; return copy;
    }

    private float SampleAuthoredBottom(float clipTime)
    {
        // No Enemy, collider, gameplay scripts or events are cloned; the inactive Animator only permits explicit sampling.
        _authoredPose.transform.SetPositionAndRotation(transform.position, transform.rotation);
        _authoredPose.transform.localScale = transform.lossyScale;
        _spawnClip.SampleAnimation(_authoredPose, Mathf.Clamp(clipTime, 0f, _spawnClip.length));
        float bottom = float.PositiveInfinity;
        foreach (var renderer in _authoredRenderers)
        {
            if (!_meshes.TryGetValue(renderer, out var mesh)) { mesh = new Mesh(); _meshes.Add(renderer, mesh); }
            renderer.BakeMesh(mesh);
            foreach (var vertex in mesh.vertices) bottom = Mathf.Min(bottom, renderer.transform.TransformPoint(vertex).y);
        }
        return bottom - _groundY;
    }

    private Bounds BakeVisualBounds(out object[] parts)
    {
        bool found = false;
        var bounds = new Bounds(transform.position, Vector3.zero);
        var values = new List<object>();
        foreach (var renderer in _renderers)
        {
            if (!renderer || !renderer.enabled) continue;
            if (!_meshes.TryGetValue(renderer, out var mesh)) { mesh = new Mesh(); _meshes.Add(renderer, mesh); }
            renderer.BakeMesh(mesh);
            bool partFound = false; var part = new Bounds();
            foreach (var vertex in mesh.vertices)
            {
                Vector3 world = renderer.transform.TransformPoint(vertex);
                if (!partFound) { part = new Bounds(world, Vector3.zero); partFound = true; } else part.Encapsulate(world);
            }
            if (!partFound) continue;
            if (!found) { bounds = part; found = true; } else bounds.Encapsulate(part);
            values.Add(new { renderer = renderer.name, vertices = mesh.vertexCount, topAboveGround = part.max.y - _groundY, bottomAboveGround = part.min.y - _groundY });
        }
        parts = values.ToArray(); return bounds;
    }

    public object Snapshot() => new {
        enemy = _enemy.name, samples = _samples, MidpointSeen, LandingObserved, CompletedObserved,
        minimumOffset = _minimumOffset, maximumOffset = _maximumOffset, finalOffset = _finalOffset,
        initialBottomAboveGround = _initialBottom, landedBottomAboveGround = _landedBottom, finalBottomAboveGround = _finalBottom,
        authoredLandingBottomAboveGround = _authoredLandingBottom, landingClipTime = _landingClipTime, landingPoseError = _landingPoseError,
        animatorEnabledAtLanding = _animatorEnabledAtLanding, finalInitializationReadyAtLanding = _finalReadyAtLanding,
        groundFound = _groundFound, groundY = _groundY, authoredRootY = _baselinePosition.y, controllerFootAboveGround = _controllerFootAboveGround,
        maxRootDrift = _maxRootDrift, completionRootDrift = _completionRootDrift, controllerStable = _controllerStable, landingTime = _landingTime,
        height = _landing.airborneHeight, duration = _landing.landingDuration, curve = _landing.motion.name,
        introCount = GetComponents<EnemySpawnIntro>().Length, timeline = _intro && _intro.timelineDirector ? _intro.timelineDirector.name : null,
        impactFeedback = _impactFeedback.Snapshot(), roarFeedback = _roarFeedback.Snapshot(),
        boundsSource = "Actual BakeMesh vertices", initialParts = _initialParts, midParts = _midParts, landedParts = _landedParts, finalParts = _finalParts
    };

    public void AssertComplete(Action<object> record)
    {
        bool pass = _samples >= 5 && MidpointSeen && LandingObserved && CompletedObserved &&
            _maximumOffset > _landing.airborneHeight * .5f && _initialBottom > 1f && Mathf.Abs(_finalOffset) < .001f &&
            _maxRootDrift < .05f && _controllerStable && _groundFound && Mathf.Abs(_controllerFootAboveGround) < .3f &&
            _animatorEnabledAtLanding && _landingPoseError < .12f && _finalBottom > -.35f && _finalBottom < .65f &&
            _impactFeedback.Increments == 1 && _roarFeedback.Increments == 1 &&
            _landingTime >= _landing.landingDuration && _landingTime < _landing.landingDuration + .2f && GetComponents<EnemySpawnIntro>().Length == 1;
        record?.Invoke(new { stage = "world-f-landing-result", pass, data = Snapshot() });
        if (!pass) throw new InvalidOperationException("F airborne landing/ground/root/collider assertion failed.");
    }

    private void OnDestroy()
    {
        foreach (var mesh in _meshes.Values) if (mesh) Destroy(mesh);
        _meshes.Clear();
        if (_authoredPose) Destroy(_authoredPose);
    }
}

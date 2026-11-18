using System;
using System.Linq;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>QA observer only; never activates or restarts the encounter.</summary>
[DefaultExecutionOrder(20000)]
public sealed class EIntroRiseRuntimeAudit : MonoBehaviour
{
    private Enemy _enemy;
    private EnemySpawnIntro _intro;
    private EnemyIntroVisualRise _rise;
    private CharacterController _controller;
    private SkinnedMeshRenderer[] _renderers;
    private Vector3 _rootPosition;
    private Vector3 _center;
    private float _height, _radius;
    private bool _tracking;
    private float _minimumOffset = float.PositiveInfinity;
    private float _maximumOffset = float.NegativeInfinity;
    private float _maxRootDrift;
    private bool _controllerStable = true;
    private float _initialTop = float.PositiveInfinity;
    private float _finalBottom;
    private float _finalOffset;
    private int _samples;
    private readonly Dictionary<SkinnedMeshRenderer, Mesh> _bakedMeshes = new();
    private object[] _initialParts, _midParts, _finalParts;
    private EnemyIntroBurstFeedback _burst;
    private bool _groundBurstObserved;
    private float _burstAnchorError = float.PositiveInfinity;
    public bool MidpointSeen { get; private set; }
    public bool CompletedObserved { get; private set; }

    public static EIntroRiseRuntimeAudit Observe(Enemy enemy)
    {
        var observer = enemy.gameObject.AddComponent<EIntroRiseRuntimeAudit>();
        observer._enemy = enemy;
        observer._intro = enemy.GetComponent<EnemySpawnIntro>();
        observer._rise = enemy.GetComponent<EnemyIntroVisualRise>();
        observer._burst = enemy.GetComponent<EnemyIntroBurstFeedback>();
        if (!observer._intro || !observer._rise || !observer._rise.motion || !observer._rise.modelRoot)
            throw new InvalidOperationException("Zone7 E rise binding is incomplete.");
        observer._controller = enemy.GetComponent<CharacterController>();
        if (!observer._controller) throw new InvalidOperationException("Zone7 E character controller missing.");
        observer._center = observer._controller.center;
        observer._height = observer._controller.height;
        observer._radius = observer._controller.radius;
        observer._renderers = enemy.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        return observer;
    }

    private void LateUpdate()
    {
        if (!_intro || !_rise || CompletedObserved) return;
        if (!_intro.Playing && !_intro.Completed) return;
        if (!_tracking)
        {
            _tracking = true; _rootPosition = transform.position;
            _initialTop = BakeVisualBounds(out _initialParts).max.y - transform.position.y;
        }
        _samples++;
        _minimumOffset = Mathf.Min(_minimumOffset, _rise.CurrentWorldOffset);
        _maximumOffset = Mathf.Max(_maximumOffset, _rise.CurrentWorldOffset);
        _maxRootDrift = Mathf.Max(_maxRootDrift, Vector3.Distance(_rootPosition, transform.position));
        _controllerStable &= _controller.center == _center && Mathf.Approximately(_controller.height, _height) && Mathf.Approximately(_controller.radius, _radius);
        if (!_groundBurstObserved && _burst && _burst.Played && _burst.feedback)
        {
            var particleFeedback = _burst.feedback.FeedbacksList.OfType<MMF_ParticlesInstantiation>().FirstOrDefault();
            var particle = particleFeedback?.InstantiatedParticleSystem;
            if (particle && particle.isPlaying && particle.gameObject.activeInHierarchy)
            {
                _groundBurstObserved = true;
                _burstAnchorError = Vector3.Distance(particle.transform.position, _burst.LastBurstPosition);
            }
        }
        float depth = _rise.submergedDepth;
        if (!MidpointSeen && _rise.CurrentWorldOffset > -depth * .8f && _rise.CurrentWorldOffset < -depth * .2f)
        { MidpointSeen = true; BakeVisualBounds(out _midParts); }
        if (_intro.Completed)
        {
            CompletedObserved = true; _finalOffset = _rise.CurrentWorldOffset;
            _finalBottom = BakeVisualBounds(out _finalParts).min.y - transform.position.y;
        }
    }

    private Bounds BakeVisualBounds(out object[] parts)
    {
        var live = _renderers.Where(r => r && r.enabled).ToArray();
        var values = new List<object>();
        bool found = false;
        var bounds = new Bounds(transform.position, Vector3.zero);
        foreach (var renderer in live)
        {
            if (!_bakedMeshes.TryGetValue(renderer, out var mesh))
            { mesh = new Mesh(); _bakedMeshes.Add(renderer, mesh); }
            renderer.BakeMesh(mesh);
            bool partFound = false; var partBounds = new Bounds();
            foreach (var vertex in mesh.vertices)
            {
                Vector3 world = renderer.transform.TransformPoint(vertex);
                if (!partFound) { partBounds = new Bounds(world, Vector3.zero); partFound = true; }
                else partBounds.Encapsulate(world);
            }
            if (!partFound) continue;
            if (!found) { bounds = partBounds; found = true; } else bounds.Encapsulate(partBounds);
            values.Add(new { renderer = renderer.name, vertices = mesh.vertexCount,
                topAboveGround = partBounds.max.y - transform.position.y, bottomAboveGround = partBounds.min.y - transform.position.y });
        }
        parts = values.ToArray();
        return bounds;
    }

    public object Snapshot()
    {
        return new { samples = _samples, minimumOffset = _minimumOffset, maximumOffset = _maximumOffset,
            MidpointSeen, CompletedObserved, maxRootDrift = _maxRootDrift, controllerStable = _controllerStable,
            initialTopAboveGround = _initialTop, finalBottomAboveGround = _finalBottom, finalOffset = _finalOffset,
            boundsSource = "Actual BakeMesh vertices, not cached renderer bounds", initialParts = _initialParts, midParts = _midParts, finalParts = _finalParts,
            groundBurstObserved = _groundBurstObserved, burstAnchorError = _burstAnchorError,
            groundFound = _burst && _burst.GroundFound, groundY = _burst ? _burst.LastGroundPoint.y : 0f,
            burstY = _burst ? _burst.LastBurstPosition.y : 0f,
            curve = _rise.motion.name, _rise.motion.upduration, _rise.motion.moveUpDistance, _rise.submergedDepth,
            currentOffset = _rise.CurrentWorldOffset, _rise.Elapsed, modelY = _rise.modelRoot.position.y, rootY = transform.position.y };
    }

    public void AssertComplete(Action<object> record)
    {
        bool pass = _samples >= 5 && CompletedObserved && MidpointSeen && _minimumOffset <= -_rise.submergedDepth * .9f &&
            Mathf.Abs(_finalOffset) < .001f && _maxRootDrift < .05f && _controllerStable && _initialTop <= .15f &&
            _finalBottom >= -.3f && _finalBottom <= .5f && _groundBurstObserved && _burst.GroundFound && _burstAnchorError < .06f;
        record?.Invoke(new { stage = "world-e-rise-result", pass, data = Snapshot() });
        if (!pass) throw new InvalidOperationException("E underground rise/root/collider restoration assertion failed.");
    }

    private void OnDestroy()
    {
        foreach (var mesh in _bakedMeshes.Values) if (mesh) Destroy(mesh);
        _bakedMeshes.Clear();
    }
}

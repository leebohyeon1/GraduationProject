#if UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using INab.Dissolve;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RenderingMaterialIsolationTests
{
    private GameObject _root;
    private Material _sharedMaterial;

    [UnitySetUp]
    public IEnumerator EnterPlayMode()
    {
        if (!Application.isPlaying)
        {
            yield return new EnterPlayMode();
        }
    }

    [UnityTearDown]
    public IEnumerator ExitPlayMode()
    {
        if (_root != null)
        {
            UnityEngine.Object.Destroy(_root);
        }

        if (_sharedMaterial != null)
        {
            UnityEngine.Object.Destroy(_sharedMaterial);
        }

        yield return null;

        if (Application.isPlaying)
        {
            yield return new ExitPlayMode();
        }
    }

    [UnityTest]
    public IEnumerator SeeThroughObject_UsesPropertyBlockWithoutCloningSharedMaterial()
    {
        Shader shader = Shader.Find("SeeThrough/LeafSeeThroughShader");
        Assert.That(shader, Is.Not.Null);

        _sharedMaterial = new Material(shader);
        _root = new GameObject("SeeThrough material isolation test");
        MeshRenderer renderer = _root.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = _sharedMaterial;

        _root.AddComponent<SeeThroughObject>();
        yield return null;

        Assert.That(renderer.sharedMaterial, Is.SameAs(_sharedMaterial));

        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propertyBlock, 0);
        Assert.That(propertyBlock.GetFloat(Shader.PropertyToID("_Dither")), Is.EqualTo(0f));
        Assert.That(propertyBlock.GetFloat(Shader.PropertyToID("_BaseGlancingAngleCut")), Is.EqualTo(1f));
    }

    [UnityTest]
    public IEnumerator Dissolver_UsesPropertyBlockAndSkipsVfxRenderer()
    {
        Shader shader = Shader.Find("SeeThrough/LeafSeeThroughShader");
        Assert.That(shader, Is.Not.Null);

        _sharedMaterial = new Material(shader);
        _root = new GameObject("Dissolver material isolation test");
        MeshRenderer renderer = _root.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = _sharedMaterial;

        GameObject vfxChild = new GameObject("VFX child");
        vfxChild.transform.SetParent(_root.transform);
        Type visualEffectType = Type.GetType("UnityEngine.VFX.VisualEffect, Unity.VisualEffectGraph.Runtime");
        if (visualEffectType != null)
        {
            vfxChild.AddComponent(visualEffectType);
        }

        Dissolver dissolver = _root.AddComponent<Dissolver>();
        dissolver.FindMaterialsInChildren();
        dissolver.ManualValuesUpdate();
        yield return null;

        Assert.That(renderer.sharedMaterial, Is.SameAs(_sharedMaterial));
        Assert.That(dissolver.materials, Has.Count.EqualTo(1));

        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propertyBlock, 0);
        Assert.That(propertyBlock.GetFloat(Shader.PropertyToID("_DissolveAmount")), Is.EqualTo(1f));
    }

    [UnityTest]
    public IEnumerator Dissolver_ReleasesRuntimeMaterialsRequiredByAutomaticKeywords()
    {
        Shader shader = Shader.Find("SeeThrough/LeafSeeThroughShader");
        Assert.That(shader, Is.Not.Null);

        _sharedMaterial = new Material(shader);
        _root = new GameObject("Dissolver runtime material lifecycle test");
        MeshRenderer renderer = _root.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = _sharedMaterial;

        Dissolver dissolver = _root.AddComponent<Dissolver>();
        dissolver.useAutomaticKeywords = true;
        dissolver.FindMaterialsInChildren();

        Material runtimeMaterial = renderer.sharedMaterial;
        Assert.That(runtimeMaterial, Is.Not.SameAs(_sharedMaterial));

        UnityEngine.Object.Destroy(dissolver);
        yield return null;

        Assert.That(renderer.sharedMaterial, Is.SameAs(_sharedMaterial));
        Assert.That(runtimeMaterial == null, Is.True);
    }
}
#endif

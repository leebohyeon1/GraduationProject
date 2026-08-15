using System.Reflection;
using BehaviorTree;
using NUnit.Framework;
using UnityEngine;

public sealed class AiControllerRuntimeCloneTests
{
    private const BindingFlags InstancePrivate = BindingFlags.Instance | BindingFlags.NonPublic;

    [Test]
    public void EnsureRuntimeAssets_WithNoAttacks_DoesNotCloneTreeTwice()
    {
        GameObject owner = new GameObject("AiControllerRuntimeCloneTests");
        AiController controller = owner.AddComponent<AiController>();
        ActionTree sourceTree = ScriptableObject.CreateInstance<ActionTree>();
        AiControllerTestNode sourceRoot = ScriptableObject.CreateInstance<AiControllerTestNode>();
        sourceTree.rootNode = sourceRoot;

        try
        {
            SetField(controller, "_behaviorTree", sourceTree);
            SetAttackDataSources(controller, new EnemyAttackData[0]);

            Invoke(controller, "EnsureRuntimeAssets");
            ActionTree firstTree = GetField<ActionTree>(controller, "_runtimeBehaviorTree");
            Node firstRoot = firstTree.rootNode;

            Invoke(controller, "EnsureRuntimeAssets");

            Assert.That(GetField<ActionTree>(controller, "_runtimeBehaviorTree"), Is.SameAs(firstTree));
            Assert.That(GetField<ActionTree>(controller, "_runtimeBehaviorTree").rootNode, Is.SameAs(firstRoot));
            Assert.That(controller.inGameenemyAttackDatas, Is.Empty);
        }
        finally
        {
            Invoke(controller, "ReleaseRuntimeAssets");
            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(sourceRoot);
            Object.DestroyImmediate(sourceTree);
        }
    }

    [Test]
    public void ReleaseRuntimeAssets_DestroysOnlyPerEnemyClones()
    {
        GameObject owner = new GameObject("AiControllerRuntimeCloneTests");
        AiController controller = owner.AddComponent<AiController>();
        ActionTree sourceTree = ScriptableObject.CreateInstance<ActionTree>();
        RunSubTreeNode sourceRoot = ScriptableObject.CreateInstance<RunSubTreeNode>();
        AiControllerTestNode runningSubTreeRoot = ScriptableObject.CreateInstance<AiControllerTestNode>();
        EnemyAttackData sourceAttack = ScriptableObject.CreateInstance<EnemyAttackData>();
        sourceAttack.damageData = new DamageData { DamageAmount = 10 };
        sourceTree.rootNode = sourceRoot;

        try
        {
            SetField(controller, "_behaviorTree", sourceTree);
            SetAttackDataSources(controller, new[] { sourceAttack });
            Invoke(controller, "EnsureRuntimeAssets");

            ActionTree runtimeTree = GetField<ActionTree>(controller, "_runtimeBehaviorTree");
            RunSubTreeNode runtimeRoot = (RunSubTreeNode)runtimeTree.rootNode;
            EnemyAttackData runtimeAttack = controller.inGameenemyAttackDatas[0];
            runtimeAttack.damageData = new DamageData { DamageAmount = 99 };
            SetField(runtimeRoot, "_runningSubTreeInstance", runningSubTreeRoot);

            Assert.That(runtimeTree, Is.Not.SameAs(sourceTree));
            Assert.That(runtimeRoot, Is.Not.SameAs(sourceRoot));
            Assert.That(runtimeAttack, Is.Not.SameAs(sourceAttack));
            Assert.That(sourceAttack.damageData.DamageAmount, Is.EqualTo(10));

            Invoke(controller, "ReleaseRuntimeAssets");

            Assert.That(runtimeTree == null, Is.True);
            Assert.That(runtimeRoot == null, Is.True);
            Assert.That(runningSubTreeRoot == null, Is.True);
            Assert.That(runtimeAttack == null, Is.True);
            Assert.That(sourceTree != null, Is.True);
            Assert.That(sourceRoot != null, Is.True);
            Assert.That(sourceAttack != null, Is.True);
            Assert.That(controller.inGameenemyAttackDatas, Is.Null);
        }
        finally
        {
            Invoke(controller, "ReleaseRuntimeAssets");
            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(sourceAttack);
            if (runningSubTreeRoot != null) Object.DestroyImmediate(runningSubTreeRoot);
            Object.DestroyImmediate(sourceRoot);
            Object.DestroyImmediate(sourceTree);
        }
    }

    private static void SetAttackDataSources(AiController controller, EnemyAttackData[] sources)
    {
        FieldInfo field = typeof(AiController).GetField("<enemyAttackDatas>k__BackingField", InstancePrivate);
        Assert.That(field, Is.Not.Null);
        field.SetValue(controller, sources);
    }

    private static void SetField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, InstancePrivate);
        Assert.That(field, Is.Not.Null);
        field.SetValue(target, value);
    }

    private static T GetField<T>(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(fieldName, InstancePrivate);
        Assert.That(field, Is.Not.Null);
        return (T)field.GetValue(target);
    }

    private static void Invoke(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, InstancePrivate);
        Assert.That(method, Is.Not.Null);
        method.Invoke(target, null);
    }
}

public sealed class AiControllerTestNode : Node
{
    protected override NodeState OnUpdate()
    {
        return NodeState.SUCCESS;
    }
}

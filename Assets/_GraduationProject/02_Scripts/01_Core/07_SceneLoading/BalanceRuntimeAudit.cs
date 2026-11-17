using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Opt-in snapshot of assets actually bound to the running player and scene enemies.
/// No AssetDatabase lookup, asset loading, initialization, or gameplay mutation.
/// </summary>
public static class BalanceRuntimeAudit
{
    private const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly FieldInfo HealthStatField = typeof(EnemyHealth).GetField("enemyStat", InstanceFields);
    private static readonly FieldInfo HealthOwnerField = typeof(EnemyHealth).GetField("_owner", InstanceFields);
    private static readonly FieldInfo RewardOverrideField = typeof(Enemy).GetField("_moneyRewardOverride", InstanceFields);

    public static object Capture(PlayerController player)
    {
        PlayerData data = player ? player.RuntimeData : null;
        PlayerDataSO source = data?.BaseData;
        var enemies = UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(enemy => enemy.gameObject.scene.IsValid() && enemy.gameObject.scene.isLoaded)
            .OrderBy(enemy => enemy.gameObject.scene.name).ThenBy(enemy => HierarchyPath(enemy.transform))
            .Select(EnemySnapshot).ToArray();
        var bosses = UnityEngine.Object.FindObjectsByType<BossDefeatCinematic>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Select(boss => new { scene = boss.gameObject.scene.name, path = HierarchyPath(boss.transform),
                boss.duration, boss.slowScale, active = boss.isActiveAndEnabled }).ToArray();
        var groups = UnityEngine.Object.FindObjectsByType<GroupAi>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Select(group => new { scene = group.gameObject.scene.name, path = HierarchyPath(group.transform),
                group.MaxAttackTokenCount }).ToArray();

        return new {
            stage = "balance-runtime-audit", utc = DateTime.UtcNow.ToString("O"),
            applicationIsEditor = Application.isEditor, scene = SceneManager.GetActiveScene().name,
            playerFound = player != null,
            playerBoundSO = source ? new {
                source.name, instanceId = source.GetInstanceID(), source.MaxHealth,
                source.MaxPotion, source.PotionHealAmount,
                normalAttacks = source.NormalAttackConfigList?.Select(SourceAttack).ToArray(),
                heavyAttacks = source.HeavyAttackConfigList?.Select(SourceAttack).ToArray(),
                normalCounter = SourceAttack(source.NormalCounterAttackConfig),
                heavyCounter = SourceAttack(source.HeavyCounterAttackConfig.AttackConfig),
                counterDamageMultiply = source.CounterDamageMultiply?.ToArray(),
                counterStiffnessMultiply = source.CounterStiffnessMultiply?.ToArray(),
                counterStackDamageMultipliers = source.CounterStackDamageMultipliers?.ToArray()
            } : null,
            playerRuntime = data == null ? null : new {
                data.CurrentHealth, maxHealth = data.Health?.Value,
                normalAttacks = data.NormalAttacks?.Select(attack => RuntimeAttack(attack)).ToArray(),
                heavyAttacks = data.HeavyAttacks?.Select(attack => RuntimeAttack(attack)).ToArray(),
                normalCounter = RuntimeAttack(data.NormalCounterAttack),
                heavyCounter = RuntimeAttack(data.HeavyCounterAttack),
                counterStackDamageMultipliers = data.CounterStackDamageMultipliers?.Select(stat => stat.Value).ToArray()
            },
            enemies, bosses, groups,
            loadedAssetBundles = AssetBundle.GetAllLoadedAssetBundles().Where(bundle => bundle != null)
                .Select(bundle => bundle.name).OrderBy(name => name).ToArray()
        };
    }

    private static object EnemySnapshot(Enemy enemy)
    {
        // Inactive enemies may not have initialized their cached components yet.
        // Read serialized component references without calling Enemy.Init/Awake.
        var health = enemy.GetComponent<EnemyHealth>();
        var stiffness = enemy.GetComponent<Mon_Stiffness>();
        var healthStat = health && HealthStatField != null ? HealthStatField.GetValue(health) as EnemyStat : null;
        var owner = health && HealthOwnerField != null ? HealthOwnerField.GetValue(health) as Enemy : null;
        bool initialized = owner != null;
        return new {
            scene = enemy.gameObject.scene.name, path = HierarchyPath(enemy.transform),
            active = enemy.gameObject.activeInHierarchy, healthInitialized = initialized,
            healthBoundSO = StatSnapshot(healthStat), rewardBoundSO = StatSnapshot(enemy.enemyStat),
            runtimeHealth = initialized ? (int?)health.CurrentHealth : null,
            runtimeMaxHealth = initialized ? (int?)health.MaxHealth : null,
            stiffnessThreshold = stiffness ? (int?)stiffness.StiffnessThreshold : null,
            currentStiffness = stiffness ? (int?)stiffness.CurrentStiffness : null,
            rewardOverride = RewardOverrideField?.GetValue(enemy)
        };
    }

    private static object StatSnapshot(EnemyStat stat) => stat ? new {
        stat.name, instanceId = stat.GetInstanceID(), stat.Maxhealth, stat.MoneyReward
    } : null;

    private static object SourceAttack(PlayerAttackConfig config) => new {
        type = config.AttackType.ToString(), damage = config.AttackDamage, stiffness = config.Stiffness
    };

    private static object RuntimeAttack(IRuntimeAttackConfig config) => config == null ? null : new {
        type = config.AttackType.ToString(), damage = config.Damage.Value, stiffness = config.Stiffness.Value,
        baseDamage = config.BaseAttackConfig.AttackDamage, baseStiffness = config.BaseAttackConfig.Stiffness
    };

    private static string HierarchyPath(Transform item)
    {
        string path = item.name;
        while (item.parent != null) { item = item.parent; path = item.name + "/" + path; }
        return path;
    }
}

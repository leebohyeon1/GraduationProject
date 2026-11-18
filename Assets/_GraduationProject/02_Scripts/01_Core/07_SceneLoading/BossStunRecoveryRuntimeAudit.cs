using System;
using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>Explicit isolated-session QA. Injects the actual stun API, then observes the real BT/Animator.</summary>
public static class BossStunRecoveryRuntimeAudit
{
    private const string Scope = "Actual boss stun API and natural BT/Animator recovery, with a protected QA player. WeakCounter cooldown is primed only to select the authored stun branch. No damage, HP, duration, animation, AI state or enemy transform assignment; not natural parry input.";
    private static double Now => Time.realtimeSinceStartupAsDouble;
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }

    public static IEnumerator Run(Enemy boss, PlayerController player, Action<object> record)
    {
        var args = SceneLoadingSmokeTest.GetArguments();
        Require(args.Contains("--soom-loading-smoke") && args.Contains("--soom-boss-weak-stun-audit"), "Explicit boss stun audit flags are required.");
        Require(DataManager.Instance && DataManager.Instance.IsTemporaryGame, "Boss stun audit requires the isolated temporary boss session.");
        Require(boss && player && boss.EnemyHealth && !boss.EnemyHealth.IsDead && boss._aiController &&
            boss._aiController._aiBrain != null && boss.ParrySystem && boss.StiffnessSystem, "Live boss initialization is incomplete.");
        var brain = boss._aiController._aiBrain;
        var state = boss._stateController;
        var events = player.Events;
        int attacks = 0, protectedContacts = 0;
        string lastAttack = null;
        int playerHp = player.Health.CurrentHealth, bossHp = boss.EnemyHealth.CurrentHealth;
        string sourcePlayer = JsonUtility.ToJson(player.Data), sourceEnemy = JsonUtility.ToJson(boss.enemyStat);

        void Protect(ref PlayerDamageContext context)
        {
            context.Data.DamageAmount = 0;
            context.Data.StiffnessAmount = 0;
            context.Data.KnockbackForce = 0;
            context.Data.KnockbackDuration = 0;
            context.HasSuperArmor = true;
            protectedContacts++;
        }
        void OnLog(string message, string stack, LogType type)
        {
            string prefix = "[Attack Node Enter] " + boss.name + " entered attack node for ";
            if (!message.StartsWith(prefix, StringComparison.Ordinal)) return;
            attacks++;
            lastAttack = message.Substring(prefix.Length);
        }
        void Sample(string phase, string testCase)
        {
            var anim = boss.animator.GetCurrentAnimatorStateInfo(0);
            record?.Invoke(new { stage = "boss-stun-recovery-sample", phase, testCase, scope = Scope,
                realTime = Now, scaledTime = Time.time, timeScale = Time.timeScale,
                stun = boss.ParrySystem.CurrentStun.ToString(), exitTime = boss.ParrySystem.StunExitTime,
                state = boss.CurrentState.ToString(), locked = state.IsStateLocked,
                actionFinished = boss.animHandler.IsActionFinished, bridgeAttacking = boss._animationBridge.IsAttacking,
                attackToken = brain.blackboard.GetValueOrDefault(EnemyBlackboardKeys.HasAttackToken, false),
                attackingFlag = brain.blackboard.GetValueOrDefault("IsAttacking", false), combat = brain._isCombat,
                animatorHash = anim.fullPathHash, normalizedTime = anim.normalizedTime,
                weakAnimation = anim.IsName("WeakCounter"), fullAnimation = anim.IsName("Strong_Counter_a") || anim.IsName("Counter_Ing_a") || anim.IsName("CounterWakeUp_a"),
                attacks, lastAttack, distance = Vector3.Distance(player.transform.position, boss.transform.position),
                protectedContacts, playerHp = player.Health.CurrentHealth, bossHp = boss.EnemyHealth.CurrentHealth });
        }

        events.BeforeDamaged += Protect;
        Application.logMessageReceived += OnLog;
        try
        {
            foreach (string testCase in new[] { "weak-between-patterns", "weak-during-pattern", "full-during-pattern" })
            {
                bool full = testCase.StartsWith("full", StringComparison.Ordinal);
                bool between = testCase == "weak-between-patterns";
                double waitStarted = Now;
                bool ready;
                do
                {
                    var anim = boss.animator.GetCurrentAnimatorStateInfo(0);
                    ready = boss.ParrySystem.CurrentStun == StunType.None &&
                        (between ? !state.IsStateLocked && (anim.IsName("Idle_a") || anim.IsName("Move")) :
                            boss.CurrentState == EnemyStateController.EnemyState.Attack);
                    if (ready) break;
                    Require(Now - waitStarted < 35, "Boss never reached the natural setup state: " + testCase);
                    yield return null;
                } while (true);

                Sample("before", testCase);
                // The real tree routes weak stun to a fake attack when this five-second
                // gate is ready. Prime the existing cooldown to exercise its alternative
                // Stunned branch, without altering the authored gate or any attack values.
                if (!full) brain.StartSkillCooldown("WeakCounter");
                float duration = full ? boss.StiffnessSystem.StiffnessDuration : boss.StiffnessSystem.WeakStiffnessDuration;
                float startedScaled = Time.time;
                double startedReal = Now;
                int attacksBefore = attacks;
                if (full) boss.ParrySystem.ApplyStun(duration, true);
                else boss.ParrySystem.ApplyWeakStun(duration);
                Require(boss.ParrySystem.CurrentStun == (full ? StunType.Full : StunType.Weak), "Actual stun API did not enter the requested stun.");
                Sample("entered", testCase);

                double nextSample = Now + .5;
                while (boss.ParrySystem.CurrentStun != StunType.None)
                {
                    if (Now >= nextSample) { Sample("waiting", testCase); nextSample = Now + .5; }
                    bool expired = full ? Time.time - startedScaled > 20 : Time.time > boss.ParrySystem.StunExitTime + 1;
                    if (expired || Now - startedReal > 35)
                    {
                        Sample("failed-to-exit", testCase);
                        throw new InvalidOperationException("Boss stun did not exit: " + testCase);
                    }
                    yield return null;
                }
                float clearedAfterScaled = Time.time - startedScaled;
                double clearedAt = Now;
                Sample("cleared", testCase);
                while (attacks <= attacksBefore)
                {
                    if (Now >= nextSample) { Sample("waiting-for-pattern", testCase); nextSample = Now + 2; }
                    if (Now - clearedAt > 20)
                    {
                        Sample("failed-to-resume", testCase);
                        throw new InvalidOperationException("Boss did not resume a natural attack after stun: " + testCase);
                    }
                    yield return null;
                }
                record?.Invoke(new { stage = "boss-stun-recovery-result", pass = true, testCase, scope = Scope,
                    authoredDuration = duration, clearedAfterScaled, resumedAfterReal = Now - clearedAt,
                    attackCount = attacks - attacksBefore, lastAttack, playerHp = player.Health.CurrentHealth,
                    bossHp = boss.EnemyHealth.CurrentHealth });
                // Let this natural attack complete before selecting the next case.
                yield return null;
            }
            bool unchanged = sourcePlayer == JsonUtility.ToJson(player.Data) && sourceEnemy == JsonUtility.ToJson(boss.enemyStat);
            Require(unchanged && player.Health.CurrentHealth == playerHp && boss.EnemyHealth.CurrentHealth == bossHp,
                "Boss stun audit changed source data or participant health.");
            record?.Invoke(new { stage = "boss-stun-recovery-complete", pass = true, cases = 3, scope = Scope,
                sourceDataUnchanged = unchanged, attacks, protectedContacts });
        }
        finally
        {
            events.BeforeDamaged -= Protect;
            Application.logMessageReceived -= OnLog;
        }
    }
}

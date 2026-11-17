using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class BossQuickStartArena : MonoBehaviour
{
    public Enemy boss;
    public PlayableDirector entrance;
    public PlayableDirector ending;
    public InteractableObject endingTotem;
    private bool _endingStarted;
    private IEnumerator Start()
    {
        if (!DataManager.Instance || !DataManager.Instance.IsTemporaryGame) yield break;
        while (!SceneLoadingManager.Instance || SceneLoadingManager.Instance.IsTeleporting) yield return null;
        if (!boss || !entrance) yield break;
        ActivateParents(boss.transform);
        var initializer = boss.GetComponent<EnemyInitializer>();
        initializer.Reinitialize();
        while (boss && !initializer.IsInitialized("Final")) yield return null;
        if (!boss) yield break;
        boss.EnemyHealth.OnDied += OnBossDefeated;
        ActivateParents(entrance.transform);
        entrance.Play();
        yield return null;
        while (entrance && entrance.state == PlayState.Playing) yield return null;
        if (boss && !boss.EnemyHealth.IsDead) boss._aiController.CombatEnter();
    }
    private static void ActivateParents(Transform target)
    {
        if (target.parent) ActivateParents(target.parent);
        target.gameObject.SetActive(true);
    }
    private void OnBossDefeated()
    {
        if (!_endingStarted) { _endingStarted=true; StartCoroutine(ContinueToEnding()); }
    }
    private IEnumerator ContinueToEnding()
    {
        // Let the existing death feedback release the arena walls and grant Boss_Clear.
        yield return new WaitForSeconds(5f);
        while (boss && boss.animHandler.IsFeedbackPlaying("Die")) yield return null;
        if (!endingTotem || !ending) yield break;
        var player = FindFirstObjectByType<PlayerController>();
        if (player) {
            var controller = player.GetComponent<CharacterController>();
            bool enabled = controller && controller.enabled;
            if (controller) controller.enabled=false;
            player.transform.position=endingTotem.transform.position;
            if (controller) controller.enabled=enabled;
        }
        ActivateParents(endingTotem.transform);
        endingTotem.Interact();
    }
    private void OnDestroy() { if (boss && boss.EnemyHealth != null) boss.EnemyHealth.OnDied -= OnBossDefeated; }
}
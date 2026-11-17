using System.Collections;
using UnityEngine;
public class DragonTotemReward : MonoBehaviour
{
    public static bool TutorialPending;
    private Chest _chest;
    private void Awake() { _chest=GetComponent<Chest>(); _chest.OnInteracted.AddListener(OnReward); }
    private void Start() { if (DataManager.Instance != null && DataManager.Instance.GetGameData().IsChestOpened(_chest.ChestID)) gameObject.SetActive(false); }
    private void OnDestroy() { if (_chest) _chest.OnInteracted.RemoveListener(OnReward); }
    private void OnReward()
    {
        var data=DataManager.Instance.GetGameData();
        if (data.IsFirstVisit("tutorial:skill-unlock")) {
            data.MarkSceneAsVisited("tutorial:skill-unlock");
            TutorialPending = true;
            StartCoroutine(BeginTutorialAfterReward());
        }
        foreach(var c in GetComponentsInChildren<Collider>()) c.enabled=false;
        var player=FindFirstObjectByType<PlayerController>();
        if (player && player.Interact.Interactable == (IInteractable)_chest) player.Interact.SetInteractable(null);
        StartCoroutine(Vanish());
    }
    private IEnumerator BeginTutorialAfterReward()
    {
        yield return null;
        var skill = FindFirstObjectByType<SkillUI>();
        if (skill && TutorialPending) { skill.BeginSelectedTutorial(); TutorialPending=false; }
    }
    private IEnumerator Vanish()
    {
        var renderers=GetComponentsInChildren<Renderer>(); var block=new MaterialPropertyBlock();
        for(float t=0;t<1.2f;t+=Time.unscaledDeltaTime) {
            foreach(var renderer in renderers) { renderer.GetPropertyBlock(block); block.SetFloat("_DissolveAmount",t/1.2f); renderer.SetPropertyBlock(block); }
            yield return null;
        }
        gameObject.SetActive(false);
    }
}

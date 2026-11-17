using System.Collections.Generic;
using Michsky.UI.Dark;
using UnityEngine;

// Keep the trigger alive while its illustrations dissolve, including rapid re-entry.
public class TutorialIllustrationZone : MonoBehaviour
{
    public UIDissolveEffect[] illustrations = System.Array.Empty<UIDissolveEffect>();
    public TotemDestructibleObject obstacle;
    public float dissolveSpeed = 1.5f;
    private readonly HashSet<Collider> _players = new HashSet<Collider>();
    private Collider _zone;
    private void OnEnable() { _zone = GetComponent<Collider>(); SetHidden(); }
    private void OnTriggerEnter(Collider other) { Track(other); }
    private void OnTriggerStay(Collider other) { Track(other); }
    private void Track(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>()) _players.Add(other);
    }
    private void OnTriggerExit(Collider other) { _players.Remove(other); }
    private void Update()
    {
        _players.RemoveWhere(c => !c || !c.enabled || !c.gameObject.activeInHierarchy ||
            (_zone && !_zone.bounds.Intersects(c.bounds)));
        bool visible = _players.Count > 0 && (!obstacle || obstacle.IsDead);
        foreach (var effect in illustrations)
            if (effect) effect.location = Mathf.MoveTowards(effect.location, visible ? 0f : 1f,
                dissolveSpeed * Time.deltaTime);
    }
    private void SetHidden()
    {
        _players.Clear();
        foreach (var effect in illustrations) if (effect) effect.location = 1f;
    }
    private void OnDisable() { SetHidden(); }
}

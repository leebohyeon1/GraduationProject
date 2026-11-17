using UnityEngine;

public sealed class PlayerBuffVisuals : MonoBehaviour
{
    public GameObject armorPrefab;
    public GameObject speedPrefab;
    public GameObject blueFlamePrefab;
    public Transform bladeAnchor;
    public SkinnedMeshRenderer weaponRenderer;
    private PlayerController _player;
    private GameObject _armor, _speed, _flame;
    private bool _armorOn, _speedOn, _flameOn;
    private bool _flameBound;
    private void Awake() { _player = GetComponent<PlayerController>(); }
    private void LateUpdate()
    {
        if (!_player || _player.RuntimeData == null || !_player.Health || _player.Health.IsDead) { HideAll(); return; }
        bool armor = _player.Ability && _player.Ability.HasTag("SuperArmor");
        var attackSpeed = _player.RuntimeData.AttackSpeed;
        bool speed = attackSpeed != null && attackSpeed.Value > attackSpeed.BaseValue + .001f;
        bool flame = _player.Combat && _player.Combat.CounterStacks >= 3;
        SetEffect(ref _armor, ref _armorOn, armor, armorPrefab, transform);
        SetEffect(ref _speed, ref _speedOn, speed, speedPrefab, transform);
        SetEffect(ref _flame, ref _flameOn, flame, blueFlamePrefab, bladeAnchor);
        if (_flame && weaponRenderer && !_flameBound)
        {
            foreach (var effect in _flame.GetComponentsInChildren<HovlStudio.HS_MeshEffect>(true))
                if (effect.selectedObject != weaponRenderer.gameObject)
                {
                    effect.selectedObject = weaponRenderer.gameObject;
                    effect.meshMaterial = null;
                    effect.ApplyMeshEffect();
                }
            _flameBound = true;
        }
    }
    private static void SetEffect(ref GameObject instance, ref bool shown, bool desired, GameObject prefab, Transform anchor)
    {
        if (!prefab || !anchor) return;
        if (desired && !instance) { instance = Instantiate(prefab, anchor, false); instance.SetActive(false); }
        if (instance && shown != desired) instance.SetActive(desired);
        shown = desired;
    }
    private void HideAll()
    {
        if (_armor) _armor.SetActive(false);
        if (_speed) _speed.SetActive(false);
        if (_flame) _flame.SetActive(false);
        _armorOn = _speedOn = _flameOn = false;
    }
    private void OnDisable() => HideAll();
}

using UnityEngine;

public class EnemyInteract : MonoBehaviour,IInteractable
{
    private PlayerController _playerController;
    public bool _isInteracted {get;private set;}= false;


    [Space(20f)]
    [SerializeField] private Transform _interactableUITransform;
    [SerializeField] private InteractableType _interactableType;

    public Transform InteractableUITransform => _interactableUITransform;
    public InteractableType InteractableType => _interactableType;
    [SerializeField] private GameObject spear;
    HpBar _hpBar;

    private void Awake() {
        
        _hpBar = GetComponentInChildren<HpBar>(true);
    }
    void Start()
    {
        _isInteracted = false;
        _interactableType = InteractableType.NPC;
        _hpBar?.gameObject.SetActive(false); // 체력바 비활성화
        if(spear == null) spear = GameObject.Find("a_Spear");
        if (spear != null) spear.SetActive(false);
    }
    public void Interact()
    {
        if (_isInteracted)
        {
            return;
        }
        Debug.Log($"[EnemyInteract] Interacted with {name}");
        _isInteracted = true;
        GetComponent<BoxCollider>().enabled = false; // 상호작용 후 트리거 비활성화
        _hpBar?.gameObject.SetActive(true); // 체력바 활성화
        if(spear != null) spear.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isInteracted)
        {
            return;
        }
        if (other.TryGetComponent<PlayerController>(out _playerController))
        {
            if (_playerController.Interact != null)
            {
                _playerController.Interact.SetInteractable(this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController controller) && controller == _playerController)
        {
            if (_playerController.Interact != null && _playerController.Interact.Interactable == (IInteractable)this)
            {
                _playerController.Interact.SetInteractable(null);
            }

            _playerController = null;
        }
    }
}

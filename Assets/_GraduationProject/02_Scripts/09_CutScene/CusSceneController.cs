using UnityEngine;

public class CusSceneController : MonoBehaviour
{
    [SerializeField] private CutSceneManager _manager;


    private void Start()
    {
        if( _manager == null )
        {        
            // 씬 시작 시 컷씬 자동 재생
            _manager = FindFirstObjectByType<CutSceneManager>();
        }

    }

    public void CutSceneStart()
    {
        _manager.OnCutSceneStart();
    }

    public void CutSceneEnd()
    {
        _manager.OnCutSceneEnd();
    }
}

using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-998)]
public class GamePlayTagManager : MonoBehaviour
{
    public static GamePlayTagManager Instance { get; private set; }

    [SerializeField] private HashSet<GamePlayTagSO> _activeTagList = new HashSet<GamePlayTagSO>();
    public event Action<GamePlayTagSO> UpdateTag;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 데이터베이스에서 저장된 데이터 불러오기
        if (DataManager.Instance.GetGameData() != null)
        {
            foreach (var id in DataManager.Instance.GetGameData().GamePlayTagIdSet)
            {
                AddTag(DataManager.Instance.GetGamePlayTag(id));
            }
        }
    }

    private void OnApplicationQuit()
    {
        UpdateTag = null;
    }

    /// <summary>
    /// 태그 추가 함수
    /// </summary>
    /// <param name="tag">추가할 태그</param>
    public void AddTag(GamePlayTagSO tag)
    {
        Sequence addSequenece = DOTween.Sequence();

        if (!_activeTagList.Contains(tag))
        {
            _activeTagList.Add(tag);

            if (DataManager.Instance != null && DataManager.Instance.GetGameData() != null)
            {
                 DataManager.Instance.GetGameData().AddGamePlayTag(tag.ID);
            }

            UpdateTag?.Invoke(tag);
            Debug.Log("태그 추가");
        }
    }

    /// <summary>
    /// 태그 가지고 있는지 여부 확인 함수
    /// </summary>
    /// <param name="tag">확인할 태그</param>
    /// <returns>소유 여부</returns>
    public bool HasTag(GamePlayTagSO tag)
    {
        return _activeTagList.Contains(tag);
    }

    public bool HasTag(string id)
    {
        if (GetTag(id) != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 태그 반환 함수
    /// </summary>
    /// <param name="tag">확인할 태그</param>
    /// <returns></returns>
    public GamePlayTagSO GetTag(GamePlayTagSO tag)
    {
        if(HasTag(tag))
        {
            return tag;
        }
        else
        {
            return null;
        }
    }

    public GamePlayTagSO GetTag(string id)
    {
        return _activeTagList.FirstOrDefault((tag)=>tag.ID == id);
    }
}

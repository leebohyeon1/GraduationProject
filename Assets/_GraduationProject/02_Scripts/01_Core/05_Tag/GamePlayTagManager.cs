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

    public void Initialize()
    {
        // 데이터베이스에서 저장된 데이터 불러오기
        if (DataManager.Instance != null && DataManager.Instance.GetGameData() != null)
        {
            foreach (var id in DataManager.Instance.GetGameData().GamePlayTagIdSet)
            {
                var tag = DataManager.Instance.GetGamePlayTag(id);
                if (tag != null) _activeTagList.Add(tag);
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
        if (tag == null) return;

        if (!HasTag(tag.ID))
        {
            // 1. 내부 리스트 업데이트
            _activeTagList.Add(tag);

            // 2. 데이터 매니저를 통해 즉시 데이터 저장 (동기성 보장)
            if (DataManager.Instance != null && DataManager.Instance.GetGameData() != null)
            {
                DataManager.Instance.GetGameData().AddGamePlayTag(tag.ID);
            }

            Debug.Log($"<color=cyan>[TagManager]</color> 태그 추가됨: {tag.ID}");

            // 3. 이벤트 발생
            UpdateTag?.Invoke(tag);
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
        return _activeTagList.Any(tag => tag.ID == id);
    }

    /// <summary>
    /// 태그 반환 함수
    /// </summary>
    /// <param name="tag">확인할 태그</param>
    /// <returns></returns>
    public GamePlayTagSO GetTag(GamePlayTagSO tag)
    {
        return HasTag(tag) ? tag : null;
    }

    public GamePlayTagSO GetTag(string id)
    {
        return _activeTagList.FirstOrDefault((tag) => tag.ID == id);
    }
}

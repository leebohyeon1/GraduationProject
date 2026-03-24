using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


/// <summary>
/// 데이터를 관리하는 매니저
/// </summary>
[DefaultExecutionOrder(-999)]
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public List<GameData> DataList = new List<GameData>();

    private GameData _currentGameData = null;
    private int _currentSlotIndex = -1; // 핵심: 현재 플레이 중인 데이터의 리스트 인덱스를 기억

    [Header("Development")]
    [SerializeField] private bool _useDevelopment = false;
    [SerializeField] private int _developementDataSlotIndex = 0;

    [Header("Game Data Library")]
    [SerializeField] private PlayerDataSO _defaultPlayerData;
    [SerializeField] private AbilityDatabaseSO _abilityDatabase; // 스크립터블 오브젝트 기반 데이터베이스
    [SerializeField] private QuestDatabaseSO _questDatabase;
    public QuestDatabaseSO QuestDatabase => _questDatabase;
    [SerializeField] private GamePlayTagDatabaseSO _gamePlayTagDatabase;
    [SerializeField] private DialogueDatabaseSO _dialogueDatabase;
    public DialogueDatabaseSO DialogueDatabase => _dialogueDatabase;

    private string _saveFileName = "AllSaveData.json"; // 파일 이름 변경

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

        LoadGame();


    }

    private void Start()
    {
#if UNITY_EDITOR
        if (_useDevelopment)
        {
            CreateNewGame(_developementDataSlotIndex);

            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                _currentGameData.PlayerData.RespawnPostion = player.transform.position;
                _currentGameData.PlayerData.LastPosition = player.transform.position;
            }
        }
#endif
    }

    private void OnDestroy()
    {
        if (GamePlayTagManager.Instance)
        {
            QuestManager.Instance.QuestCompleted -= OnQuestCompleted;
        }
        if (QuestManager.Instance)
        {
            GamePlayTagManager.Instance.UpdateTag -= OnUpdateTag;
        }
    }

    /// <summary>
    /// 게임 종료 시점에 저장
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveGame();
    }

    /// <summary>
    /// 게임 화면 나갔을 때 저장
    /// </summary>
    /// <param name="pause">멈춘 여부</param>
    private void OnApplicationPause(bool pause)
    {
        // 멈춘 상태면 저장
        if (pause  && SceneLoadingManager.Instance.IsTeleporting)
        {
            SaveGame();
        }
    }

    public GameData GetGameData()
    {
        return _currentGameData;
    }

    //==========================================================================================================================
    // Save Logic ==============================================================================================================
    //==========================================================================================================================
    public void SaveGame()
    {
        // 2. 현재 플레이 중인 데이터가 없다면 새로 생성 (예외 처리)
        if (_currentGameData == null)
        {
            return;
        }
        UpdatePlayerDataFromGame();

        // _currentGameData.LastMainScene = SceneLoadingManager.Instance.CurrentActiveChunkName;
        // 3. CurrentPlayer 데이터를 CurrentGameData에 덮어씌움 (동기화)
        _currentGameData.LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // 저장 시간 갱신

        // 수정된 핵심 로직: Contains 대신 인덱스를 통한 직접 덮어쓰기
        if (_currentSlotIndex != -1 && _currentSlotIndex < DataList.Count)
        {
            // 기존 슬롯에 확실하게 덮어씌움
            DataList[_currentSlotIndex] = _currentGameData;
        }
        else
        {
            // 예외 처리: 인덱스가 -1이거나 범위를 벗어난 경우 (안전망)
            DataList.Add(_currentGameData);
            _currentSlotIndex = DataList.Count - 1;
        }

        SaveDataContainer container = new SaveDataContainer(DataList);

        // 5. JSON 변환 및 저장
        string json = JsonUtility.ToJson(container, true);
        string filePath = Path.Combine(Application.persistentDataPath, _saveFileName);
        File.WriteAllText(filePath, json);

        Debug.Log($"[DataManager] 전체 리스트 저장 완료 ({DataList.Count}개 슬롯). 경로: {filePath}");
    }

    //==========================================================================================================================
    // Load Logic ==============================================================================================================
    //==========================================================================================================================
    public void LoadGame()
    {
        string filePath = Path.Combine(Application.persistentDataPath, _saveFileName);

        if (File.Exists(filePath))
        {
            // 1. 파일 읽기
            string json = File.ReadAllText(filePath);

            // 2. 래퍼 클래스로 역직렬화
            SaveDataContainer container = JsonUtility.FromJson<SaveDataContainer>(json);

            // 3. 리스트 복원
            if (container != null && container.DataList != null)

            {
                DataList = container.DataList;
            }
            else
            {
                DataList = new List<GameData>();
            }

            Debug.Log($"[DataManager] 로드 완료. 총 {DataList.Count}개의 세이브 데이터가 있습니다.");
        }
        else
        {
            Debug.Log("저장된 파일이 없습니다. 리스트를 초기화합니다.");
            DataList = new List<GameData>();
        }
    }

    /// <summary>
    /// 리스트의 특정 인덱스 데이터를 로드하여 게임을 시작
    /// </summary>
    public void SelectSaveData(int index)
    {
        if (index >= 0 && index < DataList.Count)
        {
            _currentGameData = DataList[index];
            _currentSlotIndex = index; // 선택한 슬롯 번호 기억!
            Debug.Log($"{index}번 세이브 데이터를 불러왔습니다.");
        }
    }

    /// <summary>
    /// 새 게임 생성 로직
    /// </summary>
    public void CreateNewGame()
    {
        _currentGameData = new GameData();
        _currentGameData.PlayerData.InitializeFromSO(_defaultPlayerData);
        _currentGameData.PlayerData.RespawnPostion = new Vector3(-157.7f, -0.17f, -162.7f);
        _currentGameData.PlayerData.LastPosition = _currentGameData.PlayerData.RespawnPostion;

        DataList.Add(_currentGameData);
        _currentSlotIndex = DataList.Count - 1; // 방금 추가된 마지막 인덱스를 기억!

        Debug.Log("새로운 게임 데이터를 생성했습니다.");
    }


    /// <summary>
    /// 새 게임 생성 로직
    /// </summary>
    public void CreateNewGame(int index)
    {
        _currentGameData = new GameData();
        _currentGameData.PlayerData.InitializeFromSO(_defaultPlayerData);
        _currentGameData.PlayerData.RespawnPostion = new Vector3(-157.7f, -0.17f, -162.7f);
        _currentGameData.PlayerData.LastPosition = _currentGameData.PlayerData.RespawnPostion;

        DataList[index] = _currentGameData;
        _currentSlotIndex = index;

        Debug.Log("새로운 게임 데이터를 생성했습니다.");
    }


    //==========================================================================================================================
    // Player Data =============================================================================================================
    //==========================================================================================================================

    // 게임상의 최신 데이터를 PlayerData 클래스로 복사
    private void UpdatePlayerDataFromGame()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            return;
        }

        // 위치 저장 (직접 연동되지 않으므로 복사 필요)
        _currentGameData.PlayerData.LastPosition = player.transform.position;

        // 보유한 능력(Ability) 저장
        var abilityComp = player.Ability;
        if (abilityComp)
        {
            _currentGameData.PlayerData.AcquiredAbilityIds.Clear();

            foreach (var ability in abilityComp.ActiveAbilities)
            {
                if (!string.IsNullOrEmpty(ability.Id))
                {
                    _currentGameData.PlayerData.AcquiredAbilityIds.Add(ability.Id);
                }

            }
        }
    }

    /// <summary>
    /// ID로 능력 스크립터블 오브젝트 찾기
    /// </summary>
    public PlayerAbilitySO GetAbility(string id)
    {
        if (_abilityDatabase == null)
        {
            Debug.LogWarning("AbilityDatabaseSO is not assigned in DataManager!");
            return null;
        }

        PlayerAbilitySO ability = _abilityDatabase.GetAbility(id);

        if (ability == null)
        {
            Debug.LogWarning($"Ability ID '{id}' not found in AbilityDatabase.");
        }

        return ability;
    }

    public void ResetPlayer()
    {
        _currentGameData.PlayerData.CurrentHealth = _currentGameData.PlayerData.MaxHealth;
        _currentGameData.PlayerData.LastPosition = _currentGameData.PlayerData.RespawnPostion;   
        _currentGameData.PlayerData.CurrentPotion = _currentGameData.PlayerData.MaxPotion;
    }

    //==========================================================================================================================
    // Quest Data ==============================================================================================================
    //==========================================================================================================================

    public void InitQuestEvent()
    {
        if (QuestManager.Instance)
        {
            QuestManager.Instance.QuestCompleted += OnQuestCompleted;
        }

    }

    public QuestData GetQuestData(int id)
    {
        return _questDatabase.QuestList.Find((data) => data.ID == id);
    }

    private void OnQuestCompleted(QuestData questData)
    {
        _currentGameData.AddQuestID(questData.ID);
    }

    //==========================================================================================================================
    // GamePlaytag Data ========================================================================================================
    //==========================================================================================================================

    public void InitGamePlayTagEvent()
    {
        if (GamePlayTagManager.Instance)
        {
            GamePlayTagManager.Instance.UpdateTag += OnUpdateTag;
        }
    }

    public GamePlayTagSO GetGamePlayTag(string id)
    {
        return _gamePlayTagDatabase.GamePlayTagList.Find((data) => data.ID == id);
    }

    private void OnUpdateTag(GamePlayTagSO tag)
    {
        _currentGameData.AddGamePlayTag(tag.ID);
    }

    //==========================================================================================================================
    // Dialogue Data ========================================================================================================
    //==========================================================================================================================

    public DialogueDataSO GetDialogueGroupData(int groupID)
    {
        // 그룹 아이디가 같으면 리턴
        return _dialogueDatabase.DialogueDataList.Find((data)=>data.DialogueGroupID == groupID);
    }
}

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
        if (_useDevelopment)
        {
            CreateNewGame(_developementDataSlotIndex, SceneLoadingManager.Instance.InitializeScene);
            GamePlayTagManager.Instance.Initialize();
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                _currentGameData.PlayerData.RespawnPosition = player.transform.position;
                if (SceneLoadingManager.Instance.CurrentActiveChunk != null)
                {
                    _currentGameData.PlayerData.RespawnSceneName = SceneLoadingManager.Instance.CurrentActiveChunk.SceneName;
                }
                _currentGameData.PlayerData.LastPosition = player.transform.position;
            }
        }
    }

    private void OnDestroy()
    {
        
        if (QuestManager.Instance)
        {
            QuestManager.Instance.QuestCompleted -= OnQuestCompleted;
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
        // 멈춘 상태면 저장 (텔레포트 중이 아닐 때만)
        if (pause && !SceneLoadingManager.Instance.IsTeleporting)
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
    
    /// <summary>
    /// 현재 게임 상태를 파일로 저장합니다.
    /// </summary>
    public void SaveGame()
    {
        if (_currentGameData == null)
        {
            Debug.LogWarning("[DataManager] 저장할 현재 게임 데이터가 없습니다.");
            return;
        }

        // 1. 게임 내 실시간 상태를 데이터 구조에 반영 (위치 등)
        UpdatePlayerDataFromGame();

        // 2. 저장 시간 갱신
        _currentGameData.LastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // 3. 리스트 내 해당 슬롯 데이터 갱신
        if (_currentSlotIndex != -1 && _currentSlotIndex < DataList.Count)
        {
            DataList[_currentSlotIndex] = _currentGameData;
        }
        else
        {
            DataList.Add(_currentGameData);
            _currentSlotIndex = DataList.Count - 1;
        }

        // 4. JSON 파일 저장
        try
        {
            SaveDataContainer container = new SaveDataContainer(DataList);
            string json = JsonUtility.ToJson(container, true);
            string filePath = Path.Combine(Application.persistentDataPath, _saveFileName);
            File.WriteAllText(filePath, json);
            Debug.Log($"[DataManager] 게임 저장 완료 (슬롯: {_currentSlotIndex}, 경로: {filePath})");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataManager] 저장 중 오류 발생: {e.Message}");
        }
    }

    //==========================================================================================================================
    // Load Logic ==============================================================================================================
    //==========================================================================================================================
    
    /// <summary>
    /// 저장 파일로부터 모든 세이브 데이터를 불러옵니다. (기기 로컬 로드)
    /// </summary>
    public void LoadGame()
    {
        string filePath = Path.Combine(Application.persistentDataPath, _saveFileName);

        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                SaveDataContainer container = JsonUtility.FromJson<SaveDataContainer>(json);

                if (container != null && container.DataList != null)
                {
                    DataList = container.DataList;
                }
                else
                {
                    DataList = new List<GameData>();
                }
                Debug.Log($"[DataManager] 로컬 데이터 로드 완료 ({DataList.Count}개 슬롯)");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] 로드 중 오류 발생: {e.Message}");
                DataList = new List<GameData>();
            }
        }
        else
        {
            Debug.Log("[DataManager] 저장된 파일이 없습니다. 새 리스트로 시작합니다.");
            DataList = new List<GameData>();
        }
    }

    /// <summary>
    /// 특정 슬롯의 데이터를 선택하여 현재 세션으로 로드합니다.
    /// </summary>
    public void SelectSaveData(int index)
    {
        if (index >= 0 && index < DataList.Count)
        {
            _currentGameData = DataList[index];
            _currentSlotIndex = index;

            // JSON 로드 시 끊겼던 ScriptableObject 참조 및 Stat 수치 복구
            _currentGameData.PlayerData.ReloadBaseData(_defaultPlayerData);

            // 다른 매니저들에게 로드 알림
            if (GamePlayTagManager.Instance != null)
                GamePlayTagManager.Instance.Initialize();
            
            Debug.Log($"[DataManager] {index}번 슬롯 데이터 선택 완료");
        }
    }

    /// <summary>
    /// 새 게임 생성 로직
    /// </summary>
    public void CreateNewGame(SceneDataSO startSceneData)
    {
        _currentGameData = new GameData();
        
        // 시작 씬 설정 (인자가 있으면 우선 사용, 없으면 SceneLoadingManager의 기본값 사용)
        SceneDataSO sceneToUse = (startSceneData != null) ? startSceneData : SceneLoadingManager.Instance.InitializeScene;
        
        if (sceneToUse != null)
        {
            _currentGameData.LastMainScene = sceneToUse.SceneName;
            _currentGameData.PlayerData.InitializeFromSO(_defaultPlayerData);
            _currentGameData.PlayerData.RespawnPosition = sceneToUse.DefaultSpawnPosition;
            _currentGameData.PlayerData.RespawnSceneName = sceneToUse.SceneName;
            _currentGameData.PlayerData.LastPosition = _currentGameData.PlayerData.RespawnPosition;
        }
        else
        {
            _currentGameData.LastMainScene = "";
            _currentGameData.PlayerData.InitializeFromSO(_defaultPlayerData);
            _currentGameData.PlayerData.RespawnPosition = Vector3.zero;
            _currentGameData.PlayerData.LastPosition = Vector3.zero;
            Debug.LogWarning("[DataManager] CreateNewGame: 시작 씬 데이터가 없습니다!");
        }

        DataList.Add(_currentGameData);
        _currentSlotIndex = DataList.Count - 1; // 방금 추가된 마지막 인덱스를 기억!
        
        // 매니저들의 상태를 완전히 초기화
        if (GamePlayTagManager.Instance != null)
        {
            GamePlayTagManager.Instance.ClearTags();      // 1. 태그 리스트 비우기
            GamePlayTagManager.Instance.Initialize();     // 2. 빈 데이터로 다시 초기화
        }
        
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ResetQuest();           // 3. 퀘스트 초기화
        }

        Debug.Log("새로운 게임 데이터를 생성했습니다.");
    }


    /// <summary>
    /// 새 게임 생성 로직 (슬롯 지정)
    /// </summary>
    public void CreateNewGame(int index, SceneDataSO startSceneData)
    {
        _currentGameData = new GameData();
        
        SceneDataSO sceneToUse = (startSceneData != null) ? startSceneData : SceneLoadingManager.Instance.InitializeScene;

        if (sceneToUse != null)
        {
            _currentGameData.LastMainScene = sceneToUse.SceneName;
            _currentGameData.PlayerData.InitializeFromSO(_defaultPlayerData);
            _currentGameData.PlayerData.RespawnPosition = sceneToUse.DefaultSpawnPosition;
            _currentGameData.PlayerData.RespawnSceneName = sceneToUse.SceneName;
            _currentGameData.PlayerData.LastPosition = _currentGameData.PlayerData.RespawnPosition;
        }
        else
        {
            _currentGameData.LastMainScene = "";
            _currentGameData.PlayerData.InitializeFromSO(_defaultPlayerData);
            _currentGameData.PlayerData.RespawnPosition = Vector3.zero;
            _currentGameData.PlayerData.LastPosition = Vector3.zero;
            Debug.LogWarning("[DataManager] CreateNewGame: 시작 씬 데이터가 없습니다!");
        }

        if (index >= 0 && index < DataList.Count)
        {
            DataList[index] = _currentGameData;
        }
        else
        {
            DataList.Add(_currentGameData);
            index = DataList.Count - 1;
        }
        
        _currentSlotIndex = index;

        // 매니저들의 상태를 완전히 초기화
        if (GamePlayTagManager.Instance != null)
        {
            GamePlayTagManager.Instance.ClearTags();
            GamePlayTagManager.Instance.Initialize();
        }
        
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ResetQuest();
        }

        Debug.Log($"{index}번 슬롯에 새로운 게임 데이터를 생성했습니다.");
    }


    //==========================================================================================================================
    // Player Data =============================================================================================================
    //==========================================================================================================================

    /// <summary>
    /// 현재 씬의 플레이어 상태를 데이터 객체에 동기화합니다.
    /// </summary>
    private void UpdatePlayerDataFromGame()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null || _currentGameData == null) return;

        // 1. 위치 정보 저장
        _currentGameData.PlayerData.LastPosition = player.transform.position;

        // 2. 능력(Ability) 데이터 동기화
        // PlayerAbility.cs에서 실시간으로 AcquiredAbilityIds를 업데이트하고 있지만,
        // 세이브 파일 쓰기 직전에 최종적으로 리스트를 정합성 있게 맞춥니다.
        var abilityComp = player.Ability;
        if (abilityComp != null)
        {
            // 현재 플레이어가 실제로 들고 있는 스킬들로 세이브 데이터를 갱신합니다.
            // (단, 1초 로딩 지연 시간 중에 저장이 불리는 특수한 상황을 고려하여 
            // 실시간 리스트가 비어있지 않을 때만 갱신하는 안전 장치를 둘 수 있습니다.)
            var activeAbilities = new List<PlayerAbilitySO>(abilityComp.ActiveAbilities);
            
            if (activeAbilities.Count > 0)
            {
                _currentGameData.PlayerData.AcquiredAbilityIds.Clear();
                foreach (var ability in activeAbilities)
                {
                    if (ability != null && !string.IsNullOrEmpty(ability.Id))
                    {
                        if (!_currentGameData.PlayerData.AcquiredAbilityIds.Contains(ability.Id))
                        {
                            _currentGameData.PlayerData.AcquiredAbilityIds.Add(ability.Id);
                        }
                    }
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
        _currentGameData.PlayerData.CurrentHealth = (int)_currentGameData.PlayerData.Health.BaseValue;
        _currentGameData.PlayerData.LastPosition = _currentGameData.PlayerData.RespawnPosition;   
        _currentGameData.PlayerData.CurrentPotion = (int)_currentGameData.PlayerData.Potion.BaseValue;
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

    public GamePlayTagSO GetGamePlayTag(string id)
    {
        return _gamePlayTagDatabase.GamePlayTagList.Find((data) => data.ID == id);
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

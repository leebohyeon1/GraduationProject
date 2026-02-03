using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    [SerializeField] private PlayerDataSO _defaultPlayerDataSO;
    public PlayerData CurrentPlayer = new PlayerData();
    
    [Header("Game Data Library")]
    [SerializeField] private AbilityDatabaseSO _abilityDatabase; // 스크립터블 오브젝트 기반 데이터베이스

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

    private void OnDestroy()
    {
        SaveGame();
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

    public void SaveGame()
    {
        // 실제 게임 월드의 데이터를 currentPlayer 객체에 반영
        UpdatePlayerDataFromGame();

        string json = JsonUtility.ToJson(CurrentPlayer, true);
        string filePath = Path.Combine(Application.persistentDataPath, "PlayerData.json");
        File.WriteAllText(filePath, json);

        Debug.Log("저장 성공! 경로: " + filePath);
    }

    public void LoadGame()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "PlayerData.json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            CurrentPlayer = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log("로드 성공!");
        }
        else
        {
            Debug.Log("저장된 파일이 없어 새로운 데이터를 생성합니다.");
            CurrentPlayer.InitializeFromSO(_defaultPlayerDataSO);
        }
    }

    // 게임상의 최신 데이터를 PlayerData 클래스로 복사
    private void UpdatePlayerDataFromGame()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        // 위치 저장 (직접 연동되지 않으므로 복사 필요)
        CurrentPlayer.x = player.transform.position.x;
        CurrentPlayer.y = player.transform.position.y;
        CurrentPlayer.z = player.transform.position.z;

        // Health, Money, Potion, Stamina, Combat 등은 RuntimeData를 직접 참조하므로 별도 복사 불필요
        
        // 보유한 능력(Ability) 저장
        var abilityComp = player.Ability;
        if (abilityComp)
        {
            CurrentPlayer.AcquiredAbilityIds.Clear();
            foreach (var ability in abilityComp.ActiveAbilities)
            {
                if (!string.IsNullOrEmpty(ability.Id))
                {
                    CurrentPlayer.AcquiredAbilityIds.Add(ability.Id);
                }
            }
        }
    }
}

using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    public PlayerData currentPlayer = new PlayerData();
    
    [Header("Game Data Library")]
    public List<PlayerAbilitySO> abilityDatabase = new List<PlayerAbilitySO>(); // 게임에 존재하는 모든 스킬 리스트

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
    
    /// <summary>
    /// ID로 능력 스크립터블 오브젝트 찾기
    /// </summary>
    public PlayerAbilitySO GetAbility(string id)
    {
        foreach (var ability in abilityDatabase)
        {
            if (ability.Id == id)
            {
                return ability;
            }
        }
        Debug.LogWarning($"Ability ID '{id}' not found in DataManager database.");
        return null;
    }

    public void SaveGame()
    {
        // 실제 게임 월드의 데이터를 currentPlayer 객체에 반영
        UpdatePlayerDataFromGame();

        string json = JsonUtility.ToJson(currentPlayer, true);
        string filePath = Path.Combine(Application.persistentDataPath, "MyGameData.json");
        File.WriteAllText(filePath, json);

        Debug.Log("저장 성공! 경로: " + filePath);
    }

    public void LoadGame()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "MyGameData.json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            currentPlayer = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log("로드 성공!");
        }
        else
        {
            Debug.Log("저장된 파일이 없어 새로운 데이터를 생성합니다.");
            currentPlayer = new PlayerData();
        }
    }

    // 게임상의 최신 데이터를 PlayerData 클래스로 복사
    private void UpdatePlayerDataFromGame()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        // 위치 저장
        currentPlayer.x = player.transform.position.x;
        currentPlayer.y = player.transform.position.y;
        currentPlayer.z = player.transform.position.z;

        // 체력/돈/포션 저장 (각 컴포넌트 접근)
        var health = player.GetComponent<PlayerHealth>();
        if (health)
        {
            currentPlayer.currentHealth = health.CurrentHealth;
            currentPlayer.maxHealth = health.MaxHealth;
        }

        var money = player.GetComponent<PlayerMoney>();
        if (money) currentPlayer.gold = money.CurrentMoney;

        var potion = player.GetComponent<PlayerPotion>();
        if (potion)
        {
            currentPlayer.currentPotion = potion.CurrentPotion;
            currentPlayer.maxPotion = potion.MaxPotion;
        }

        // 스태미나 저장
        var stamina = player.GetComponent<PlayerStamina>();
        if (stamina)
        {
             currentPlayer.currentStamina = stamina.CurrentStamina;
             currentPlayer.maxStamina = stamina.MaxStamina;
        }
        
        // 보유한 능력(Ability) 저장
        var abilityComp = player.GetComponent<PlayerAbility>();
        if (abilityComp)
        {
            currentPlayer.acquiredAbilityIds.Clear();
            foreach (var ability in abilityComp.ActiveAbilities)
            {
                if (!string.IsNullOrEmpty(ability.Id))
                {
                    currentPlayer.acquiredAbilityIds.Add(ability.Id);
                }
            }
        }
    }
}

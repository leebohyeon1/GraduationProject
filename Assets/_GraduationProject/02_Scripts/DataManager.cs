using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    public PlayerData currentPlayer = new PlayerData();

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
        // Note: PlayerStamina 필드 접근은 구현에 따라 public 필드나 프로퍼티가 필요할 수 있습니다.
        
        // 보유한 능력(Ability) 저장
        var abilityComp = player.GetComponent<PlayerAbility>();
        if (abilityComp)
        {
            currentPlayer.acquiredAbilityIds.Clear();
            // PlayerAbility에 현재 보유한 리스트를 가져오는 기능이 필요할 수 있습니다.
            // 여기서는 개념적으로 string ID 리스트를 저장하는 방식을 제안합니다.
        }
    }
}

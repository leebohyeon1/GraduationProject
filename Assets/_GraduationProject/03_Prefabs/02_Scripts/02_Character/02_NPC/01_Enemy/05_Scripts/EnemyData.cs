using System;
using UnityEngine;

public class EnemyData
{
    public Enemy.Enemy_Type EnemyType { get; set; }
    public Enemy.MonsterName MonsterName { get; set; }
    public Vector3 StartPosition { get; set; }
    public Transform LaunchPoint { get; set; }  
    private int _currentStiffness = 4;
    public int CurrentStiffness
    {
        get => _currentStiffness;       
        set => _currentStiffness = Mathf.Clamp(value, 0, 100);
    }   

    public GroupAi GroupAi { get; set; }
    public int SquadSlotIndex { get; set; }
    public bool IsSurrounding { get; set; }
    public bool HasAttackToken { get; set; }
    
    // 컴포넌트 참조 (읽기 전용)
    public PlayerController Player { get; set; }
    
    
    public static EnemyData Create(Enemy enemy)
    {
         if (enemy == null)
            throw new ArgumentNullException(nameof(enemy));
        
        return new EnemyData
        {
            EnemyType = enemy.EnemyType,
            StartPosition = enemy.transform.position,
            Player = GameObject.FindFirstObjectByType<PlayerController>(),
            LaunchPoint = enemy.transform.Find("LaunchPoint"), // 또는 Inspector에서 설정
            GroupAi = enemy.groupAi
        };
    }
    public void ResetForRespawn(Vector3 spawnPosition)
    {
        StartPosition = spawnPosition;
        CurrentStiffness = 4;
        SquadSlotIndex = 0;
        IsSurrounding = false;
        HasAttackToken = false;
    }
    public EnemyData Clone()
    {
        return new EnemyData
        {
            EnemyType = this.EnemyType,
            MonsterName = this.MonsterName,
            StartPosition = this.StartPosition,
            LaunchPoint = this.LaunchPoint,
            CurrentStiffness = this.CurrentStiffness,
            GroupAi = this.GroupAi,
            Player = this.Player
        };
    }
}
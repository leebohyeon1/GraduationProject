using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GroupAi : MonoBehaviour
{
    [Header("Settings")]
    public int MaxAttackTokenCount = 2; // [추가] 동시에 공격 가능한 몬스터 수
    public float updateInterval = 0.1f;
    List<Enemy> enemies = new List<Enemy>();
    public string KEY_TOKEN = "HasAttackToken";
    public string KEY_THREAT = "IsTargetAimingMe";
    public string KEY_TARGET_LOC = "TargetLocation";
    public string KEY_COLLEAGUES = "PeripheralColleagues";
    // [설정] 업데이트 주기 (매 프레임 계산은 낭비일 수 있음)
    private class EnemyCandidate
    {
        public Enemy enemy;
        public bool isThreatened;
        public float distance;
    }

    private float _updateTimer;

    bool CombatGroup = false;
    public void GroupAdd(Enemy enemy)
    {
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
            // 새로 들어온 놈에게도 주변 동료 수 갱신
            UpdateColleaguesCount();
        }
        else
        {
            Debug.LogWarning($"이미 그룹에 존재하는 몬스터 {enemy.gameObject.GetInstanceID()}입니다.");
        }
    }

    public void GroupRemove(Enemy enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
            UpdateColleaguesCount();
            
            // 나갈 때 토큰 반납
            if(enemy != null && enemy._aiController != null)
                enemy._aiController._aiBrain.blackboard.SetValue(KEY_TOKEN, false);
        }
    }
    private void UpdateColleaguesCount()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null)
                enemy._aiController._aiBrain.blackboard.SetValue(KEY_COLLEAGUES, enemies.Count);
        }
    }

    public bool OnlyCowardly()
    {
        if (enemies.Count == 1)
        {
            foreach (var enemy in enemies)
            {
                if (enemy.EnemyType == Enemy.Enemy_Type.Cowardly)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void CombatAll()
    {
        
        CombatGroup = true;
        foreach (var enemy in enemies)
        {
            enemy._aiController.CombatEnter();
        }
        UpdateColleaguesCount(); 
        AssignSlots();
    }

    public void CombatReset()
    {
        CombatGroup = false;
        foreach (var enemy in enemies)
        {
            enemy._aiController.CombatEnter(false);
            // 전투 끝나면 토큰 회수
            enemy._aiController._aiBrain.blackboard.SetValue(KEY_TOKEN, false);
        }
        UpdateColleaguesCount();
    }

    public void AssignSlots()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if(enemies[i].EnemyType != Enemy.Enemy_Type.Cunning)
            {
                return;
            }
            // 2. 나의 고유 번호 (Slot Index)
            enemies[i]._aiController._aiBrain.blackboard.SetValue("SquadSlotIndex", i);
            // 3. 포위 명령
            enemies[i]._aiController._aiBrain.blackboard.SetValue("IsSurrounding", true);
        }
    }
    void Update()
    {
        // 몬스터가 없으면 계산 안 함
        if (enemies.Count == 0) return;
        if(!CombatGroup) return;
        // 일정 주기마다 토큰 갱신
        _updateTimer += Time.deltaTime;
        if (_updateTimer >= updateInterval)
        {
            UpdateAttackToken();
            _updateTimer = 0f;
        }
    }
    private void UpdateAttackToken()
    {
        List<EnemyCandidate> candidates = new List<EnemyCandidate>();

        // 1. 모든 몬스터의 상태(위협, 거리)를 조사하여 후보 리스트 생성
        foreach (var enemy in enemies)
        {
            // 죽거나 비활성화된 놈은 제외
            if (enemy == null || !enemy.gameObject.activeInHierarchy || enemy.EnemyHealth.IsDead) continue;
            
            var blackboard = enemy._aiController._aiBrain.blackboard;

            // 위협 상태 확인
            bool threatened = false;
            if (blackboard.HasKey(KEY_THREAT))
                threatened = blackboard.GetValue<bool>(KEY_THREAT);

            // 거리 계산
            float dist = float.MaxValue;
            if (blackboard.HasKey(KEY_TARGET_LOC))
            {
                dist = Vector3.Distance(enemy.transform.position, blackboard.GetValue<Vector3>(KEY_TARGET_LOC));
            }
            else if (enemy.player != null)
            {
                dist = Vector3.Distance(enemy.transform.position, enemy.player.transform.position);
            }

            candidates.Add(new EnemyCandidate 
            { 
                enemy = enemy, 
                isThreatened = threatened, 
                distance = dist 
            });
        }

        // 2. 우선순위대로 정렬 (Sort)
        // 규칙: 위협받는 놈 우선(내림차순) -> 그 다음 거리 가까운 놈(오름차순)
        candidates.Sort((a, b) => 
        {
            if (a.isThreatened != b.isThreatened)
                return b.isThreatened.CompareTo(a.isThreatened); // true가 먼저 오도록
            
            return a.distance.CompareTo(b.distance); // 거리가 작은 게 먼저 오도록
        });

        // 3. 상위 N명에게 토큰 부여, 나머지는 회수
        int tokenGivenCount = 0;
        foreach (var candidate in candidates)
        {
            bool giveToken = false;

            // 아직 정원이 남았고 + (위협받고 있거나 OR 거리가 공격 범위 내라면)
            // 여기선 단순히 상위 N명에게 무조건 줍니다.
            if (tokenGivenCount < MaxAttackTokenCount)
            {
                giveToken = true;
                tokenGivenCount++;
            }

            // 블랙보드에 반영
            candidate.enemy._aiController._aiBrain.blackboard.SetValue(KEY_TOKEN, giveToken);
        }
    }
}

using UnityEngine;
using System;
using System.Collections.Generic;

public enum TotemType { Object, Obstacle }
public enum TotemState { Idle, Sliding, Hit, Destroyed }

[System.Serializable]
public class TotemData
{
    public TotemType type;
    public Vector2Int startPos; // 초기 그리드 좌표 (0,0) ~ (4,4)
    
    [Header("Object Totem Only")]
    public int maxDurability; // 내구도
    public Vector2Int targetPos; // 목표 좌표
}

[System.Serializable]
public class PuzzleLevelData
{
    public int levelID;
    public List<TotemData> totems; // 맵에 배치된 모든 토템 정보
    public GameObject doorObject; // 성공 시 열릴 문
}

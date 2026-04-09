using System;
using UnityEngine;

/// <summary>
/// 끌 수 있는 오브젝트에 넣는 인터페이스
/// </summary>
public interface IDragable
{
    /// <summary>
    /// 끌기 시작
    /// </summary>
    void Drag();

    /// <summary>
    /// 끌기 종료
    /// </summary>
    void Drop();

    /// <summary>
    /// 드래그 이벤트
    /// </summary>
    event Action<bool> Dragged;
}

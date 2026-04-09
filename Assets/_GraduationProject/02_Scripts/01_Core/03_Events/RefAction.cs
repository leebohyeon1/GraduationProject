using UnityEngine;


/// <summary>
/// 레퍼런스 사용가능한 액션
/// 기존 Action은 Ref가 되지 않는 문제가 있음
/// 클래스를 사용할 경우 GC 발생
/// </summary>
/// <typeparam name="T">제네릭 타입</typeparam>
/// <param name="obj">오브젝트</param>
public delegate void RefAction<T>(ref T obj);

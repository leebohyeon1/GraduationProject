using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TowerHitPuzzle : MonoBehaviour
{
    [SerializeField] private float _resetTime = 5f;

    [SerializeField]private HitTower[] _hitTowers;
    private int _hitTowersCount = 0;
    private bool _isPuzzleCompleted = false;

    private Coroutine _puzzleCoroutine;

    public UnityEvent OnPuzzleCompleted;

    private void Start()
    {
        _hitTowersCount = 0;
        _isPuzzleCompleted = false;

        foreach (var hitTower in _hitTowers)
        {
            hitTower.ResetTower();
        
            hitTower.OnDied += OnTowerDied;
        }
    }

    private void OnDisable()
    {

        foreach (var hitTower in _hitTowers)
        {
            hitTower.ResetTower();

            hitTower.OnDied -= OnTowerDied;
        }
    }

    private void OnTowerDied()
    {           
        // 이미 퍼즐이 클리어된 상태이면 리턴
        if (_isPuzzleCompleted)
        {
            return;
        }

        _hitTowersCount++;

        // 모든 타워가 맞춰졌는지 확인
        if (_hitTowersCount >= _hitTowers.Length)
        {
            Debug.Log("퍼즐 성공!");
            StopCoroutine(_puzzleCoroutine);
            _puzzleCoroutine = null;
            _isPuzzleCompleted = true;

            OnPuzzleCompleted?.Invoke();

            return;
        }

        // 퍼즐이 아직 시작되지 않은 상태면 리셋 타이머 시작
        if (_puzzleCoroutine != null)
        {
            return;
        }

        Debug.Log("퍼즐 시작");
        _puzzleCoroutine = StartCoroutine(StartPuzzle());
    }

    private IEnumerator StartPuzzle()
    {
        yield return new WaitForSeconds(_resetTime);
        Debug.Log("퍼즐 실패!");
        foreach (var hitTower in _hitTowers)
        {
            hitTower.ResetTower();
        }
        _hitTowersCount = 0;
        _puzzleCoroutine = null;
    }
}

using System;
using UnityEngine;

public class CutSceneSkip : MonoBehaviour
{
    [SerializeField] private InputReaderSO _inputReaderSO;

    private void OnEnable()
    {
        _inputReaderSO.SkipStartEvent += OnSkipStart;
        _inputReaderSO.SkipEndEvent += OnSkipEnd;
    }

    private void OnDisable()
    {
        _inputReaderSO.SkipStartEvent -= OnSkipStart;
        _inputReaderSO.SkipEndEvent -= OnSkipEnd;
    }

    private void OnSkipStart()
    {
        throw new NotImplementedException();
    }

    private void OnSkipEnd()
    {
        throw new NotImplementedException();
    }

}

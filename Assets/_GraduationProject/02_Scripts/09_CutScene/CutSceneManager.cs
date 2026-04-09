using System;
using UnityEngine;

public class CutSceneManager : MonoBehaviour
{
    [SerializeField] private InputReaderSO _inputReaderSO;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCutSceneStart()
    {
        _inputReaderSO.SetInputMode(InputReaderSO.InputMode.CutScene);
    }

    public void OnCutSceneEnd()
    {
        _inputReaderSO.SetInputMode(InputReaderSO.InputMode.Gameplay);
    }
}

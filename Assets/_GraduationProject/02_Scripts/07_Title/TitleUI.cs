using System.Collections.Generic;
using UnityEngine;

public class TitleUI : MonoBehaviour
{
    [SerializeField] private TitleManager _titleManager;

    // GameObject가 아니라 우리가 만든 TitleView 리스트를 가짐
    [SerializeField] private List<TitleView> _views;

    private void Start()
    {
        _titleManager.TitleStateChanged += OnTitleStateChanged;

        // 시작할 때 다 끄고 시작하거나 초기화
        foreach (var view in _views) view.Hide();
    }

    private void OnDestroy()
    {
        _titleManager.TitleStateChanged -= OnTitleStateChanged;
    }

    private void OnTitleStateChanged(TitleState state)
    {
        // 리스트를 돌면서 상태에 맞는 뷰만 켜고 나머지는 끔
        foreach (var view in _views)
        {
            if (view.TargetState == state)
            {
                view.Show(); // MainMenuView라면 알아서 버튼 선택하고 커서 맞춤
            }
            else
            {
                view.Hide();
            }
        }
    }
}
using UnityEngine;
using UnityEngine.AddressableAssets; // 어드레서블 기능 사용

[CreateAssetMenu(fileName = "NewSceneData", menuName = "Project/Scene Data")]
public class SceneDataSO : ScriptableObject
{
    [Header("Scene Information")]
    public string SceneName;        // 예: "얼음 동굴"
    [TextArea]
    public string LoadingTip;       // 로딩창에 띄울 팁 텍스트
    public Sprite LoadingBackground;// 로딩창 배경 이미지
    
    [Header("Environment Settings")]
    // ★ 추가된 부분: 이 씬에서 사용할 스카이박스 머티리얼
    public Material skyboxMaterial;
    
    [Header("Addressable Reference")]
    // Build Settings 대신 Addressables의 에셋 레퍼런스를 사용합니다!
    public AssetReference SceneReference;
}
// RadialBlur.cs
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom/Radial Blur", typeof(UniversalRenderPipeline))]
public class RadialBlur : VolumeComponent, IPostProcessComponent
{
    [Tooltip("블러 효과의 강도를 조절합니다.")]
    public ClampedFloatParameter strength = new ClampedFloatParameter(0f, 0f, 1f);

    [Tooltip("블러 효과의 중심점을 화면 기준으로 설정합니다 (0,0 ~ 1,1).")]
    public Vector2Parameter center = new Vector2Parameter(new Vector2(0.5f, 0.5f));

    [Tooltip("블러의 품질을 결정하는 샘플링 횟수입니다.")]
    public ClampedIntParameter sampleCount = new ClampedIntParameter(10, 2, 20);

    // 이 효과가 현재 활성화되어야 하는지 여부를 반환합니다.
    public bool IsActive() => strength.value > 0f;

    // 이 효과가 Tile 기반 렌더링에 영향을 주는지 여부를 나타냅니다.
    public bool IsTileCompatible() => false;
}
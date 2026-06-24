using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
namespace GraduationProject.Editor
{
    /// <summary>
    /// 빌드 시 컴파일이 불필요한 URP 및 레거시 셰이더 변수(Shader Variants)들을 제외하여
    /// 셰이더 컴파일 병목을 없애고 빌드 시간을 대폭 단축하는 프리프로세서 스크립트입니다.
    /// </summary>
    public class ShaderVariantStripper : IPreprocessShaders
    {
        // 최우선 순위로 작동하도록 설정
        public int callbackOrder => 0;

        // 빌드 단계에서 제외할 키워드 목록
        private static readonly HashSet<string> StripKeywords = new HashSet<string>
        {
            // 디퍼드 렌더링 키워드 (프로젝트는 Forward 렌더링 사용)
            "_DEFERRED_SHADING",
            
            // 추가 광원의 실시간 그림자 키워드 (메인 섀도우 외 추가 섀도우 미사용)
            "_ADDITIONAL_LIGHT_SHADOWS",
            
            // 사용하지 않는 섀도우마스크 모드
            "SHADOWS_SHADOWMASK",
            
            // 디버그용 디스플레이 셰이더 키워드
            "DEBUG_DISPLAY",
            
            // 사용하지 않는 라이트맵 종류
            "DIRLIGHTMAP_COMBINED"
        };

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            // URP 환경이므로 Legacy Built-in 셰이더 빌드 제외 (불필요한 중복 컴파일 방지)
            if (shader.name.StartsWith("Standard") || 
                shader.name.StartsWith("Mobile/Diffuse") || 
                shader.name.StartsWith("Legacy Shaders/"))
            {
                data.Clear();
                return;
            }

            // 컴파일 데이터가 비어있다면 생략
            if (data == null || data.Count == 0) return;

            // 역순으로 순회하며 불필요한 키워드가 포함된 베리언트 제거
            for (int i = data.Count - 1; i >= 0; i--)
            {
                ShaderCompilerData shaderData = data[i];
                bool shouldStrip = false;

                foreach (var keyword in StripKeywords)
                {
                    // 로컬 키워드가 해당 셰이더에 활성화되어 있는지 검사
                    LocalKeyword localKeyword = new LocalKeyword(shader, keyword);
                    if (shaderData.shaderKeywordSet.IsEnabled(localKeyword))
                    {
                        shouldStrip = true;
                        break;
                    }
                }

                if (shouldStrip)
                {
                    data.RemoveAt(i);
                }
            }
        }
    }
}
#endif

using UnityEngine;
using MoreMountains.Feedbacks; // Feel 에셋 네임스페이스

public class FeelEventTrigger : MonoBehaviour
{
    // 인스펙터 창에서 여러 개의 MMF Player를 할당받을 배열입니다.
    public MMF_Player[] feedbacksToPlay;

    /// <summary>
    /// 이 함수를 애니메이션 이벤트에서 호출할 것입니다.
    /// int형 매개변수(index)를 받아 해당 번호의 피드백을 재생합니다.
    /// </summary>
    /// <param name="index">재생할 feedbacksToPlay 배열의 순번</param>
    public void PlayFeedbacksByIndex(int index)
    {
        // 배열이 비어있지 않고, 요청된 index가 배열 범위 내에 있는지 확인합니다.
        if (feedbacksToPlay != null && index >= 0 && index < feedbacksToPlay.Length)
        {
            // 해당 index의 MMF Player가 할당되어 있는지 확인하고 재생합니다.
            if (feedbacksToPlay[index] != null)
            {
                feedbacksToPlay[index].PlayFeedbacks();
            }
            else
            {
                Debug.LogWarning($"배열의 {index}번 MMF Player가 비어있습니다!", this.gameObject);
            }
        }
        else
        {
            Debug.LogError($"잘못된 인덱스({index})가 요청되었거나 MMF Player 배열이 비어있습니다.", this.gameObject);
        }
    }
}
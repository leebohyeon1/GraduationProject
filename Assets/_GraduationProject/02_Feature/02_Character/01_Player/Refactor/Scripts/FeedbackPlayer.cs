using BH_Lib.Log;
using MoreMountains.Feedbacks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace refactor
{
    [Serializable]
    public struct FeedbackConfig<T>
    {
        public T type;
        public MMF_Player Feedback;
    }

    [Serializable]
    public class FeedbackPlayer<T>
    {
        [Header("Feedbacks")]
        [SerializeField] private List<FeedbackConfig<T>> _feedbacks;
        private Dictionary<T, MMF_Player> _feedbackDictionary = new Dictionary<T, MMF_Player>();

        public void Initialize()
        {
            foreach (var feedbackPlayer in _feedbacks)
            {
                _feedbackDictionary[feedbackPlayer.type] = feedbackPlayer.Feedback;
            }
        }

        /// <summary>
        /// 피드백 재생 함수
        /// </summary>
        /// <param name="feedbackType">피드백 분류</param>
        /// <param name="position">피드백 재생 위치</param>
        public void PlayFeedback(T feedbackType, Vector3 position)
        {
            if (_feedbackDictionary.TryGetValue(feedbackType, out MMF_Player feedback))
            {
                if (feedback == null)
                {
                    Log.PrintWarning($"피드백이 null {feedbackType}");
                    return;
                }

                feedback.PlayFeedbacks(position);
            }
            else
            {
                Log.PrintWarning($"피드백 등록안됨 {feedbackType}");
            }
        }

        /// <summary>
        /// 피드백 정지 함수
        /// </summary>
        /// <param name="feedbackType">피드백 분류</param>
        public void StopFeedback(T feedbackType)
        {
            if (_feedbackDictionary.TryGetValue(feedbackType, out MMF_Player feedback))
            {
                if (feedback == null)
                {
                    Log.PrintWarning($"피드백이 null {feedbackType}");
                    return;
                }

                feedback.StopFeedbacks();
            }
            else
            {
                Log.PrintWarning($"피드백 등록안됨 {feedbackType}");
            }
        }

    }
}

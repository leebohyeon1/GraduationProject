using UnityEngine;
using MoreMountains.Tools;

namespace GraduationProject
{
    /// <summary>
    /// MMSoundManager를 Unity Event에서 쉽게 사용할 수 있도록 도와주는 래퍼 클래스입니다.
    /// </summary>
    public class MMSoundManagerEventHelper : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("재생할 오디오 클립을 할당하세요.")]
        public AudioClip TargetAudioClip;
        
        [Tooltip("재생될 트랙을 설정하세요.")]
        public MMSoundManager.MMSoundManagerTracks Track = MMSoundManager.MMSoundManagerTracks.Music;
        
        [Tooltip("루프 재생 여부")]
        public bool Loop = true;
        
        [Range(0f, 1f)]
        public float Volume = 1.0f;

        [Header("Status")]
        [SerializeField]
        protected AudioSource _currentSource;

        /// <summary>
        /// 설정된 클립을 재생합니다. (Unity Event에서 호출 가능)
        /// </summary>
        public void Play()
        {
            if (TargetAudioClip == null)
            {
                Debug.LogWarning($"[{name}] TargetAudioClip이 할당되지 않았습니다.");
                return;
            }

            // 이미 재생 중인 소리가 있다면 정지 (선택 사항)
            Stop();

            _currentSource = MMSoundManager.Instance.PlaySound(
                TargetAudioClip, 
                Track, 
                transform.position, 
                Loop, 
                Volume
            );
        }

        /// <summary>
        /// 현재 재생 중인 소리를 정지합니다. (Unity Event에서 호출 가능)
        /// </summary>
        public void Stop()
        {
            if (_currentSource != null)
            {
                MMSoundManager.Instance.StopSound(_currentSource);
                _currentSource = null;
            }
        }

        /// <summary>
        /// 특정 트랙의 모든 소리를 정지합니다. (Unity Event에서 호출 가능)
        /// </summary>
        public void StopTrack()
        {
            MMSoundManager.Instance.StopTrack(Track);
        }

        /// <summary>
        /// 일시 정지 (Unity Event에서 호출 가능)
        /// </summary>
        public void Pause()
        {
            if (_currentSource != null)
            {
                MMSoundManager.Instance.PauseSound(_currentSource);
            }
        }

        /// <summary>
        /// 일시 정지 해제 (Unity Event에서 호출 가능)
        /// </summary>
        public void Resume()
        {
            if (_currentSource != null)
            {
                MMSoundManager.Instance.ResumeSound(_currentSource);
            }
        }
    }
}

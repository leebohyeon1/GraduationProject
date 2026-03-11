using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools; // MMSoundManager 사용을 위해 필요

public class AudioSettingUI : MonoBehaviour
{
    [Header("UI Sliders")]
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    private void Start()
    {
        if (MMSoundManager.HasInstance)
        {
            // 초기값 설정 (MMSoundManager의 현재 설정값 로드)
            if (_masterSlider != null)
                _masterSlider.value = MMSoundManager.Instance.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.Master, false);
            
            if (_musicSlider != null)
                _musicSlider.value = MMSoundManager.Instance.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.Music, false);
            
            if (_sfxSlider != null)
                _sfxSlider.value = MMSoundManager.Instance.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.Sfx, false);
        }

        // 리스너 등록
        if (_masterSlider != null)
            _masterSlider.onValueChanged.AddListener(val => SetVolume(MMSoundManager.MMSoundManagerTracks.Master, val));
        
        if (_musicSlider != null)
            _musicSlider.onValueChanged.AddListener(val => SetVolume(MMSoundManager.MMSoundManagerTracks.Music, val));
        
        if (_sfxSlider != null)
            _sfxSlider.onValueChanged.AddListener(val => SetVolume(MMSoundManager.MMSoundManagerTracks.Sfx, val));
    }

    private void SetVolume(MMSoundManager.MMSoundManagerTracks track, float volume)
    {
        if (MMSoundManager.HasInstance)
        {
            MMSoundManager.Instance.SetTrackVolume(track, volume);
            // 설정값 저장
            MMSoundManager.Instance.SaveSettings();
        }
    }
}

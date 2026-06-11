using TMPro;
using UnityEngine;

public class SkillDescription : MonoBehaviour
{
    [SerializeField] private TMP_Text _skillName;
    [SerializeField] private TMP_Text _skillDescription;
    [SerializeField] private TMP_Text _moneyAmount;
    [SerializeField] private UnityEngine.Video.VideoPlayer _videoPlayer;
    // [SerializeField] private TMP_Text _specialmoneyAmount;

    public void SetDescription(string skillName, string skillDescription, string moneyAmount, string specialMoneyAmount, UnityEngine.Video.VideoClip videoClip = null)
    {
        _skillName.text = skillName;
        _skillDescription.text = skillDescription;
        _moneyAmount.text = moneyAmount;
        // _specialmoneyAmount.text = specialMoneyAmount;

        if (_videoPlayer != null)
        {
            if (videoClip != null)
            {
                _videoPlayer.clip = videoClip;
                _videoPlayer.Play();
            }
            else
            {
                _videoPlayer.Stop();
                _videoPlayer.clip = null;
            }
        }
    }
}

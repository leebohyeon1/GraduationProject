using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

// Scene music and combat share the existing Music mixer, preserving the user's BGM volume.
public class CombatMusicController : MonoBehaviour
{
    public float crossfadeSeconds = 1.5f;
    public float combatReleaseSeconds = 2f;
    public float combatRange = 65f;
    private readonly AudioSource[] _voices = new AudioSource[2];
    private readonly float[] _gain = new float[2];
    private readonly float[] _start = new float[2];
    private readonly float[] _levels = new float[2];
    private readonly Dictionary<AudioClip, float> _positions = new Dictionary<AudioClip, float>();
    private AudioClip _background, _combat, _wanted, _sceneBackground;
    private bool _scriptedMusic;
    private float _backgroundVolume = 1f, _wantedVolume = 1f, _combatVolume = 1f;
    private bool _backgroundLoop = true, _wantedLoop = true, _paused, _stopped;
    private float _lastCombat = float.NegativeInfinity, _nextPoll, _elapsed;
    private int _target = -1;
    private bool _fading;
    private PlayerController _player;
    public bool CombatActive { get; private set; }

    public static CombatMusicController Ensure()
    {
        var host = SceneLoadingManager.Instance;
        if (!host) return null;
        var music = host.GetComponent<CombatMusicController>();
        return music ? music : host.gameObject.AddComponent<CombatMusicController>();
    }

    private void Awake()
    {
        for (int i = 0; i < 2; i++)
        {
            var child = new GameObject("Music Crossfade " + i);
            child.transform.SetParent(transform, false);
            _voices[i] = child.AddComponent<AudioSource>();
            _voices[i].playOnAwake = false;
            _voices[i].spatialBlend = 0f;
            _voices[i].volume = 0f;
        }
    }

    public void SetScene(SceneDataSO scene)
    {
        _sceneBackground = scene ? scene.BackgroundMusic : null;
        _combat = scene ? scene.CombatMusic : null;
        _combatVolume = scene ? Mathf.Clamp01(scene.CombatMusicVolume) : 1f;
        _lastCombat = float.NegativeInfinity;
        CombatActive = false;
        SetBackground(scene ? scene.BackgroundMusic : null);
    }

    public void SetBackground(AudioClip clip, float volume = 1f, bool loop = true)
    {
        _background = clip;
        // Timeline/boss music owns playback until scene music is explicitly restored.
        _scriptedMusic = clip && clip != _sceneBackground;
        if (_scriptedMusic) { CombatActive=false; _lastCombat=float.NegativeInfinity; }
        _backgroundVolume = Mathf.Clamp01(volume);
        _backgroundLoop = loop;
        _stopped = false;
        SetPaused(false);
        _nextPoll = 0f;
    }

    public void StopMusic() { _stopped = true; _wanted = null; _nextPoll = 0f; }
    public void SetPaused(bool paused)
    {
        _paused = paused;
        foreach (var voice in _voices)
            if (voice) { if (paused) voice.Pause(); else voice.UnPause(); }
    }

    private void PollCombat()
    {
        if (!_player || !_player.gameObject.activeInHierarchy) _player = FindFirstObjectByType<PlayerController>();
        var player = _player;
        bool allowed = !_scriptedMusic && _combat && player && player.Health != null && !player.Health.IsDead &&
            (!SceneLoadingManager.Instance || !SceneLoadingManager.Instance.IsTeleporting) &&
            (!player.InputReader || player.InputReader.CurrentInputMode != InputReaderSO.InputMode.CutScene);
        bool fighting = false;
        if (allowed)
        {
            foreach (var enemy in Enemy.ActiveEnemies)
            {
                if (!enemy || enemy.EnemyHealth == null || enemy.EnemyHealth.IsDead ||
                    enemy._aiController == null || enemy._aiController._aiBrain == null) continue;
                if (enemy._aiController._aiBrain._isCombat &&
                    (enemy.transform.position - player.transform.position).sqrMagnitude <= combatRange * combatRange)
                { fighting = true; break; }
            }
        }
        if (fighting) _lastCombat = Time.unscaledTime;
        if (!allowed) _lastCombat = float.NegativeInfinity;
        CombatActive = allowed && (fighting || Time.unscaledTime - _lastCombat < combatReleaseSeconds);
        _wanted = _stopped ? null : (CombatActive ? _combat : _background);
        _wantedVolume = CombatActive ? _combatVolume : _backgroundVolume;
        _wantedLoop = CombatActive || _backgroundLoop;
    }

    private void Update()
    {
        if (_paused) return;
        var manager = MMSoundManager.Instance;
        if (!manager || !manager.settingsSo) return;
        foreach (var voice in _voices)
            if (voice.outputAudioMixerGroup != manager.settingsSo.MusicAudioMixerGroup)
                voice.outputAudioMixerGroup = manager.settingsSo.MusicAudioMixerGroup;
        if (Time.unscaledTime >= _nextPoll) { _nextPoll = Time.unscaledTime + .2f; PollCombat(); }

        int desired = -1;
        if (_wanted)
        {
            for (int i = 0; i < 2; i++) if (_voices[i].clip == _wanted) desired = i;
            if (desired < 0)
            {
                // Finish an in-flight transition before introducing a third track.
                for (int i = 0; i < 2; i++) if (_gain[i] <= .0001f) { desired = i; break; }
                if (desired >= 0)
                {
                    RememberAndStop(desired);
                    var voice = _voices[desired];
                    voice.clip = _wanted;
                    voice.loop = _wantedLoop;
                    voice.volume = 0f;
                    if (_positions.TryGetValue(_wanted, out float position) && position < _wanted.length)
                        voice.time = position;
                    voice.Play();
                }
                else desired = _target;
            }
            if (desired >= 0 && _voices[desired].clip == _wanted)
            { _levels[desired] = _wantedVolume; _voices[desired].loop = _wantedLoop; }
        }
        if (desired != _target)
        {
            _target = desired;
            _elapsed = 0f;
            _fading = true;
            for (int i = 0; i < 2; i++) _start[i] = _gain[i];
        }
        if (_fading)
        {
            _elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_elapsed / Mathf.Max(.05f, crossfadeSeconds)));
            for (int i = 0; i < 2; i++)
                _gain[i] = Mathf.Sqrt(Mathf.Lerp(_start[i] * _start[i], i == _target ? 1f : 0f, t));
            if (t >= 1f)
            {
                _fading = false;
                for (int i = 0; i < 2; i++) if (i != _target) RememberAndStop(i);
            }
        }
        for (int i = 0; i < 2; i++) _voices[i].volume = _gain[i] * _levels[i];
    }

    private void RememberAndStop(int i)
    {
        var voice = _voices[i];
        if (voice.clip && voice.isPlaying) _positions[voice.clip] = voice.time;
        voice.Stop();
        voice.clip = null;
    }
}

using UnityEngine;

public enum AudioSfx
{
    PlayerShoot,
    EnemyHit,
    PlayerHit,
    CoinCollect,
    DoorOpen,
    UiClick,
    RoomClear
}

public class AudioManager : MonoBehaviour
{
    private const string BgmVolumeKey = "Audio.BgmVolume";
    private const string SfxVolumeKey = "Audio.SfxVolume";
    private const string PlayerShootPath = "Audio/SFX/SFX_Player_Shoot";
    private const string EnemyHitPath = "Audio/SFX/SFX_Enemy_Hit";
    private const string PlayerHitPath = "Audio/SFX/SFX_Player_Hit";
    private const string CoinCollectPath = "Audio/SFX/SFX_Coin_Collect";
    private const string DoorOpenPath = "Audio/SFX/SFX_Door_Open";
    private const string UiClickPath = "Audio/SFX/SFX_UI_Click";
    private const string RoomClearPath = "Audio/SFX/SFX_Room_Clear";

    public static AudioManager MainInstance { get; private set; }

    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField, Range(0f, 1f)] private float _defaultBgmVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float _defaultSfxVolume = 0.8f;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip _playerShoot;
    [SerializeField] private AudioClip _enemyHit;
    [SerializeField] private AudioClip _playerHit;
    [SerializeField] private AudioClip _coinCollect;
    [SerializeField] private AudioClip _doorOpen;
    [SerializeField] private AudioClip _uiClick;
    [SerializeField] private AudioClip _roomClear;

    public float BgmVolume { get; private set; }
    public float SfxVolume { get; private set; }

    private void Awake()
    {
        if (MainInstance && MainInstance != this)
        {
            Destroy(gameObject);
            return;
        }

        MainInstance = this;
        ResolveAudioSources();
        ResolveSfxClips();
        LoadVolumeSettings();
        ConfigureSources();
    }

    public void PlaySfx(AudioSfx sfx)
    {
        AudioClip clip = GetSfxClip(sfx);
        if (!_sfxSource || !clip)
            return;

        _sfxSource.PlayOneShot(clip);
    }

    public void SetBgmVolume(float volume)
    {
        BgmVolume = Mathf.Clamp01(volume);
        if (_bgmSource)
            _bgmSource.volume = BgmVolume;

        PlayerPrefs.SetFloat(BgmVolumeKey, BgmVolume);
    }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp01(volume);
        if (_sfxSource)
            _sfxSource.volume = SfxVolume;

        PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
    }

    private void ConfigureSources()
    {
        if (_bgmSource)
        {
            _bgmSource.loop = true;
            _bgmSource.spatialBlend = 0f;
            _bgmSource.volume = BgmVolume;
        }

        if (_sfxSource)
        {
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
            _sfxSource.spatialBlend = 0f;
            _sfxSource.volume = SfxVolume;
        }
    }

    private void LoadVolumeSettings()
    {
        BgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, _defaultBgmVolume);
        SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, _defaultSfxVolume);
    }

    private void ResolveAudioSources()
    {
        if (!_bgmSource)
            _bgmSource = FindChildAudioSource("BgmSource");

        if (!_sfxSource)
            _sfxSource = FindChildAudioSource("SFXSource");
    }

    private AudioSource FindChildAudioSource(string childName)
    {
        Transform child = transform.Find(childName);
        if (!child)
            return null;

        return child.GetComponent<AudioSource>();
    }

    private void ResolveSfxClips()
    {
        if (!_playerShoot)
            _playerShoot = Resources.Load<AudioClip>(PlayerShootPath);

        if (!_enemyHit)
            _enemyHit = Resources.Load<AudioClip>(EnemyHitPath);

        if (!_playerHit)
            _playerHit = Resources.Load<AudioClip>(PlayerHitPath);

        if (!_coinCollect)
            _coinCollect = Resources.Load<AudioClip>(CoinCollectPath);

        if (!_doorOpen)
            _doorOpen = Resources.Load<AudioClip>(DoorOpenPath);

        if (!_uiClick)
            _uiClick = Resources.Load<AudioClip>(UiClickPath);

        if (!_roomClear)
            _roomClear = Resources.Load<AudioClip>(RoomClearPath);
    }

    private AudioClip GetSfxClip(AudioSfx sfx)
    {
        switch (sfx)
        {
            case AudioSfx.PlayerShoot:
                return _playerShoot;
            case AudioSfx.EnemyHit:
                return _enemyHit;
            case AudioSfx.PlayerHit:
                return _playerHit;
            case AudioSfx.CoinCollect:
                return _coinCollect;
            case AudioSfx.DoorOpen:
                return _doorOpen;
            case AudioSfx.UiClick:
                return _uiClick;
            case AudioSfx.RoomClear:
                return _roomClear;
            default:
                return null;
        }
    }

    private void OnDestroy()
    {
        if (MainInstance == this)
            MainInstance = null;
    }
}

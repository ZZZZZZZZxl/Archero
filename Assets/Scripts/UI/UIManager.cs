using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager MainInstance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject _hudPanel;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _upgradePanel;
    [SerializeField] private GameObject _doorMenuPanel;
    [SerializeField] private GameObject _gameOverPanel;

    [Header("Buttons")]
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _returnMenuButton;
    [SerializeField] private Button _gameOverRestartButton;

    [Header("Audio")]
    [SerializeField] private Slider[] _bgmSliders;
    [SerializeField] private Slider[] _sfxSliders;

    [Header("Health")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private PlayerHealthController _playerHealth;
    [SerializeField] private float _maxHealthSliderWidthMultiplier = 2.5f;

    [Header("Experience")]
    [SerializeField] private Slider _experienceSlider;
    [SerializeField] private TMP_Text _experienceText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private Color _levelTextColor = new Color(0.45f, 0.02f, 0.02f, 1f);
    [SerializeField] private PlayerExperienceController _playerExperience;

    [Header("Upgrade")]
    [SerializeField] private TMP_Text _upgradeTitleText;
    [SerializeField] private Button[] _upgradeChoiceButtons;
    [SerializeField] private TMP_Text[] _upgradeChoiceTitleTexts;
    [SerializeField] private TMP_Text[] _upgradeChoiceDescriptionTexts;

    private Action<PlayerUpgradeOption> _onUpgradeSelected;
    private readonly List<PlayerUpgradeOption> _currentUpgradeChoices = new List<PlayerUpgradeOption>();
    private RectTransform _healthSliderRect;
    private float _baseHealthSliderWidth;
    private float _baseMaxHealth;

    private void Awake()
    {
        MainInstance = this;
        ResolvePanelReferences();
        ResolvePlayerReferences();
    }

    private void OnDestroy()
    {
        if (MainInstance == this)
            MainInstance = null;
    }

    private void OnEnable()
    {
        BindButtons();
        ResolveAudioSliders();
        BindAudioSliders();
        BindPlayerStats();
    }

    private void OnDisable()
    {
        UnbindButtons();
        UnbindAudioSliders();
        UnbindPlayerStats();
    }

    private void Start()
    {
        ResolvePanelReferences();
        ResolvePlayerReferences();
        ResolveAudioSliders();
        RefreshAudioSliders();
        RefreshHealth();
        RefreshExperience();

        if (GameFlowManager.MainInstance)
            SetState(GameFlowManager.MainInstance.State);
    }

    public void SetState(GameFlowState state)
    {
        ResolvePanelReferences();

        SetActive(_hudPanel, state == GameFlowState.Playing || state == GameFlowState.Paused || state == GameFlowState.UpgradeSelecting);
        SetActive(_pausePanel, state == GameFlowState.Paused);
        SetActive(_doorMenuPanel, false);
        SetActive(_upgradePanel, state == GameFlowState.UpgradeSelecting);
        SetActive(_gameOverPanel, state == GameFlowState.GameOver);
    }

    public void ShowUpgradeChoices(IReadOnlyList<PlayerUpgradeOption> choices, Action<PlayerUpgradeOption> onSelected)
    {
        _onUpgradeSelected = onSelected;
        _currentUpgradeChoices.Clear();

        if (choices != null)
            _currentUpgradeChoices.AddRange(choices);

        if (_upgradeTitleText)
            _upgradeTitleText.text = "选择强化";

        if (_upgradeChoiceButtons == null)
            return;

        for (int i = 0; i < _upgradeChoiceButtons.Length; i++)
        {
            Button button = _upgradeChoiceButtons[i];
            bool hasChoice = i < _currentUpgradeChoices.Count;
            SetActive(button ? button.gameObject : null, hasChoice);

            if (!button)
                continue;

            button.onClick.RemoveAllListeners();
            if (!hasChoice)
                continue;

            int choiceIndex = i;
            button.onClick.AddListener(() => OnUpgradeChoiceClicked(choiceIndex));

            PlayerUpgradeOption option = _currentUpgradeChoices[i];
            if (_upgradeChoiceTitleTexts != null && i < _upgradeChoiceTitleTexts.Length && _upgradeChoiceTitleTexts[i])
                _upgradeChoiceTitleTexts[i].text = option.Title;

            if (_upgradeChoiceDescriptionTexts != null && i < _upgradeChoiceDescriptionTexts.Length && _upgradeChoiceDescriptionTexts[i])
                _upgradeChoiceDescriptionTexts[i].text = option.Description;
        }

        SetActive(_upgradePanel, true);
    }

    private void BindButtons()
    {
        if (_resumeButton)
            _resumeButton.onClick.AddListener(OnResumeClicked);
        if (_restartButton)
            _restartButton.onClick.AddListener(OnRestartClicked);
        if (_returnMenuButton)
            _returnMenuButton.onClick.AddListener(OnReturnMenuClicked);
        if (_gameOverRestartButton)
            _gameOverRestartButton.onClick.AddListener(OnRestartClicked);
    }

    private void UnbindButtons()
    {
        if (_resumeButton)
            _resumeButton.onClick.RemoveListener(OnResumeClicked);
        if (_restartButton)
            _restartButton.onClick.RemoveListener(OnRestartClicked);
        if (_returnMenuButton)
            _returnMenuButton.onClick.RemoveListener(OnReturnMenuClicked);
        if (_gameOverRestartButton)
            _gameOverRestartButton.onClick.RemoveListener(OnRestartClicked);
    }

    private void BindAudioSliders()
    {
        ResolveAudioSliders();
        AddSliderListeners(_bgmSliders, OnBgmVolumeChanged);
        AddSliderListeners(_sfxSliders, OnSfxVolumeChanged);
        RefreshAudioSliders();
    }

    private void UnbindAudioSliders()
    {
        RemoveSliderListeners(_bgmSliders, OnBgmVolumeChanged);
        RemoveSliderListeners(_sfxSliders, OnSfxVolumeChanged);
    }

    private void BindPlayerStats()
    {
        ResolvePlayerReferences();
        BindHealth();
        BindExperience();
    }

    private void UnbindPlayerStats()
    {
        UnbindHealth();
        UnbindExperience();
    }

    private void BindHealth()
    {
        if (!_playerHealth)
            return;

        _playerHealth.HealthChangedEvent -= OnHealthChanged;
        _playerHealth.HealthChangedEvent += OnHealthChanged;
        RefreshHealth();
    }

    private void UnbindHealth()
    {
        if (!_playerHealth)
            return;

        _playerHealth.HealthChangedEvent -= OnHealthChanged;
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        if (_healthSlider)
        {
            if (!_healthSlider.gameObject.activeSelf)
                _healthSlider.gameObject.SetActive(true);

            _healthSlider.maxValue = Mathf.Max(1f, maxHealth);
            _healthSlider.value = Mathf.Clamp(currentHealth, 0f, _healthSlider.maxValue);
        }

        if (_healthText)
        {
            if (!_healthText.gameObject.activeSelf)
                _healthText.gameObject.SetActive(true);

            _healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }

        UpdateHealthSliderWidth(maxHealth);
    }

    private void BindExperience()
    {
        if (!_playerExperience)
            return;

        _playerExperience.ExperienceChangedEvent -= OnExperienceChanged;
        _playerExperience.ExperienceChangedEvent += OnExperienceChanged;
        _playerExperience.LevelUpEvent -= OnLevelUp;
        _playerExperience.LevelUpEvent += OnLevelUp;
    }

    private void UnbindExperience()
    {
        if (!_playerExperience)
            return;

        _playerExperience.ExperienceChangedEvent -= OnExperienceChanged;
        _playerExperience.LevelUpEvent -= OnLevelUp;
    }

    private void OnExperienceChanged(int level, int currentExperience, int experienceToNextLevel)
    {
        bool isMaxLevel = _playerExperience && _playerExperience.IsMaxLevel;

        if (_experienceSlider)
        {
            _experienceSlider.maxValue = isMaxLevel ? 1 : Mathf.Max(1, experienceToNextLevel);
            _experienceSlider.value = isMaxLevel ? 1 : currentExperience;
        }

        if (_levelText)
        {
            _levelText.color = _levelTextColor;
            _levelText.text = isMaxLevel ? $"Lv.{level} MAX" : $"Lv.{level}";
        }

        if (_experienceText)
        {
            _experienceText.text = string.Empty;
            _experienceText.gameObject.SetActive(false);
        }
    }

    private void OnLevelUp(int level)
    {
        RefreshExperience();
    }

    private void RefreshExperience()
    {
        if (_playerExperience)
            OnExperienceChanged(
                _playerExperience.Level,
                _playerExperience.CurrentExperience,
                _playerExperience.ExperienceToNextLevel);
    }

    public void RefreshHealth()
    {
        ResolvePlayerReferences();

        if (_playerHealth)
            OnHealthChanged(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
    }

    private void RefreshAudioSliders()
    {
        ResolveAudioSliders();

        AudioManager audioManager = AudioManager.MainInstance;
        if (!audioManager)
            return;

        SetSliderValuesWithoutNotify(_bgmSliders, audioManager.BgmVolume);
        SetSliderValuesWithoutNotify(_sfxSliders, audioManager.SfxVolume);
    }

    private void OnBgmVolumeChanged(float volume)
    {
        if (AudioManager.MainInstance)
            AudioManager.MainInstance.SetBgmVolume(volume);

        SetSliderValuesWithoutNotify(_bgmSliders, volume);
    }

    private void OnSfxVolumeChanged(float volume)
    {
        if (AudioManager.MainInstance)
            AudioManager.MainInstance.SetSfxVolume(volume);

        SetSliderValuesWithoutNotify(_sfxSliders, volume);
    }

    private void OnResumeClicked()
    {
        PlayClickSfx();
        if (GameFlowManager.MainInstance)
            GameFlowManager.MainInstance.ResumeGame();
    }

    private void OnRestartClicked()
    {
        PlayClickSfx();
        if (GameFlowManager.MainInstance)
            GameFlowManager.MainInstance.RestartGame();
    }

    private void OnReturnMenuClicked()
    {
        PlayClickSfx();
        if (GameFlowManager.MainInstance)
            GameFlowManager.MainInstance.ReturnToMainMenu();
    }

    private void OnUpgradeChoiceClicked(int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= _currentUpgradeChoices.Count)
            return;

        PlayClickSfx();
        PlayerUpgradeOption option = _currentUpgradeChoices[choiceIndex];
        Action<PlayerUpgradeOption> onUpgradeSelected = _onUpgradeSelected;
        HideUpgradeChoices();
        onUpgradeSelected?.Invoke(option);
    }

    private void HideUpgradeChoices()
    {
        SetActive(_upgradePanel, false);
        _currentUpgradeChoices.Clear();
        _onUpgradeSelected = null;
    }

    private void ResolvePlayerReferences()
    {
        if (!_playerHealth)
            _playerHealth = FindObjectOfType<PlayerHealthController>();

        if (!_playerExperience)
            _playerExperience = FindObjectOfType<PlayerExperienceController>();

        if (!_healthSlider)
            _healthSlider = FindComponentByName<Slider>(transform, "HealthSlider");

        if (!_healthText)
            _healthText = FindComponentByName<TMP_Text>(transform, "HealthText");

        ResolveHealthSliderRect();
    }

    private void ResolveHealthSliderRect()
    {
        if (!_healthSlider)
            return;

        if (!_healthSliderRect)
            _healthSliderRect = _healthSlider.GetComponent<RectTransform>();

        if (_healthSliderRect && _baseHealthSliderWidth <= 0f)
            _baseHealthSliderWidth = _healthSliderRect.sizeDelta.x;

        if (_playerHealth && _baseMaxHealth <= 0f)
            _baseMaxHealth = Mathf.Max(1f, _playerHealth.MaxHealth);
    }

    private void UpdateHealthSliderWidth(float maxHealth)
    {
        ResolveHealthSliderRect();

        if (!_healthSliderRect || _baseHealthSliderWidth <= 0f || _baseMaxHealth <= 0f)
            return;

        float widthMultiplier = Mathf.Clamp(
            maxHealth / _baseMaxHealth,
            1f,
            Mathf.Max(1f, _maxHealthSliderWidthMultiplier)
        );

        Vector2 sizeDelta = _healthSliderRect.sizeDelta;
        sizeDelta.x = _baseHealthSliderWidth * widthMultiplier;
        _healthSliderRect.sizeDelta = sizeDelta;
    }

    private void ResolvePanelReferences()
    {
        if (!_hudPanel)
            _hudPanel = FindChildGameObject("HUDPanel");

        if (!_pausePanel)
            _pausePanel = FindChildGameObject("PausePanel");

        if (!_upgradePanel)
            _upgradePanel = FindChildGameObject("UpgradePanel");

        if (!_doorMenuPanel)
            _doorMenuPanel = FindChildGameObject("DoorMenuPanel");

        if (!_gameOverPanel)
            _gameOverPanel = FindChildGameObject("GameOverPanel");
    }

    private void ResolveAudioSliders()
    {
        if (!HasAnyValidSlider(_bgmSliders))
            _bgmSliders = FindSlidersByParentName("BgmVolume");

        if (!HasAnyValidSlider(_sfxSliders))
            _sfxSliders = FindSlidersByParentName("SfxVolume");

        ConfigureSliders(_bgmSliders);
        ConfigureSliders(_sfxSliders);
    }

    private Slider[] FindSlidersByParentName(string parentName)
    {
        List<Slider> results = new List<Slider>();
        Slider[] sliders = GetComponentsInChildren<Slider>(true);
        foreach (Slider slider in sliders)
        {
            if (!slider)
                continue;

            Transform parent = slider.transform.parent;
            if (parent && parent.name.Contains(parentName))
                results.Add(slider);
        }

        return results.ToArray();
    }

    private static bool HasAnyValidSlider(Slider[] sliders)
    {
        if (sliders == null || sliders.Length == 0)
            return false;

        foreach (Slider slider in sliders)
        {
            if (slider)
                return true;
        }

        return false;
    }

    private static void ConfigureSliders(Slider[] sliders)
    {
        if (sliders == null)
            return;

        foreach (Slider slider in sliders)
        {
            if (!slider)
                continue;

            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.interactable = true;

            Graphic[] graphics = slider.GetComponentsInChildren<Graphic>(true);
            foreach (Graphic graphic in graphics)
            {
                if (graphic)
                    graphic.raycastTarget = true;
            }
        }
    }

    private GameObject FindChildGameObject(string targetName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child && child.name == targetName)
                return child.gameObject;
        }

        return null;
    }

    private static T FindComponentByName<T>(Transform root, string targetName) where T : Component
    {
        if (!root)
            return null;

        T[] components = root.GetComponentsInChildren<T>(true);
        foreach (T component in components)
        {
            if (component && component.name == targetName)
                return component;
        }

        return null;
    }

    private static void AddSliderListeners(Slider[] sliders, UnityEngine.Events.UnityAction<float> callback)
    {
        if (sliders == null)
            return;

        foreach (Slider slider in sliders)
        {
            if (!slider)
                continue;

            slider.onValueChanged.RemoveListener(callback);
            slider.onValueChanged.AddListener(callback);
        }
    }

    private static void RemoveSliderListeners(Slider[] sliders, UnityEngine.Events.UnityAction<float> callback)
    {
        if (sliders == null)
            return;

        foreach (Slider slider in sliders)
        {
            if (slider)
                slider.onValueChanged.RemoveListener(callback);
        }
    }

    private static void SetSliderValuesWithoutNotify(Slider[] sliders, float value)
    {
        if (sliders == null)
            return;

        foreach (Slider slider in sliders)
        {
            if (slider)
                slider.SetValueWithoutNotify(value);
        }
    }

    private static void PlayClickSfx()
    {
        if (AudioManager.MainInstance)
            AudioManager.MainInstance.PlaySfx(AudioSfx.UiClick);
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target && target.activeSelf != active)
            target.SetActive(active);
    }
}

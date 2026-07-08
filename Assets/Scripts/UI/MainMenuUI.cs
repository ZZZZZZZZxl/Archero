using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private GameObject _loadingBlocker;

    private void OnEnable()
    {
        ResolveSliders();
        ConfigureSlider(_bgmSlider);
        ConfigureSlider(_sfxSlider);
        BindButtons();
        BindSliders();
        RefreshSliders();
        SetLoading(false);
    }

    private void OnDisable()
    {
        UnbindButtons();
        UnbindSliders();
    }

    private void Update()
    {
        if (_loadingBlocker && _loadingBlocker.activeSelf != SceneLoader.IsLoading)
            _loadingBlocker.SetActive(SceneLoader.IsLoading);
    }

    private void BindButtons()
    {
        if (_startButton)
            _startButton.onClick.AddListener(OnStartClicked);

        if (_quitButton)
            _quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void UnbindButtons()
    {
        if (_startButton)
            _startButton.onClick.RemoveListener(OnStartClicked);

        if (_quitButton)
            _quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    private void BindSliders()
    {
        ResolveSliders();

        if (_bgmSlider)
        {
            _bgmSlider.onValueChanged.RemoveListener(OnBgmVolumeChanged);
            _bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        }

        if (_sfxSlider)
        {
            _sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            _sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }
    }

    private void UnbindSliders()
    {
        if (_bgmSlider)
            _bgmSlider.onValueChanged.RemoveListener(OnBgmVolumeChanged);

        if (_sfxSlider)
            _sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
    }

    private void RefreshSliders()
    {
        ResolveSliders();

        AudioManager audioManager = AudioManager.MainInstance;
        if (!audioManager)
            return;

        if (_bgmSlider)
            _bgmSlider.SetValueWithoutNotify(audioManager.BgmVolume);

        if (_sfxSlider)
            _sfxSlider.SetValueWithoutNotify(audioManager.SfxVolume);
    }

    private void OnStartClicked()
    {
        PlayClickSfx();
        SetLoading(true);
        SceneLoader.LoadGame();
    }

    private void OnQuitClicked()
    {
        PlayClickSfx();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnBgmVolumeChanged(float volume)
    {
        if (AudioManager.MainInstance)
            AudioManager.MainInstance.SetBgmVolume(volume);
    }

    private void OnSfxVolumeChanged(float volume)
    {
        if (AudioManager.MainInstance)
            AudioManager.MainInstance.SetSfxVolume(volume);
    }

    private void SetLoading(bool loading)
    {
        if (_loadingBlocker)
            _loadingBlocker.SetActive(loading);

        if (_startButton)
            _startButton.interactable = !loading;
    }

    private static void PlayClickSfx()
    {
        if (AudioManager.MainInstance)
            AudioManager.MainInstance.PlaySfx(AudioSfx.UiClick);
    }

    private void ResolveSliders()
    {
        if (!_bgmSlider)
            _bgmSlider = FindSliderByParentName("BgmVolume");

        if (!_sfxSlider)
            _sfxSlider = FindSliderByParentName("SfxVolume");
    }

    private Slider FindSliderByParentName(string parentName)
    {
        Slider[] sliders = GetComponentsInChildren<Slider>(true);
        foreach (Slider slider in sliders)
        {
            if (!slider)
                continue;

            Transform parent = slider.transform.parent;
            if (parent && parent.name.Contains(parentName))
                return slider;
        }

        return null;
    }

    private static void ConfigureSlider(Slider slider)
    {
        if (!slider)
            return;

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

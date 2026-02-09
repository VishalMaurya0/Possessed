using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Master")]
    public Slider masterSlider;

    [Header("Music")]
    public Slider musicSlider;
    public Slider ambienceSlider;
    public Slider horrorSlider;
    //public Slider stingersSlider;

    [Header("SFX")]
    public Slider sfxSlider;
    public Slider playerSlider;
    public Slider ghostSlider;
    public Slider environmentSlider;
    //public Slider uiSlider;

    [Header("seed")]
    public TMP_Text seedText;
    int seed;
    int toRunForLongTime = 100000;

    const float MIN_DB = -80f;


    #region Public Setters (UI → OnValueChanged)

    public void SetMaster(float v) => SetVolume("Master", v);
    public void SetMusic(float v) => SetVolume("Music", v);
    public void SetAmbience(float v) => SetVolume("Ambience", v);
    public void SetHorror(float v) => SetVolume("Horror", v);
    //public void SetStingers(float v) => SetVolume("Stingers", v);

    public void SetSFX(float v) => SetVolume("SFX", v);
    public void SetPlayer(float v) => SetVolume("Player", v);
    public void SetGhost(float v) => SetVolume("Ghost", v);
    public void SetEnvironment(float v) => SetVolume("Environment", v);
    //public void SetUI(float v) => SetVolume("UI", v);

    #endregion

    #region Core Logic

    void SetVolume(string param, float value)
    {
        float db = value <= 0.001f ? MIN_DB : Mathf.Log10(value) * 20f;
        audioMixer.SetFloat(param, db);
        PlayerPrefs.SetFloat(param, value);
    }

    float GetSaved(string key, float def = 1f)
    {
        return PlayerPrefs.GetFloat(key, def);
    }

    void LoadAllAudio()
    {
        LoadSlider(masterSlider, "Master");
        LoadSlider(musicSlider, "Music");
        LoadSlider(ambienceSlider, "Ambience");
        LoadSlider(horrorSlider, "Horror");
        //LoadSlider(stingersSlider, "StingersVolume");

        LoadSlider(sfxSlider, "SFX");
        LoadSlider(playerSlider, "Player");
        LoadSlider(ghostSlider, "Ghost");
        LoadSlider(environmentSlider, "Environment");
        //LoadSlider(uiSlider, "UIVolume");
    }

    void LoadSlider(Slider slider, string key)
    {
        if (!slider) return;

        float value = GetSaved(key);
        slider.value = value;

        float db = value <= 0.001f ? MIN_DB : Mathf.Log10(value) * 20f;
        audioMixer.SetFloat(key, db);
    }

    #endregion

    [Header("Graphics")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown qualityDropdown;
    //public Toggle fullscreenToggle;
    //public Toggle vSyncToggle;
    public Slider gammaSlider;

    [Header("Gameplay")]
    public Slider mouseSensitivitySlider;
    public Toggle invertYToggle;

    Resolution[] resolutions;

    const string MASTER_VOL = "MasterVolume";
    const string MUSIC_VOL = "MusicVolume";
    const string SFX_VOL = "SFXVolume";
    const string SENSITIVITY = "MouseSensitivity";
    const string INVERT_Y = "InvertY";

    void Awake()
    {
        //LoadGameplaySettings();
        //SetupResolutions();
        //LoadGraphicsSettings();
        //LoadAllAudio();
    }


    private void Start()
    {
            seed = GameManager.Instance.seed;
    }

    private void Update()
    {
        if (toRunForLongTime > 0)
        {
            toRunForLongTime--;
            seedText.text = $"Seed - {GameManager.Instance.seed}";
            seed = GameManager.Instance.seed;
        }
    }


    #region GRAPHICS
    void SetupResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        int currentResIndex = 0;
        var options = new System.Collections.Generic.List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    public void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        PlayerPrefs.SetInt("VSync", enabled ? 1 : 0);
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("Quality", index);
            GameManager.Instance.dabbaQuality = false;

        if (index == 3)
        {
            GameManager.Instance.dabbaQuality = true;
        }
    }

    public void SetGamma(float index)
    {
        GameManager.Instance.gammaValue = index;
        GameManager.Instance.SetSettings();

        
        //PlayerPrefs.SetInt("Gamma", index);
    }

    void LoadGraphicsSettings()
    {
        //fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        //vSyncToggle.isOn = PlayerPrefs.GetInt("VSync", 1) == 1;

        qualityDropdown.value = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());

        int resIndex = PlayerPrefs.GetInt("ResolutionIndex", resolutionDropdown.value);
        resolutionDropdown.value = resIndex;
    }
    #endregion

    #region GAMEPLAY
    public void SetMouseSensitivity(float value)
    {
        PlayerPrefs.SetFloat(SENSITIVITY, value);
        GameManager.Instance.mouseSensitivity = value;
        if (GameManager.Instance.ownerPlayer)
        {
            GameManager.Instance.ownerPlayer.GetComponent<PlayerController>().lookSensitivity = value;
        }
    }

    //public void SetInvertY(bool invert)
    //{
    //    PlayerPrefs.SetInt(INVERT_Y, invert ? 1 : 0);
    //    GameManager.Instance.invertY = invert;
    //}

    void LoadGameplaySettings()
    {
        float sens = PlayerPrefs.GetFloat(SENSITIVITY, 1f);
        //bool invert = PlayerPrefs.GetInt(INVERT_Y, 0) == 1;

        mouseSensitivitySlider.value = sens;
        //invertYToggle.isOn = invert;

        SetMouseSensitivity(sens);
        //SetInvertY(invert);
    }
    #endregion

    public void ResetSettings()
    {
        PlayerPrefs.DeleteAll();
        Awake();
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIRelated : MonoBehaviour
{
    [Header("For Minimap UI")]
    public RectTransform FullMapBackground;
    public CanvasGroup FullMap;
    public Image FullMapBackgroundImage;
    public TMP_Text FullMapText;
    public float height;
    public float originalHeight;
    public float decAlpha;
    public float originalAlpha;
    public Color normalColor;
    [ColorUsage(true, true)] public Color redColor;
    public float disabledAlpha = 0.2f;

    [Header("Animator References")]
    public Animator MinimapAnim;

    [Header("Values")]
    public bool isFullMiniMapShowing;
    public float fractionTime;
    public float fractionTimeLimit = 0.2f;

    [Header("Times")]
    public float timeLeftToShowMiniMap;
    public float totalTimeToShowMap = 10;
    public float rechargeRate = 0.02f;


    private void Start()
    {
        timeLeftToShowMiniMap = totalTimeToShowMap;
        originalHeight = FullMapBackground.sizeDelta.y;
        originalAlpha = FullMapText.alpha;
        normalColor = FullMapBackgroundImage.color;
    }

    private void Update()
    {
        fractionTime = timeLeftToShowMiniMap / totalTimeToShowMap;
        if (Input.GetKeyDown(KeyCode.Tab) && fractionTime > fractionTimeLimit)
        {
            LoadMinimapPanel(true);
        }

        if (Input.GetKeyUp(KeyCode.Tab) || timeLeftToShowMiniMap <= 0)
        {
            LoadMinimapPanel(false);
        }

        if (isFullMiniMapShowing)
        {
            timeLeftToShowMiniMap -= Time.deltaTime;
        }
        
        if (!isFullMiniMapShowing)
        {
            timeLeftToShowMiniMap += Time.deltaTime * rechargeRate;
            timeLeftToShowMiniMap = Mathf.Clamp(timeLeftToShowMiniMap, 0, totalTimeToShowMap);
        }

        height = fractionTime * originalHeight;
        FullMapBackground.sizeDelta = new(FullMapBackground.sizeDelta.x, height);

        decAlpha = fractionTime * originalAlpha;
        //FullMapText.alpha = decAlpha;

        if (isFullMiniMapShowing)
        {
            FullMapBackgroundImage.GetComponent<Image>().color = redColor;
            FullMap.alpha = 1;
        }
        else if (fractionTime < fractionTimeLimit)
        {
            FullMapBackgroundImage.GetComponent<Image>().color = redColor;
            FullMap.alpha = disabledAlpha;
        }
        else
        {
            FullMapBackgroundImage.GetComponent<Image>().color = normalColor;
            FullMap.alpha = 1;
        }


        FullMapText.text = $"Full Map : {Mathf.Round(fractionTime * 10 * 100f) / 100f}";
    }

    public void LoadMinimapPanel(bool Load)
    {
        isFullMiniMapShowing = Load;
        MinimapAnim.SetBool("Load", Load);

        if (Load)
        {
            AudioManager.PlaySound(AudioType.PanelOpen);
        }else
        {
            AudioManager.PlaySound(AudioType.PanelClose);
        }
    }

    // For After Death Or Win
    public void LoadMainMenuScene()
    {
        SceneManager.LoadSceneAsync("Main Menu");
        GameManager.Instance.LoadingPanel.SetActive(true);
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRelated : MonoBehaviour
{
    [Header("For Minimap UI")]
    public RectTransform FullMapBackground;
    public Image FullMapBackgroundImage;
    public TMP_Text FullMapText;
    public float height;
    public float originalHeight;
    public float decAlpha;
    public float originalAlpha;
    public Color normalColor;
    [ColorUsage(true, true)]public Color redColor;

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
        if (Input.GetKeyUp(KeyCode.Tab) && fractionTime > fractionTimeLimit)
        {
            LoadMinimapPanel(!isFullMiniMapShowing);
        } else if (timeLeftToShowMiniMap <= 0 || Input.GetKeyUp(KeyCode.Tab))
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

        if (fractionTime < fractionTimeLimit || isFullMiniMapShowing)
        {
            FullMapBackgroundImage.GetComponent<Image>().color = redColor;
        }else
        {
            FullMapBackgroundImage.GetComponent<Image>().color = normalColor;
        }

        FullMapText.text = $"Full Map : {Mathf.Round(fractionTime * 10 * 100f) / 100f}";
    }

    public void LoadMinimapPanel(bool Load)
    {
        isFullMiniMapShowing = Load;
        MinimapAnim.SetBool("Load", Load);
    }
}

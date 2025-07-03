using UnityEngine;

public class UIRelated : MonoBehaviour
{
    [Header("References")]
    public Animator MinimapAnim;

    [Header("Values")]
    public bool isFullMiniMapShowing;

    [Header("Times")]
    public float timeMiniMapShown;
    public float totalTimeToShowMap = 10;
    public float rechargeRate = 0.02f;


    private void Start()
    {
        timeMiniMapShown = totalTimeToShowMap;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Tab) && timeMiniMapShown > 2)
        {
            LoadMinimapPanel(!isFullMiniMapShowing);
        }else if (timeMiniMapShown < 2)
        {
            LoadMinimapPanel(false);
        }

        if (isFullMiniMapShowing)
        {
            timeMiniMapShown -= Time.deltaTime;
        }
        
        if (!isFullMiniMapShowing)
        {
            timeMiniMapShown += Time.deltaTime * rechargeRate;
        }
    }

    public void LoadMinimapPanel(bool Load)
    {
        isFullMiniMapShowing = Load;
        MinimapAnim.SetBool("Load", Load);
    }
}

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.WSA;

public class ItemFrameUI : MonoBehaviour
{
    public Image frame;
    public GameObject itemAmountAndNameUI;
    public float animTime = 0.5f;
    public bool activated;

    public void Activate()
    {
        if (!activated)
        {
            activated = true;
            frame.color = new Color(1, 1, 1, 1);
            itemAmountAndNameUI.SetActive(true);
            LeanTween.alpha(frame.GetComponent<RectTransform>(), 0, animTime).setLoopPingPong().setEase(LeanTweenType.easeInOutExpo);
        }
    }

    public void Deactivate()
    {
        if (activated)
        {
            activated = false;
            frame.color = new Color(1, 1, 1, 0);
            itemAmountAndNameUI.SetActive(false);
            LeanTween.cancel(frame.gameObject);
        }
    }
}

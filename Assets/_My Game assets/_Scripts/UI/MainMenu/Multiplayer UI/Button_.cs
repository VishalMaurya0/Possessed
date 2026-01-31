using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Button_ : MonoBehaviour, IPointerEnterHandler
{
    Button button;
    Toggle toggle;

    private void Start()
    {
        button = GetComponent<Button>();
        toggle = GetComponent<Toggle>();

        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }

        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnToggleChanged);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.IsInteractable())
        {
            AudioManager.PlaySound(AudioType.UnClick);
        }
    }

    void OnButtonClick()
    {
        AudioManager.PlaySound(AudioType.Click);
    }

    void OnToggleChanged(bool value)
    {
        AudioManager.PlaySound(AudioType.Click);
    }
}

using UnityEngine;

public class TabButton : MonoBehaviour
{
    public TabPanelAnimator animator;
    public int panelIndex;

    public void OnTabClicked()
    {
        animator.ShowPanel(panelIndex);
    }
}

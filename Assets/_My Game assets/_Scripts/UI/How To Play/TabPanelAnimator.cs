using UnityEngine;
using System.Collections;

public class TabPanelAnimator : MonoBehaviour
{
    [System.Serializable]
    public class Panel
    {
        public RectTransform rect;
        public CanvasGroup canvasGroup;
    }

    [Header("Animation Settings")]
    public float slideDistance = 80f;
    public float duration = 0.35f;
    public AnimationCurve easeCurve;

    Animator animator;

    [Header("Panels")]
    public Panel[] panels;

    Panel currentPanel;
    Coroutine animationRoutine;

    void Start()
    {
        animator = GetComponent<Animator>();


        // Disable all panels at start
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].rect.gameObject.SetActive(false);
            panels[i].canvasGroup.alpha = 0f;

        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            JPress();
        }

        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            JPress(true);
        }

        
    }

    public void JPress(bool esc = false)
    {
        if (esc) 
        {
            animator.SetBool("Load", false);
            
            {
                //GameManager.Instance.lockCurser = true;
                //GameManager.Instance.handlePlayerLookWithMouse = true;
                AudioManager.PlaySound(AudioType.PanelClose);
            }

            return;
        }
        animator.SetBool("Load", !animator.GetBool("Load"));
        if (animator.GetBool("Load"))
        {
            GameManager.Instance.lockCurser = false;
            GameManager.Instance.handlePlayerLookWithMouse = false;
            AudioManager.PlaySound(AudioType.PanelOpen);
        }
        else
        {
            GameManager.Instance.lockCurser = true;
            GameManager.Instance.handlePlayerLookWithMouse = true;
            AudioManager.PlaySound(AudioType.PanelClose);
        }
    }

    public void ShowPanel(int panelIndex)
    {

        if (panelIndex < 0 || panelIndex >= panels.Length)
        {
            return;
        }

        Panel nextPanel = panels[panelIndex];

        if (currentPanel == nextPanel)
        {
            return;
        }

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        animationRoutine = StartCoroutine(SwitchPanel(currentPanel, nextPanel));
        currentPanel = nextPanel;
    }

    IEnumerator SwitchPanel(Panel from, Panel to)
    {

        if (from != null)
        {
            yield return StartCoroutine(AnimateOut(from));
        }

        yield return StartCoroutine(AnimateIn(to));

    }

    IEnumerator AnimateIn(Panel panel)
    {

        panel.rect.gameObject.SetActive(true);

        float t = 0f;
        Vector2 startPos = new Vector2(0, -slideDistance);
        Vector2 endPos = Vector2.zero;

        panel.rect.anchoredPosition = startPos;
        panel.canvasGroup.alpha = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float eased = easeCurve.Evaluate(t);

            panel.rect.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
            panel.canvasGroup.alpha = eased;

            yield return null;
        }

        panel.rect.anchoredPosition = endPos;
        panel.canvasGroup.alpha = 1f;

    }

    IEnumerator AnimateOut(Panel panel)
    {

        float t = 0f;
        Vector2 startPos = panel.rect.anchoredPosition;
        Vector2 endPos = new Vector2(0, -slideDistance);

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float eased = easeCurve.Evaluate(t);

            panel.rect.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
            panel.canvasGroup.alpha = 1f - eased;

            yield return null;
        }

        panel.canvasGroup.alpha = 0f;
        panel.rect.gameObject.SetActive(false);

    }
}

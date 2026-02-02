using UnityEngine;
using System.Collections;

public class TabPanelAnimator_HorizontalStepper : MonoBehaviour
{
    [System.Serializable]
    public class Panel
    {
        public RectTransform rect;
        public CanvasGroup canvasGroup;
    }

    [Header("Animation Settings")]
    public float slideDistance = 120f;
    public float duration = 0.35f;
    public AnimationCurve easeCurve;


    [Header("Panels")]
    public Panel[] panels;
    public GameObject[] panelsGO;

    int currentIndex = -1;
    Coroutine animationRoutine;

    enum SlideDirection { Left, Right }

    void Start()
    {
        panels = new Panel[panelsGO.Length];
        for (int i = 0; i < panelsGO.Length; i++)
        {
            RectTransform rt = panelsGO[i].transform.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = panelsGO[i].transform.GetComponent<CanvasGroup>();

            panels[i] = new Panel();
            panels[i].canvasGroup = canvasGroup;
            panels[i].rect = rt;
        }

        for (int i = 0; i < panels.Length; i++)
        {
            if (i == 0) continue;
            panels[i].rect.gameObject.SetActive(false);
            panels[i].canvasGroup.alpha = 0f;
        }

        currentIndex = 0;

        // Optional: start from first panel
        //StartCoroutine(ShowFirstPanel());

    }

    //IEnumerator ShowFirstPanel()
    //{
    //    yield return null;

    //    currentIndex = 0;
    //    panels[0].rect.gameObject.SetActive(true);
    //    panels[0].rect.anchoredPosition = Vector2.zero;
    //    panels[0].canvasGroup.alpha = 1f;
    //}



    public void ShowNext()
    {
        if (currentIndex >= panels.Length - 1)
            return;

        ShowPanel(currentIndex + 1, SlideDirection.Left);
    }

    public void ShowPrevious()
    {
        if (currentIndex <= 0)
            return;

        ShowPanel(currentIndex - 1, SlideDirection.Right);
    }

    void ShowPanel(int newIndex, SlideDirection direction)
    {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        Panel from = currentIndex >= 0 ? panels[currentIndex] : null;
        Panel to = panels[newIndex];

        animationRoutine = StartCoroutine(SwitchPanel(from, to, direction));
        currentIndex = newIndex;
    }

    IEnumerator SwitchPanel(Panel from, Panel to, SlideDirection direction)
    {
        if (from != null)
            yield return StartCoroutine(AnimateOut(from, direction));

        yield return StartCoroutine(AnimateIn(to, direction));
    }

    IEnumerator AnimateIn(Panel panel, SlideDirection direction)
    {
        panel.rect.gameObject.SetActive(true);

        float t = 0f;

        float startX = direction == SlideDirection.Left ? slideDistance : -slideDistance;
        Vector2 startPos = new Vector2(startX, 0);
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

    IEnumerator AnimateOut(Panel panel, SlideDirection direction)
    {
        float t = 0f;

        float endX = direction == SlideDirection.Left ? -slideDistance : slideDistance;
        Vector2 startPos = panel.rect.anchoredPosition;
        Vector2 endPos = new Vector2(endX, 0);

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

using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{

    InventorySlotTracker inventorySlotTracker;
    public ItemFrameUI itemFrameUI;
    Inventory inventory;
    public ItemAmountUI ItemAmountUI;

    [Header("Generated")]
    public RectTransform[] positions = new RectTransform[9];
    public GameObject[] iconPlaceholders = new GameObject[9];
    public GameObject[] icons = new GameObject[9];
    public GameObject iconPrefab;


    public bool resetIcons;
    public float animTime = 0.5f;
    public float decreasedAlpha = 0.6f;

    void Start()
    {
        inventorySlotTracker = GetComponent<InventorySlotTracker>();
        LeanTween.init(800); // Increase the number of available tweens

        InitializePostions();
    }

    private void Update()
    {
        if (inventory == null)
        {
            inventory = inventorySlotTracker.inventory;
        }
    }


    // Initialize the 8 icon placeholders and 8 icons=====//
    private void InitializePostions()       
    {
        //==========  ==========//
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = transform.GetChild(i).GetComponent<RectTransform>();
            iconPlaceholders[i] = Instantiate(iconPrefab, transform);
            icons[i] = Instantiate(iconPlaceholders[i], transform);
        }
    }


    // Set the icon placeholders and icons with the given item data, and adjust their position and opacity=====//
    public void InitializeIcon()
    {
        for (int i = 0; i < inventorySlotTracker.leftSlot.slots.Count; i++)
        {
            int j = i + inventorySlotTracker.leftSlot.slots.Count + 1;
            if (inventorySlotTracker.leftSlot.slots[i].isFull && inventorySlotTracker.leftSlot.slots[i].inventorySlot.itemData != null)        //=======if left slot is full =======//
            {
                ItemDataSO itemDataSO = ScriptableObjectFinder.FindItemSO(inventorySlotTracker.leftSlot.slots[i].inventorySlot.itemData);
                SetIconPlaceholders(positions[i], i, false, itemDataSO.icon);
            }
            if (inventorySlotTracker.rightSlot.slots[i].isFull && inventorySlotTracker.rightSlot.slots[i].inventorySlot.itemData != null)      //=======if right slot is full =======//
            {
                ItemDataSO itemDataSO = ScriptableObjectFinder.FindItemSO(inventorySlotTracker.rightSlot.slots[i].inventorySlot.itemData);
                SetIconPlaceholders(positions[j], j, false, itemDataSO.icon);
            }
            if (!inventorySlotTracker.leftSlot.slots[i].isFull)                                                                                //=======if left slot is not full =======//
            {
                SetIconPlaceholders(positions[i], i, true);
            }
            if (!inventorySlotTracker.rightSlot.slots[i].isFull)                                                                               //=======if right slot is not full =======//
            {
                SetIconPlaceholders(positions[j], j, true);
            }
        }

        int k = inventorySlotTracker.leftSlot.slots.Count;
        if (!inventorySlotTracker.currentSlot.slot.isFull)
        {
            SetIconPlaceholders(positions[k], k, true);
        }
        else if (inventorySlotTracker.currentSlot.slot.inventorySlot.itemData != null)
        {
            ItemDataSO itemDataSO = ScriptableObjectFinder.FindItemSO(inventorySlotTracker.currentSlot.slot.inventorySlot.itemData);
            SetIconPlaceholders(positions[k], k, false, itemDataSO.icon);
        }
    }

    private void SetIconPlaceholders(RectTransform rectTransform, int i, bool toDestroy, Sprite icon = null)
    {
        if (!toDestroy)
        {
            if (!iconPlaceholders[i].activeSelf)           //////=============== if no gameobj spawn it ===================//
            {
                iconPlaceholders[i].SetActive(true);
                Image img = iconPlaceholders[i].GetComponent<Image>();
                iconPlaceholders[i].GetComponent<RectTransform>().anchoredPosition = rectTransform.anchoredPosition;
                iconPlaceholders[i].GetComponent<RectTransform>().sizeDelta = rectTransform.sizeDelta;
                iconPlaceholders[i].name = "IconPlaceholder_" + i;
                iconPlaceholders[i].GetComponent<Image>().sprite = icon;
                iconPlaceholders[i].SetActive(true);
            }
            iconPlaceholders[i].GetComponent<RectTransform>().anchoredPosition = positions[i].anchoredPosition;
            iconPlaceholders[i].GetComponent<Image>().sprite = icon;
        }
        else
        {
            iconPlaceholders[i].SetActive(false);
        }


    }


    // ACtives the icon same as iconplaceholders but only if not to animate
    // while animation it animates then set again
    public void SetIcon(bool notToAnimate)
    {

        if (notToAnimate)
        {

            for (int i = 0; i < icons.Length; i++)
            {
                icons[i].SetActive(false);
            }

            for (int j = 0; j < icons.Length; j++)
            {

                if (iconPlaceholders[j].activeSelf && !icons[j].activeSelf)
                {
                    icons[j].SetActive(true);
                    RectTransform iconTransform = icons[j].GetComponent<RectTransform>();
                    RectTransform placeholderTransform = iconPlaceholders[j].GetComponent<RectTransform>();
                    Image iconImage = icons[j].GetComponent<Image>();
                    Image placeholderImage = iconPlaceholders[j].GetComponent<Image>();

                    iconTransform.anchoredPosition = placeholderTransform.anchoredPosition;
                    iconTransform.sizeDelta = placeholderTransform.sizeDelta;
                    iconImage.sprite = placeholderImage.sprite;
                    iconImage.color = Color.white;
                    icons[j].name = "Icon_" + j;

                    float alphaValue = (j != inventorySlotTracker.leftSlot.slots.Count) ? decreasedAlpha : 1f;
                    icons[j].GetComponent<CanvasGroup>().alpha = alphaValue;

                }
                else if (iconPlaceholders[j].activeSelf && icons[j].activeSelf)
                {
                    icons[j].GetComponent<Image>().sprite = iconPlaceholders[j].GetComponent<Image>().sprite;
                }
                else if (!iconPlaceholders[j].activeSelf)
                {
                    icons[j].SetActive(false);
                }
            }

            ///====== Animate the Frame Icon =======//
            bool showIcon = false;

            foreach (var icon in iconPlaceholders)
            {
                if (icon.activeSelf)
                {
                    showIcon = true;
                    break;
                }
            }

            if (showIcon)
            {
                itemFrameUI.Activate();
            }
            else
            {
                itemFrameUI.Deactivate();
            }
        }
        else
        {
            AnimateIcon___UIImage();
        }

        ItemAmountUI.UpdateTotalAmountAndNameUI();
    }

    public void AnimateIcon___UIImage()
    {

        int firstPlaceholderIndex = -1;
        int firstIconIndex = -1;

        // Find the first valid placeholder index
        for (int i = 0; i < iconPlaceholders.Length; i++)
        {
            if (iconPlaceholders[i].activeSelf)
            {
                firstPlaceholderIndex = i;
                break;
            }
        }

        // Find the first valid icon index
        for (int i = 0; i < icons.Length; i++)
        {
            if (icons[i].activeSelf)
            {
                firstIconIndex = i;
                break;
            }
        }


        if (firstPlaceholderIndex == -1 || firstIconIndex == -1)
        {
            return;
        }

        for (int i = firstIconIndex, j = firstPlaceholderIndex; i < icons.Length && j < iconPlaceholders.Length; i++, j++)
        {
            if (icons[i].activeSelf && iconPlaceholders[j].activeSelf)
            {

                RectTransform iconTransform = icons[i].GetComponent<RectTransform>();
                RectTransform placeholderTransform = iconPlaceholders[j].GetComponent<RectTransform>();
                CanvasGroup canvasGroup = icons[i].GetComponent<CanvasGroup>();

                if (canvasGroup == null)
                {
                    canvasGroup = icons[i].AddComponent<CanvasGroup>(); // Ensure CanvasGroup exists
                }

                LeanTween.move(iconTransform, placeholderTransform.anchoredPosition3D, animTime)
                    .setEase(LeanTweenType.easeOutExpo);

                LeanTween.size(iconTransform, placeholderTransform.sizeDelta, animTime)
                    .setEase(LeanTweenType.easeOutExpo);

                float targetAlpha = (j == inventorySlotTracker.leftSlot.slots.Count) ? 1f : decreasedAlpha;
                LeanTween.alphaCanvas(canvasGroup, targetAlpha, animTime / 5);

            }
        }

        LeanTween.delayedCall(animTime, () =>
        {
            if (!LeanTween.isTweening())
            {
                SetIcon(true);
            }
        });
    }





    public void ScrollUp___UIImage()
    {
        //if (inventory.slotNo.Value != 4)           //============ when slot has to go left only ========//
        //{
        //    for (int i = 1; i < inventorySlotTracker.leftSlot.slots.Count; i++)     // 0 [1] [2] [3] 4 5 6 7 8 //
        //    {
        //        GameObject icon = icons[i];
        //        if (icon != null)
        //        {
        //            LeanTween.move(icon.GetComponent<RectTransform>(), positions[i - 1].anchoredPosition3D, 0.5f).setEase(LeanTweenType.easeOutExpo);
        //        }
        //    }



        //    for (int i = inventorySlotTracker.leftSlot.slots.Count + 2; i < (GameManager.Instance.inventorySlots * 2) - 1; i++)     // 0 1 2 3 4 5 [6] [7] [8] //
        //    {
        //        GameObject icon = icons[i];
        //        if (icon != null)
        //        {
        //            LeanTween.move(icon.GetComponent<RectTransform>(), positions[i - 1].anchoredPosition3D, 0.5f).setEase(LeanTweenType.easeOutExpo);
        //        }
        //    }



        //    int j = inventorySlotTracker.rightSlot.slots.Count + 1;                 // 0 1 2 3 4 [5] 6 7 8 //
        //    GameObject iconToBeMain = icons[j];
        //    if (iconToBeMain != null)
        //    {
        //        LeanTween.move(iconToBeMain.GetComponent<RectTransform>(), positions[j - 1].anchoredPosition3D, 0.5f).setEase(LeanTweenType.easeOutExpo);
        //        LeanTween.scale(iconToBeMain.GetComponent<RectTransform>(), positions[j - 1].localScale, 0.5f).setEase(LeanTweenType.easeOutExpo);
        //    }



        //}
        //else                                        //========== when slot have to go full right ========//
        //{

        //}

    }

    
}

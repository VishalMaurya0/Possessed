using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{

    InventorySlotTracker inventorySlotTracker;
    Inventory inventory;


    public RectTransform[] positions = new RectTransform[9];
    public GameObject[] iconPlaceholders = new GameObject[9];
    public GameObject[] icons = new GameObject[9];
    public GameObject iconPrefab;


    public bool resetIcons;
    public float animTime = 0.5f;

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



    private void InitializePostions()
    {
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = transform.GetChild(i).GetComponent<RectTransform>();
        }
    }


    public void InitializeIcon(bool spawnIcons)
    {
        for (int i = 0; i < inventorySlotTracker.leftSlot.slots.Count; i++)
        {
            int j = i + inventorySlotTracker.leftSlot.slots.Count + 1;
            if (inventorySlotTracker.leftSlot.slots[i].isFull && inventorySlotTracker.leftSlot.slots[i].inventorySlot.itemData != null)        //=======if left slot is full =======//
            {
                ItemDataSO itemDataSO = ScriptableObjectFinder.FindItemSO(inventorySlotTracker.leftSlot.slots[i].inventorySlot.itemData);
                SetIconPlaceholders(positions[i], i, false, spawnIcons, itemDataSO.icon);
            }
            if (inventorySlotTracker.rightSlot.slots[i].isFull && inventorySlotTracker.rightSlot.slots[i].inventorySlot.itemData != null)      //=======if right slot is full =======//
            {
                ItemDataSO itemDataSO = ScriptableObjectFinder.FindItemSO(inventorySlotTracker.rightSlot.slots[i].inventorySlot.itemData);
                SetIconPlaceholders(positions[j], j, false, spawnIcons, itemDataSO.icon);
            }
            if (!inventorySlotTracker.leftSlot.slots[i].isFull)                                                                                //=======if left slot is not full =======//
            {
                SetIconPlaceholders(positions[i], i, true, spawnIcons);
            }
            if (!inventorySlotTracker.rightSlot.slots[i].isFull)                                                                               //=======if right slot is not full =======//
            {
                SetIconPlaceholders(positions[j], j, true, spawnIcons);
            }
        }

        int k = inventorySlotTracker.leftSlot.slots.Count;
        if (!inventorySlotTracker.currentSlot.slot.isFull)
        {
            SetIconPlaceholders(positions[k], k, true, spawnIcons);
        }
        else if (inventorySlotTracker.currentSlot.slot.inventorySlot.itemData != null)
        {
            ItemDataSO itemDataSO = ScriptableObjectFinder.FindItemSO(inventorySlotTracker.currentSlot.slot.inventorySlot.itemData);
            SetIconPlaceholders(positions[k], k, false, spawnIcons, itemDataSO.icon);
        }
    }

    private void SetIconPlaceholders(RectTransform rectTransform, int i, bool toDestroy, bool spawnIcons, Sprite icon = null)
    {
        if (!toDestroy)
        {
            if (iconPlaceholders[i] == null)           //////=============== if no gameobj spawn it ===================//
            {
                GameObject itemIcon = Instantiate(iconPrefab, transform);
                Image img = itemIcon.GetComponent<Image>();
                itemIcon.GetComponent<RectTransform>().anchoredPosition = rectTransform.anchoredPosition;
                itemIcon.GetComponent<RectTransform>().sizeDelta = rectTransform.sizeDelta;
                itemIcon.name = "IconPlaceholder_" + i;
                itemIcon.GetComponent<Image>().sprite = icon;
                itemIcon.SetActive(true);
                iconPlaceholders[i] = itemIcon;
            }
            iconPlaceholders[i].GetComponent<RectTransform>().anchoredPosition = positions[i].anchoredPosition;
        }
        else
        {
            Destroy(iconPlaceholders[i]);
            iconPlaceholders[i] = null;
        }


    }

    public void SetIcon(bool spawnIcons)
    {

        if (spawnIcons)
        {
            for (int i = 0; i < icons.Length; i++)
            {
                Destroy(icons[i]);
                icons[i] = null;
            }
            for (int j = 0; j < icons.Length; j++)
            {
                if (iconPlaceholders[j] != null && icons[j] == null)
                {
                    GameObject itemIcon = Instantiate(iconPlaceholders[j], transform);
                    itemIcon.GetComponent<Image>().color = Color.white;
                    itemIcon.name = "Icon_" + j;
                    icons[j] = itemIcon;
                }
                else if (iconPlaceholders[j] != null && icons[j] != null)
                {
                    icons[j].GetComponent<Image>().sprite = iconPlaceholders[j].GetComponent<Image>().sprite;
                }
                else if (iconPlaceholders[j] == null)
                {
                    Destroy(icons[j]);
                    icons[j] = null;
                }

            }
        }
        else
        {
            AnimateIcon___UIImage();
        }
    }



    public void AnimateIcon___UIImage()
    {
        int firstPlaceholderIndex = -1;
        int firstIconIndex = -1;

        // Find the first valid placeholder index
        for (int i = 0; i < iconPlaceholders.Length; i++)
        {
            if (iconPlaceholders[i] != null)
            {
                firstPlaceholderIndex = i;
                break;
            }
        }

        // Find the first valid icon index
        for (int i = 0; i < icons.Length; i++)
        {
            if (icons[i] != null)
            {
                firstIconIndex = i;
                break;
            }
        }

        // Check if we found valid indices before proceeding
        if (firstPlaceholderIndex == -1 || firstIconIndex == -1)
        {
            return;
        }

        // Start animation
        for (int i = firstIconIndex, j = firstPlaceholderIndex; i < icons.Length && j < iconPlaceholders.Length; i++, j++)
        {
            if (icons[i] != null && iconPlaceholders[j] != null)
            {
                LeanTween.cancel(icons[i]); // Cancel any existing tween to prevent stacking

                // Move animation
                LeanTween.move(icons[i].GetComponent<RectTransform>(), iconPlaceholders[j].GetComponent<RectTransform>().anchoredPosition3D, animTime)
                    .setEase(LeanTweenType.easeOutExpo);

                // Scale animation
                LeanTween.size(icons[i].GetComponent<RectTransform>(), iconPlaceholders[j].GetComponent<RectTransform>().sizeDelta, animTime)
                    .setEase(LeanTweenType.easeOutExpo);
            }
        }

        LeanTween.delayedCall(animTime, () =>
        {
            SetIcon(true);
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

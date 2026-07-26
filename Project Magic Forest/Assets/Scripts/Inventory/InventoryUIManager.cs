using System.Collections.Generic;
using UnityEngine;

public sealed class InventoryUIManager : MonoBehaviour
{
    [System.Serializable]
    private sealed class ItemUiMapping
    {
        [SerializeField] public ItemDefinition item;
        [SerializeField] public GameObject uiElement;
    }

    [SerializeField] private List<ItemUiMapping> itemUiMappings = new();
    private PlayerInventory playerInventory;

    private void Start()
    {
        playerInventory = FindFirstObjectByType<PlayerInventory>();
        if (playerInventory != null)
        {
            playerInventory.ItemAmountChanged += OnItemAmountChanged;
        }
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.ItemAmountChanged -= OnItemAmountChanged;
        }
    }

    private void OnItemAmountChanged(ItemDefinition item, float newAmount)
    {
        if (item == null || newAmount <= 0f)
        {
            return;
        }

        // Find and show the UI element for this item
        foreach (ItemUiMapping mapping in itemUiMappings)
        {
            if (mapping.item == item && mapping.uiElement != null)
            {
                mapping.uiElement.SetActive(true);
                return;
            }
        }
    }
}

using UnityEngine;

public sealed class GetItemInteractable : Interactable
{
    [SerializeField] private ItemDefinition item;
    [SerializeField] private float amount = 1f;

    public override bool TryInteract(PlayerInventory inventory, GameObject player)
    {
        if (!base.TryInteract(inventory, player))
        {
            return false;
        }

        if (inventory == null || item == null)
        {
            return false;
        }

        bool added = inventory.Add(item, amount);
        
        if (added)
        {
            Debug.Log($"[GetItemInteractable] Obtained {amount} {item.DisplayName}");
            Destroy(gameObject);
        }

        return added;
    }
}

using UnityEngine;

public sealed class PoiContext
{
    public PoiContext(PlayerInventory inventory, GameObject player, Vector3 worldPosition)
    {
        Inventory = inventory;
        Player = player;
        WorldPosition = worldPosition;
    }

    public PlayerInventory Inventory { get; }
    public GameObject Player { get; }
    public Vector3 WorldPosition { get; }
}

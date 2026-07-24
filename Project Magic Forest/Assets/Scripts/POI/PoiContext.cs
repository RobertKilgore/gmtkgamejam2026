using UnityEngine;

public sealed class PoiContext
{
    public PoiContext(PlayerInventory inventory, GameObject player, Vector3 worldPosition, PlayerTimers playerTimers)
    {
        Inventory = inventory;
        Player = player;
        WorldPosition = worldPosition;
        PlayerTimers = playerTimers;
    }

    public PlayerInventory Inventory { get; }
    public GameObject Player { get; }
    public Vector3 WorldPosition { get; }
    public PlayerTimers PlayerTimers { get; }
}

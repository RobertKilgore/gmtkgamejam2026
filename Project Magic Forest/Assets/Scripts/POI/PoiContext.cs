using UnityEngine;

public sealed class PoiContext
{
    public PoiContext(PlayerInventory inventory, GameObject player, Vector3 worldPosition, PlayerTimers playerTimers, PoiBehaviour poiBehaviour = null)
    {
        Inventory = inventory;
        Player = player;
        WorldPosition = worldPosition;
        PlayerTimers = playerTimers;
        PoiBehaviour = poiBehaviour;
    }

    public PlayerInventory Inventory { get; }
    public GameObject Player { get; }
    public Vector3 WorldPosition { get; }
    public PlayerTimers PlayerTimers { get; }
    public PoiBehaviour PoiBehaviour { get; }
}

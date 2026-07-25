
using UnityEngine;

public class ReturnCabinRoof : MonoBehaviour
{
    public GameObject roof;
    public GameObject outdoorDimmer;
    public GameObject interior;
    public GameObject player;
    public string playerTag = "Player";
    public string interiorSortingLayer = "Default";

    private SpriteRenderer sr_roof;
    private BoxCollider2D bc_roof;

    private void Awake()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag(playerTag);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnsurePlayerExists();

        SetInteriorSortingOrder(-1);
        if (player != null)
        {
            var playerSprite = player.GetComponent<SpriteRenderer>();
            if (playerSprite != null)
            {
                playerSprite.sortingOrder = 1;
            }
        }

        sr_roof = roof != null ? roof.GetComponent<SpriteRenderer>() : null;
        if (sr_roof != null)
        {
            sr_roof.enabled = true;
        }

        bc_roof = roof != null ? roof.GetComponent<BoxCollider2D>() : null;
        if (bc_roof != null)
        {
            bc_roof.enabled = true;
        }

        if (outdoorDimmer != null)
        {
            outdoorDimmer.SetActive(false);
        }
        // Re-enable interior colliders when returning the roof
        if (interior != null)
        {
            var interiorColliders = interior.GetComponentsInChildren<Collider2D>(true);
            foreach (var c in interiorColliders)
            {
                if (c != null)
                    c.enabled = false;
            }
        }
    }

    private void EnsurePlayerExists()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag(playerTag);
        }
    }

    private void SetInteriorSortingOrder(int order)
    {
        SetInteriorSortingLayerAndOrder(interiorSortingLayer, order);
    }

    private void SetInteriorSortingLayerAndOrder(string layerName, int order)
    {
        if (interior == null)
        {
            return;
        }

        var sortingGroups = interior.GetComponentsInChildren<UnityEngine.Rendering.SortingGroup>(true);
        foreach (var sortingGroup in sortingGroups)
        {
            if (sortingGroup != null)
            {
                sortingGroup.sortingLayerName = layerName;
                sortingGroup.sortingOrder = order;
            }
        }

        var renderers = interior.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.sortingLayerName = layerName;
                renderer.sortingOrder = order;
            }
        }
    }
}



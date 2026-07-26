
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
    private BoxCollider2D[] bc_roof;
    private CameraManager cameraManager;

    private void Awake()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag(playerTag);
        }

        if (cameraManager == null)
        {
            cameraManager = FindFirstObjectByType<CameraManager>(FindObjectsInactive.Include);
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

        bc_roof = roof != null ? roof.GetComponents<BoxCollider2D>() : null;

        if (outdoorDimmer != null)
        {
            outdoorDimmer.SetActive(false);
        }

        if (bc_roof != null)
        {
            foreach (BoxCollider2D collider in bc_roof)
            {
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }
        }

        if (interior != null)
        {
            var interiorColliders = interior.GetComponentsInChildren<Collider2D>(true);
            foreach (var c in interiorColliders)
            {
                if (c != null)
                    c.enabled = false;
            }
        }

        cameraManager?.ActivatePlayerCamera();
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



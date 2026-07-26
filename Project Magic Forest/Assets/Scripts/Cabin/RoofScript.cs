using UnityEngine;

public class RoofScript : MonoBehaviour
{
    public GameObject roof;
    public GameObject interior;
    public GameObject player;
    public GameObject outdoorDimmer;
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

    void OnTriggerEnter2D(Collider2D other)
    {
        EnsurePlayerExists();

        SetInteriorSortingOrder(3);
        if (player != null)
        {
            var playerSprite = player.GetComponent<SpriteRenderer>();
            if (playerSprite != null)
            {
                playerSprite.sortingOrder = 3;
            }
        }

        sr_roof = roof != null ? roof.GetComponent<SpriteRenderer>() : null;
        if (sr_roof != null)
        {
            sr_roof.enabled = false;
        }

        bc_roof = roof != null ? roof.GetComponents<BoxCollider2D>() : null;
        if (outdoorDimmer != null)
        {
            outdoorDimmer.SetActive(true);
        }

        if (bc_roof != null)
        {
            foreach (BoxCollider2D collider in bc_roof)
            {
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }
        // Disable interior colliders so player can enter interior without colliding with interior geometry
        if (interior != null)
        {
            var interiorColliders = interior.GetComponentsInChildren<Collider2D>(true);
            foreach (var c in interiorColliders)
            {
                if (c != null)
                    c.enabled = true;
            }
        }

        cameraManager?.ActivateCabinCamera();
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

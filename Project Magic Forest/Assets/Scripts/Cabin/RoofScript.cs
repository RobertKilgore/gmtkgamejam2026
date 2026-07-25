using UnityEngine;

public class RoofScript : MonoBehaviour
{
    public GameObject roof;
    public GameObject interior;
    public GameObject player;
    public GameObject outdoorDimmer;
    public GameObject doorGlow;
    public string playerTag = "Player";

    private SpriteRenderer sr_roof;
    private BoxCollider2D[] bc_roof;

    private void Awake()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag(playerTag);
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
                playerSprite.sortingOrder = 4;
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
        if (doorGlow != null)
        {
            doorGlow.SetActive(false);
            Debug.Log("[RoofScript] Door glow disabled");
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
        if (interior == null)
        {
            return;
        }

        var sortingGroup = interior.GetComponent<UnityEngine.Rendering.SortingGroup>();
        if (sortingGroup != null)
        {
            Debug.Log($"[RoofScript] Setting interior sorting order to {order}");
            sortingGroup.sortingOrder = order;
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class FairyCircleInteractable : Interactable
{
    [Header("Fairy Circle")]
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite usedSprite;
    [SerializeField] private string homeTag = "home";

    [Header("Audio")]
    [SerializeField] private AudioClips audioClips;
    [SerializeField] private AudioClip teleportSound;
    [SerializeField, Range(0f, 1f)] private float teleportVolume = 1f;

    private SpriteRenderer spriteRenderer;
    private bool hasTeleported;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        RefreshState();
    }

    public override bool CanInteract()
    {
        return base.CanInteract() && !hasTeleported;
    }

    protected override void HandleInteraction(PlayerInventory inventory, GameObject player)
    {
        if (hasTeleported)
        {
            return;
        }

        if (player == null)
        {
            Debug.LogWarning("[FairyCircleInteractable] Missing player reference.");
            return;
        }

        GameObject home = FindHomeObject();
        if (home == null)
        {
            Debug.LogWarning("[FairyCircleInteractable] Home object not found for teleport.");
            return;
        }

        if (!TeleportPlayer(player, home.transform.position))
        {
            return;
        }

        PlayTeleportAudio(home.transform.position);
        hasTeleported = true;
        RefreshState();
        base.HandleInteraction(inventory, player);
    }

    private GameObject FindHomeObject()
    {
        try
        {
            return GameObject.FindGameObjectWithTag(homeTag);
        }
        catch (UnityException)
        {
            return null;
        }
    }

    private bool TeleportPlayer(GameObject player, Vector3 destination)
    {
        if (player == null)
        {
            return false;
        }

        Vector3 targetPosition = new Vector3(destination.x, destination.y, player.transform.position.z);
        player.transform.position = targetPosition;
        return true;
    }

    private void PlayTeleportAudio(Vector3 sourcePosition)
    {
        AudioClip clipToPlay = teleportSound;
        if (clipToPlay == null && audioClips != null)
        {
            clipToPlay = audioClips.TeleportSound;
        }

        if (clipToPlay == null)
        {
            Debug.LogWarning("[FairyCircleInteractable] No teleport sound assigned.");
            return;
        }

        AudioManager.PlaySFXAtPoint(clipToPlay, sourcePosition, teleportVolume);
    }

    private void RefreshState()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.sprite = hasTeleported ? usedSprite : activeSprite;
    }
}

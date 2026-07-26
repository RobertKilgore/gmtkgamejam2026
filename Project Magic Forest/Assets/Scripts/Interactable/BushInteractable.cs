using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class BushInteractable : Interactable
{
    public enum BushMode
    {
        SpriteAnimation,
        DestroyObjects
    }

    [SerializeField] private BushMode mode = BushMode.SpriteAnimation;
    [SerializeField] private float foodToAdd = 30f;

    [Header("Sprite Animation Mode")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<Sprite> animationSprites = new List<Sprite>();
    [SerializeField] private float framesPerSecond = 10f;

    [Header("Destroy Objects Mode")]
    [SerializeField] private List<GameObject> objectsToDestroyInOrder = new List<GameObject>();
    [SerializeField] private float destroyFramesPerSecond = 10f;

    private PlayerTimers playerTimers;
    private bool isInteracting;

    private void Start()
    {
        if (spriteRenderer == null && mode == BushMode.SpriteAnimation)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }
    }

    public override bool TryInteract(PlayerInventory inventory, GameObject player)
    {
        if (!base.TryInteract(inventory, player))
        {
            return false;
        }

        if (isInteracting)
        {
            return false;
        }

        // Find PlayerTimers if not cached
        if (playerTimers == null && player != null)
        {
            playerTimers = player.GetComponent<PlayerTimers>();
            if (playerTimers == null)
            {
                playerTimers = player.GetComponentInChildren<PlayerTimers>(true);
            }
        }

        isInteracting = true;

        if (mode == BushMode.SpriteAnimation)
        {
            StartCoroutine(PlayAnimationAndGiveFood());
        }
        else if (mode == BushMode.DestroyObjects)
        {
            StartCoroutine(DestroyObjectsAndGiveFood());
        }

        return true;
    }

    private IEnumerator PlayAnimationAndGiveFood()
    {
        if (spriteRenderer != null && animationSprites.Count > 0)
        {
            float frameDuration = 1f / Mathf.Max(1f, framesPerSecond);

            for (int i = 0; i < animationSprites.Count; i++)
            {
                if (spriteRenderer != null && animationSprites[i] != null)
                {
                    spriteRenderer.sprite = animationSprites[i];
                }

                yield return new WaitForSeconds(frameDuration);
            }
        }

        GiveFood();
        SetHighlighted(false);
        enabled = false;
    }

    private IEnumerator DestroyObjectsAndGiveFood()
    {
        float frameDuration = 1f / Mathf.Max(1f, destroyFramesPerSecond);

        for (int i = 0; i < objectsToDestroyInOrder.Count; i++)
        {
            if (objectsToDestroyInOrder[i] != null)
            {
                Debug.Log($"[BushInteractable] Destroying object: {objectsToDestroyInOrder[i].name}");
                Destroy(objectsToDestroyInOrder[i]);
            }

            yield return new WaitForSeconds(frameDuration);
        }

        GiveFood();
        SetHighlighted(false);
        enabled = false;
    }

    private void GiveFood()
    {
        if (playerTimers != null)
        {
            Timer foodTimer = playerTimers.FindTimer("Food");
            if (foodTimer != null)
            {
                foodTimer.AddTime(foodToAdd);
                Debug.Log($"[BushInteractable] Added {foodToAdd} seconds to Food timer");
            }
            else
            {
                Debug.LogWarning("[BushInteractable] Food timer not found in PlayerTimers");
            }
        }
        else
        {
            Debug.LogWarning("[BushInteractable] PlayerTimers not found");
        }
    }
}

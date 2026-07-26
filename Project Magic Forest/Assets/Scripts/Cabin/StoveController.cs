using System.Collections.Generic;
using UnityEngine;

public class StoveController : MonoBehaviour
{
    [Header("Fuel")]
    [SerializeField] private GameObject player;
    [SerializeField] private string fuelTimerKey = "Fuel";

    [Header("Sprite State")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private List<Sprite> onStateFrames = new List<Sprite>();
    [SerializeField] private float frameDuration = 0.15f;

    [Header("Door Glow Lights")]
    [SerializeField] private GameObject targetObject;
    [SerializeField] private GameObject doorGlowOut;
    [SerializeField] private GameObject doorGlowIn;
    [SerializeField] private bool allowDoorGlowOut = true;
    [SerializeField] private TimerModifierArea modifierArea;

    private PlayerTimers playerTimers;
    private Timer fuelTimer;
    private int frameIndex;
    private bool isAnimating;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        ResolvePlayerTimers();
    }

    private void Start()
    {
        ResolveFuelTimer();
        RefreshDoorGlowState();
        RefreshSpriteState();
    }

    private void Update()
    {
        RefreshDoorGlowState();
        RefreshSpriteState();
    }

    public void SetDoorGlowSelection(GameObject referenceObject)
    {
        targetObject = referenceObject;
        RefreshDoorGlowState();
    }

    private void RefreshDoorGlowState()
    {
        bool hasFuel = HasFuel();
        bool referenceActive = targetObject != null && targetObject.TryGetComponent<SpriteRenderer>(out var spriteRenderer) && spriteRenderer.enabled;

        bool useDoorGlowOut = hasFuel && referenceActive && allowDoorGlowOut;
        bool useDoorGlowIn = hasFuel && !referenceActive;

        SetDoorGlowOutEnabled(useDoorGlowOut);
        SetDoorGlowInEnabled(useDoorGlowIn);

        if (modifierArea != null)
        {
            modifierArea.SetAreaActive(hasFuel);
        }
    }

    private void RefreshSpriteState()
    {
        bool hasFuel = HasFuel();

        if (!hasFuel)
        {
            isAnimating = false;
            if (spriteRenderer != null && offSprite != null)
            {
                spriteRenderer.sprite = offSprite;
            }
            return;
        }

        if (!isAnimating && onStateFrames != null && onStateFrames.Count > 0)
        {
            isAnimating = true;
            frameIndex = 0;
        }

        if (isAnimating && spriteRenderer != null && onStateFrames != null && onStateFrames.Count > 0)
        {
            if (Time.frameCount % Mathf.Max(1, Mathf.RoundToInt(frameDuration * 60f)) == 0)
            {
                spriteRenderer.sprite = onStateFrames[frameIndex];
                frameIndex = (frameIndex + 1) % onStateFrames.Count;
            }
        }
    }

    private void SetDoorGlowOutEnabled(bool enabled)
    {
        if (doorGlowOut == null)
        {
            return;
        }

        doorGlowOut.SetActive(enabled);
    }

    private void SetDoorGlowInEnabled(bool enabled)
    {
        if (doorGlowIn == null)
        {
            return;
        }

        doorGlowIn.SetActive(enabled);
    }

    public bool HasFuel()
    {
        if (fuelTimer == null)
        {
            ResolveFuelTimer();
        }

        if (fuelTimer == null)
        {
            return false;
        }

        return fuelTimer.TimeRemaining > 0f;
    }

    private void ResolveFuelTimer()
    {
        if (playerTimers == null)
        {
            ResolvePlayerTimers();
        }

        if (playerTimers == null)
        {
            fuelTimer = null;
            return;
        }

        fuelTimer = playerTimers.FindTimer(fuelTimerKey);
    }

    private void ResolvePlayerTimers()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player == null)
        {
            return;
        }

        playerTimers = player.GetComponent<PlayerTimers>();
        if (playerTimers == null)
        {
            playerTimers = player.GetComponentInChildren<PlayerTimers>(true);
        }
    }
}

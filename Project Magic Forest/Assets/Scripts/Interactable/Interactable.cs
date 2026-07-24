using System;
using UnityEngine;

public enum InteractMode
{
    Click,
    Proximity,
    ButtonPress,
    ClickAndButton
}

public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private InteractMode interactMode = InteractMode.Click;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private GameObject buttonPrompt;
    [SerializeField] private bool showPromptOnlyForButtonMode = true;
    [SerializeField] private float maxHighlightDistance = 2.5f;

    private bool isHighlighted;
    private bool isActive;
    private int currentStateIndex;

    public event Action<Interactable> HighlightChanged;
    public event Action<Interactable> Clicked;

    public bool IsHighlighted
    {
        get => isHighlighted;
        protected set
        {
            if (isHighlighted != value)
            {
                isHighlighted = value;
                OnHighlightStateChanged(value);
                HighlightChanged?.Invoke(this);
            }
        }
    }

    public InteractMode InteractMode => interactMode;
    public KeyCode InteractionKey => interactionKey;
    public bool IsActive => isActive;
    public int CurrentStateIndex => currentStateIndex;

    protected virtual void Awake()
    {
        UpdatePromptVisibility();
    }

    protected virtual void OnEnable()
    {
        UpdatePromptVisibility();
    }

    protected virtual void OnDisable()
    {
        if (buttonPrompt != null)
        {
            buttonPrompt.SetActive(false);
        }
    }

    public virtual void OnClicked()
    {
        TryInteract(null, null);
    }

    public virtual bool TryInteract(PlayerInventory inventory, GameObject player)
    {
        if (!CanInteract() || !CanAcceptInteraction())
        {
            return false;
        }

        HandleInteraction(inventory, player);
        return true;
    }

    protected virtual void HandleInteraction(PlayerInventory inventory, GameObject player)
    {
        Clicked?.Invoke(this);
    }

    protected virtual void OnHighlightStateChanged(bool isNowHighlighted)
    {
        UpdatePromptVisibility();
    }

    public virtual bool CanInteract()
    {
        return true;
    }

    public virtual bool IsInHighlightRange(Vector3 playerPosition)
    {
        if (maxHighlightDistance <= 0f)
        {
            return true;
        }

        return Vector2.Distance(transform.position, playerPosition) <= maxHighlightDistance;
    }

    public virtual void SetActive(bool active)
    {
        isActive = active;
    }

    public virtual void SetState(int stateIndex)
    {
        currentStateIndex = stateIndex;
        ApplyState(stateIndex);
    }

    public virtual void ResetState()
    {
        SetState(0);
    }

    protected virtual void ApplyState(int stateIndex)
    {
    }

    public void SetHighlighted(bool highlighted)
    {
        IsHighlighted = highlighted;
    }

    protected bool CanAcceptInteraction()
    {
        if ((interactMode == InteractMode.Click || interactMode == InteractMode.ButtonPress || interactMode == InteractMode.ClickAndButton) && !isHighlighted)
        {
            return false;
        }

        return true;
    }

    private void UpdatePromptVisibility()
    {
        if (buttonPrompt == null)
        {
            return;
        }

        bool shouldShowPrompt = !showPromptOnlyForButtonMode || interactMode == InteractMode.ButtonPress || interactMode == InteractMode.ClickAndButton;
        shouldShowPrompt = shouldShowPrompt && isHighlighted;
        buttonPrompt.SetActive(shouldShowPrompt);
    }
}

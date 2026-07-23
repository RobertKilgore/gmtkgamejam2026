using UnityEngine;

[RequireComponent(typeof(Interactable))]
public sealed class InteractableHighlightVisualController : MonoBehaviour
{
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlineWidth = 0.1f;

    private Interactable interactable;
    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Material outlineMaterial;

    private void Awake()
    {
        interactable = GetComponent<Interactable>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
            Debug.Log($"[Highlight] Found SpriteRenderer on {gameObject.name}");
            CreateOutlineMaterial();
        }
        else
        {
            Debug.LogWarning($"[Highlight] No SpriteRenderer found on {gameObject.name}!");
        }
    }

    private void CreateOutlineMaterial()
    {
        Shader outlineShader = Shader.Find("Magic Forest/SpriteOutline");
        if (outlineShader == null)
        {
            Debug.LogError("[Highlight] SpriteOutline shader not found! Make sure it's in Assets/Shaders/SpriteOutline.shader");
            return;
        }

        outlineMaterial = new Material(outlineShader);
        outlineMaterial.SetColor("_OutlineColor", outlineColor);
        outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
        Debug.Log($"[Highlight] Created outline material on {gameObject.name}");
    }

    private void OnEnable()
    {
        if (interactable != null)
        {
            interactable.HighlightChanged += OnHighlightChanged;
            Debug.Log($"[Highlight] Subscribed to HighlightChanged event on {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"[Highlight] Interactable component not found on {gameObject.name}!");
        }
    }

    private void OnDisable()
    {
        if (interactable != null)
        {
            interactable.HighlightChanged -= OnHighlightChanged;
            Debug.Log($"[Highlight] Unsubscribed from HighlightChanged event on {gameObject.name}");
        }

        ResetVisuals();
    }

    private void OnHighlightChanged(Interactable source)
    {
        if (source.IsHighlighted)
        {
            Debug.Log($"[Highlight] {gameObject.name} highlighted.");
            ApplyHighlight();
        }
        else
        {
            Debug.Log($"[Highlight] {gameObject.name} unhighlighted.");
            ResetVisuals();
        }
    }

    private void ApplyHighlight()
    {
        if (spriteRenderer == null || outlineMaterial == null)
        {
            Debug.LogWarning($"[Highlight] Cannot apply highlight - missing SpriteRenderer or material on {gameObject.name}");
            return;
        }

        spriteRenderer.material = outlineMaterial;
        Debug.Log($"[Highlight] Applied outline to {gameObject.name}");
    }

    private void ResetVisuals()
    {
        if (spriteRenderer == null || originalMaterial == null)
        {
            return;
        }

        spriteRenderer.material = originalMaterial;
        Debug.Log($"[Highlight] Reset outline on {gameObject.name}");
    }
}

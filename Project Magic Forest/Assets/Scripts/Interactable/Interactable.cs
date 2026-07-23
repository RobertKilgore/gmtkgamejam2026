using System;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    private bool isHighlighted;

    public event Action<Interactable> HighlightChanged;
    public event Action<Interactable> Clicked;

    public bool IsHighlighted
    {
        get => isHighlighted;
        set
        {
            if (isHighlighted != value)
            {
                isHighlighted = value;
                OnHighlightStateChanged(value);
                HighlightChanged?.Invoke(this);
            }
        }
    }

    public virtual void OnClicked()
    {
        Clicked?.Invoke(this);
    }

    protected virtual void OnHighlightStateChanged(bool isNowHighlighted)
    {
    }
}

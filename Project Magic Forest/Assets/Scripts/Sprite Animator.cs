using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteLoopAnimator : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private List<Sprite> sprites = new List<Sprite>();
    [SerializeField, Range(1f, 60f)] private float framesPerSecond = 10f;

    private SpriteRenderer spriteRenderer;
    private int currentIndex;
    private float timer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && sprites.Count > 0)
        {
            spriteRenderer.sprite = sprites[0];
        }
    }

    private void Update()
    {
        if (spriteRenderer == null || sprites.Count <= 1)
        {
            return;
        }

        float frameInterval = 1f / framesPerSecond;
        timer += Time.deltaTime;

        if (timer >= frameInterval)
        {
            timer = 0f;
            currentIndex = (currentIndex + 1) % sprites.Count;
            spriteRenderer.sprite = sprites[currentIndex];
        }
    }
}
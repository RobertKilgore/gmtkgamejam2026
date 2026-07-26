using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager for handling end-of-game and death transitions.
/// Attach an Animator (on this GameObject) to play a transition animation.
/// Set `persistAcrossScenes = true` to keep this object alive across scene loads
/// so the animation can continue playing while loading the next scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Scene")]
    public string mainMenuSceneName = "Start Menu";

    [Header("Transition Animation")]
    public Animator transitionAnimator;
    [Tooltip("Animator trigger name to play for an end-of-game transition.")]
    public string endGameTrigger = "EndGame";
    [Tooltip("Animator trigger name to play for a death transition.")]
    public string deathTrigger = "Death";
    [Tooltip("Fallback duration to wait for the animation (seconds, unscaled). Set to actual clip length.")]
    public float transitionDuration = 2f;

    [Header("Behavior")]
    public bool persistAcrossScenes = true;

    private static GameManager instance;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public static GameManager Instance => instance;

    /// <summary>
    /// End the game after the given number of seconds. Optionally play the end animation.
    /// </summary>
    public void EndGameAfterSeconds(float seconds, bool playAnimation = true)
    {
        StartCoroutine(EndGameAfterSecondsRoutine(seconds, playAnimation));
    }

    private IEnumerator EndGameAfterSecondsRoutine(float seconds, bool playAnimation)
    {
        yield return new WaitForSeconds(seconds);
        yield return StartCoroutine(PlayTransitionThenLoadMainMenu(playAnimation));
    }

    /// <summary>
    /// Immediately start the end-of-game flow (play animation then load main menu).
    /// </summary>
    public void EndGameNow(bool playAnimation = true)
    {
        if (!isTransitioning)
            StartCoroutine(PlayTransitionThenLoadMainMenu(playAnimation));
    }

    /// <summary>
    /// Called when the player dies; optionally plays the death animation then returns to main menu.
    /// </summary>
    public void OnPlayerDeath(bool playAnimation = true)
    {
        if (!isTransitioning)
            StartCoroutine(PlayDeathThenLoadMainMenu(playAnimation));
    }

    private IEnumerator PlayTransitionThenLoadMainMenu(bool playAnimation)
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        if (playAnimation && transitionAnimator != null && !string.IsNullOrEmpty(endGameTrigger))
        {
            transitionAnimator.SetTrigger(endGameTrigger);
            yield return new WaitForSecondsRealtime(transitionDuration);
        }

        SceneManager.LoadScene(mainMenuSceneName);
        isTransitioning = false;
    }

    private IEnumerator PlayDeathThenLoadMainMenu(bool playAnimation)
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        if (playAnimation && transitionAnimator != null && !string.IsNullOrEmpty(deathTrigger))
        {
            transitionAnimator.SetTrigger(deathTrigger);
            yield return new WaitForSecondsRealtime(transitionDuration);
        }

        SceneManager.LoadScene(mainMenuSceneName);
        isTransitioning = false;
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "AudioClips", menuName = "Audio Clips/Audio Clips Database")]
public class AudioClips : ScriptableObject
{
    [Header("Footsteps")]
    public AudioClip walkOnWood;
    public AudioClip walkOnSnow;

    [Header("UI")]
    public AudioClip uiClick;
    public AudioClip uiHover;
    public AudioClip pauseMenuSound;

    [Header("Items & Interaction")]
    public AudioClip getItemSound;
    public AudioClip grabFruit;
    public AudioClip axeHitTree;
    public AudioClip chestOpen;

    [Header("Environmental & Magic")]
    public AudioClip magicStone;
    public AudioClip fire;
    public AudioClip fairyCircleTeleport;
    public AudioClip fountainSound;
    public AudioClip willOWispSound;

    public AudioClip TeleportSound;

    [Header("Creatures")]
    public AudioClip dragonBreathing;
    public AudioClip dragonAngry;
}

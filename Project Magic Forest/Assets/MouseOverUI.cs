using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MouseOverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;

    public GameObject timeDisplay;
    [Header("Target")]
    [SerializeField] private PlayerTimers playerTimers;
     [SerializeField] private Timer timer;
     [Header("Formatting")]
    [SerializeField] private string format = "0";
    [SerializeField] private string timerKey = "Temperature";

    [Header("Text")]
    [SerializeField] private TMP_Text textComponent;
    [SerializeField] private float threshold = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
     private void Awake()
    {
        if (playerTimers == null)
        {
            playerTimers = FindFirstObjectByType<PlayerTimers>();
            Debug.Log("PlayerTimers not found.");
        }

        if (textComponent == null)
        {
            textComponent = GetComponentInChildren<TMP_Text>();
            Debug.Log("Text component not found.");
        }
    }
    void Start()
    {
         timer = playerTimers.FindTimer(timerKey);
        timeDisplay.SetActive(false);
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        int secondsLeft = Mathf.CeilToInt(timer.TimeRemaining);
        textComponent.text = secondsLeft.ToString(format);

        if (threshold > 0f)
        {
            float t = Mathf.Clamp01(secondsLeft / threshold);
        }
    }
     public void OnPointerEnter(PointerEventData eventData)
    {
        timeDisplay.SetActive(true);
    }

    // Triggered when mouse leaves the UI element
    public void OnPointerExit(PointerEventData eventData)
    {
        timeDisplay.SetActive(false);
    }
}

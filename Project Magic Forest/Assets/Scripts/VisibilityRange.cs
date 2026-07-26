using UnityEngine;
using UnityEngine.Rendering.Universal;

public class VisibilityRange : MonoBehaviour


{
    [Header("Target")]
    [SerializeField] private PlayerTimers playerTimers;
    [SerializeField] private string timerKey = "";
    public GameObject player;
    public float visibilityRange;
    public float visibilityReductionScale = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
      private void Awake()
    {
        if (playerTimers == null)
        {
            playerTimers = FindFirstObjectByType<PlayerTimers>();
        }
    }
    
    
    void Start()
    {
        
        
      
    }

    // Update is called once per frame
    void Update()
    {

        Timer timer = playerTimers.FindTimer(timerKey);
        Light2D light = GetComponent<Light2D>();
        visibilityRange = timer.TimeRemaining / visibilityReductionScale;
        light.pointLightOuterRadius = visibilityRange;
       
    
    }
}

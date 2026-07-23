using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TempTimer : InheritableTimer 



{
    public Slider timerSlider;
    [SerializeField] float defaultTempTime;
    [SerializeField] TextMeshProUGUI timerText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timerSlider.maxValue = defaultTempTime;
        SetTimer(defaultTempTime);
        OnTimerStart();
    }
    
    override public void OnUpdate()
    {
        timerSlider.value = timeRemaining;
        DisplayTime(timeRemaining);
    }
    // Formats and prints the float value into an MM:SS configuration 
    private void DisplayTime(float timeToDisplay)
    {
        // Prevents negative numbers from rendering on screen
        if (timeToDisplay < 0) timeToDisplay = 0; 

        // Mathematical conversion from total seconds to minutes and remainder seconds
        int minutes = Mathf.FloorToInt(timeToDisplay / 60); 
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);

        // String formatting applying leading zeros (00:00) to optimize string memory footprint
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
}

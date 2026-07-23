using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;


public class TempTimer : InheritableTimer 



{
    public Slider timerSlider;
    [SerializeField] float defaultTempTime;
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
    }

    
}

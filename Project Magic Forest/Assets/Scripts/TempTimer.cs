using UnityEditor.Rendering;
using UnityEngine;


public class TempTimer : InheritableTimer 



{
    [SerializeField] float defaultTempTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        setTimer(defaultTempTime);
        onTimerStart();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

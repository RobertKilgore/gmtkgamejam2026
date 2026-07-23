using UnityEngine;

public abstract class InheritableTimer : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private float timeRemaining;
    private bool timerIsRunning = false;
   

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (timerIsRunning == true)
        {
            if (timeRemaining > 0)
            {

                timeRemaining -= Time.deltaTime;

            }
            else
            {
                Debug.Log("Time End");
                //Incase of negative
                timeRemaining = 0;
                timerIsRunning = false;
                onTimerEnd();
            }
        }
    }

    public virtual void onTimerEnd()
    {
        timerEndEffect();
    }

    public virtual void timerEndEffect()
    {
        
    }

    
    public virtual void onTimerStart()
    {
        timerIsRunning = true;
        timerStartEffect();
    }

    public virtual void timerStartEffect()
    {
        
    }
    public virtual void setTimer(float newTime)
    {
        timeRemaining = newTime;
    }
}

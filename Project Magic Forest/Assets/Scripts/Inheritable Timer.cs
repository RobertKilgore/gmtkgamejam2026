using UnityEngine;

public abstract class InheritableTimer : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] public float timeRemaining;
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
                OnTimerEnd();
            }
        }
        OnUpdate();
    }

    public virtual void OnTimerEnd()
    {
        TimerEndEffect();
    }

    public virtual void TimerEndEffect()
    {
        
    }
    public virtual void OnUpdate()
    {
        
    }
    
    public virtual void OnTimerStart()
    {
        timerIsRunning = true;
        TimerStartEffect();
    }

    public virtual void TimerStartEffect()
    {
        
    }
    public virtual void SetTimer(float newTime)
    {
        timeRemaining = newTime;
    }
}

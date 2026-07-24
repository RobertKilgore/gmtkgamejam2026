using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

public class TransitionManager : MonoBehaviour
{
    public GameObject exitBlocker;
    public float checker;
    public GameObject cabinCamera;
    public GameObject playerCamera;
    public bool transitionRunning;
    public bool transitionReady;
    BoxCollider2D _exitBlocker;
    public GameObject SnowIn; 
    public GameObject SnowOut; 


    
    
    void Start()
    {
        _exitBlocker = exitBlocker.GetComponent<BoxCollider2D>();
        _exitBlocker.enabled = false;
        checker = 1; 

    }
    // Update is called once per frame
    void Update()
    {
        if (cabinCamera.activeSelf == true && transitionRunning == false && transitionReady == true)
        {
            checker = 2;

            StartTransition();

        }
        if (playerCamera.activeSelf == true)
        {
            transitionReady = true;
        }
    }
    void StartTransition()
    {
        _exitBlocker.enabled = true;
        checker = 3;
      transitionRunning = true;
      transitionReady = false;
      //AnimationStart
    SnowIn.SetActive(true);
    Invoke(nameof(WorldRandomizer), 2f);
    }
    void WorldRandomizer()
    {
        Debug.Log("Works");
        SnowOut.SetActive(true);
        SnowIn.SetActive(false);
        Invoke(nameof(TransitionEnd), 2f);
    }

      void TransitionEnd()
    {
        transitionRunning = false;
        _exitBlocker.enabled = false;
         SnowOut.SetActive(false);


    }


}
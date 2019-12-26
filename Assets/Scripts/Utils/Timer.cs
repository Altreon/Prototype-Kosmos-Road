using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// définit un timer très simple
public class Timer
{
    private float cooldown; // cooldown du timer
    private float time; // temp courant

    public Timer(float cooldown)
    {
        Cooldown = cooldown;
        time = 0;
    }

    public Timer() { }

    // fait avancer le timer de deltatime secondes (renvoi si le timer est fini
    public bool Tick(float deltaTime)
    {
        if(!IsFinished())
        {
            time -= deltaTime;
            if (time < 0)
            {
                time = 0;
            }
        }
        return IsFinished();
    }

    public void Start(float cooldown)
    {
        Cooldown = cooldown;
        StartTimer();
    }

    public void Start()
    {
        if(Cooldown != 0)
        {
            StartTimer();
        }
        else
        {
            throw new System.ArgumentException("Cooldown need to be initialized before starting", "original");
        }
    }

    // arrête le timer
    public void Stop()
    {
        time = 0;
    }

    private void StartTimer()
    {
        time = Cooldown;
    }

    public bool IsFinished()
    {
        return (time == 0);
    }

    // getter /setter cooldown
    public float Cooldown
    {
        get
        {
            return cooldown;
        }
        set
        {
            if ((value > 0))
            {
                cooldown = value;
            }
            else
            {
                cooldown = 0;
            }
        }
    }
}

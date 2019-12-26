using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : Dictionary<string, Timer>
{
    // fait avancer tous les timers
    public void UpdateTimers(float deltaTime)
    {
        foreach (KeyValuePair<string, Timer> timer in this)
        {
            timer.Value.Tick(deltaTime);
        }
    }
    public void ResetTimers()
    {
        foreach (KeyValuePair<string, Timer> timer in this)
        {
            timer.Value.Stop();
        }
    }
}
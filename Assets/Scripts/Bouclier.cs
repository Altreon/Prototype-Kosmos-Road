using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bouclier : MonoBehaviour
{
    public GameObject bouclier;

    private Timer timerDuration;

    // Start is called before the first frame update
    void Start()
    {
        bouclier.SetActive(false);
        timerDuration = new Timer();
    }

    // Update is called once per frame
    void Update()
    {
        if (!timerDuration.IsFinished())
        {
            if (timerDuration.Tick(Time.deltaTime)) // si le timer finit
            {
                bouclier.SetActive(false);
            }
        }
    }
    
    // fonction à appellée pour activer le bouclier (pour une durée donnée)
    public void Activate(float duration)
    {
        timerDuration.Cooldown = duration;
        timerDuration.Start();
        bouclier.SetActive(true);
    }
}

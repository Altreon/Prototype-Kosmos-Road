using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChronometerHUD : MonoBehaviour
{
    private float time;
    private Text textComponentValue;

    // Start is called before the first frame update
    void Start()
    {
        textComponentValue = transform.GetComponent<Text>();
        time = 0;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        textComponentValue.text = GetFormattedChronometer();
    }

    string GetFormattedChronometer()
    {
        int hour = (int) time / 3600;
        float modulo = time % 3600;
        int minutes = (int) modulo / 60;
        modulo %= 60;
        int secondes = (int) modulo;
        modulo %= 1;
        int hundredth = (int) (modulo*100);

        return Format(hour) + ":" + Format(minutes) + ":" + Format(secondes) + ":" + Format(hundredth);
    }

    string Format(int value)
    {
        return value.ToString("00");
    }

    public void Restart()
    {
        time = 0;
    }
}

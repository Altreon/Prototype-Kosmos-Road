using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DistanceHUD : MonoBehaviour
{
    public UnitSystem unit;

    public Player player;
    private Text textComponentValue;
    private Text textComponentUnit;
    private double distance;

    // Start is called before the first frame update
    void Start()
    {
        textComponentValue = transform.Find("DistanceValue").GetComponent<Text>();
        textComponentUnit = transform.Find("DistanceUnit").GetComponent<Text>();
        distance = 0;
    }

    // Update is called once per frame
    void Update()
    {
        distance += GetDeltaDistance();
        if (distance != -1)
        {
            textComponentValue.text = distance.ToString("0000.00");
        }
        else
        {
            textComponentValue.text = "Err";
        }
        textComponentUnit.text = getUnit();
    }

    double GetDeltaDistance()
    {
        switch (unit)
        {
            case UnitSystem.meter:
                return (player.GetHovercraft().GetVitesse()/ 1000) * Time.deltaTime;
            case UnitSystem.mile:
                return ConvertSpeedToMilesPerSecond(player.GetHovercraft().GetVitesse()) * Time.deltaTime;
            default:
                return -1;
        }
    }

    string getUnit()
    {
        switch (unit)
        {
            case UnitSystem.meter:
                return "Km";
            case UnitSystem.mile:
                return "Miles";
            default:
                return "Err";
        }
    }

    double ConvertSpeedToMilesPerSecond(double speedMeterPerSecond)
    {
        return speedMeterPerSecond / 1609.344;
    }

    public void Restart()
    {
        distance = 0;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UnitSystem
{
    meter,
    mile
};

public class SpeedHUD : MonoBehaviour
{
    public Player player;
    public UnitSystem unit;

    private Text textComponentValue;
    private Text textComponentUnit;
    private int speed;

    // Start is called before the first frame update
    void Start()
    {
        textComponentValue = transform.Find("SpeedValue").GetComponent<Text>();
        textComponentUnit = transform.Find("SpeedUnit").GetComponent<Text>();
        speed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        speed = GetSpeed();
        if (speed != -1)
        {
            textComponentValue.text = speed.ToString("000");
        }
        else
        {
            textComponentValue.text = "Err";
        }
        textComponentUnit.text = GetUnit();

        SetColor(); // change la couleur en fonction du seuil de vitesse de mort du joueur
    }

    int GetSpeed()
    {
        switch (unit)
        {
            case UnitSystem.meter:
                return Utils.ConvertSpeedToKmph(player.GetHovercraft().GetVitesse());
            case UnitSystem.mile:
                return Utils.ConvertSpeedToMph(player.GetHovercraft().GetVitesse());
            default:
                return -1;
        }
    }

    string GetUnit()
    {
        switch (unit)
        {
            case UnitSystem.meter:
                return "Km/h";
            case UnitSystem.mile:
                return "Mph";
            default:
                return "Err";
        }
    }

    // change la couleur de la vitesse en fonction de si le joueur va assez vite pour ne pas mourrir (vert) ou non (rouge)
    private void SetColor()
    {
        if (speed > player.GetSeuilVitesseMarteau()) // si il va assez vite pour le marteau, en bleu
        {
            textComponentValue.color = Color.blue;
            textComponentUnit.color = Color.blue;
        }
        else if (speed > player.GetSeuilVitesseMort()) // si il va assez vite, en vert
        {
            textComponentValue.color = Color.green;
            textComponentUnit.color = Color.green;
        }
        else // sinon en rouge
        {
            textComponentValue.color = Color.red;
            textComponentUnit.color = Color.red;
        }
    }
}
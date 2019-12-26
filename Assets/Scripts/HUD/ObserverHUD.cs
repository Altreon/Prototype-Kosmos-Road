using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// classe d'observateur de l'HUD, on peut lui notifier tout ce que l'on veut
public class ObserverHUD : Observer
{
    // HUD DES COOLDOWN DE CAPACITES
    private CoolDownHUD jumpHUD;
    //private CoolDownHUD boostHUD;
    private CoolDownHUD hammerHUD;
    //private CoolDownHUD emptyHUD;

   // private DistanceHUD distanceHUD;
    private ChronometerHUD chronoHUD;

    private LifeManagerHUD lifeManagerHUD;

    // Start is called before the first frame update
    void Start()
    {
        jumpHUD = transform.Find("CapacitiesLayout").transform.Find("CapacityJump").GetComponent<CoolDownHUD>();
        //boostHUD = transform.Find("CapacitiesLayout").transform.Find("CapacityBoost").GetComponent<CoolDownHUD>();
        hammerHUD = transform.Find("CapacitiesLayout").transform.Find("CapacityHammer").GetComponent<CoolDownHUD>();
        //emptyHUD = transform.Find("CapacitiesLayout").transform.Find("CapacityEmpty").GetComponent<CoolDownHUD>();

        //distanceHUD = transform.Find("Distance").GetComponent<DistanceHUD>();
        chronoHUD = transform.Find("Chronometer").GetComponent<ChronometerHUD>();

        lifeManagerHUD = transform.Find("Life").GetComponent<LifeManagerHUD>();
    }

    override
    protected void HandleInfos(BasicMessage message)
    {
        switch (message.type)
        {
            case type_message.cooldown_start_jump:
                jumpHUD.Activate(((Message<float>)message).infos);
                break;

            case type_message.cooldown_start_boost:
                //boostHUD.Activate(((Message<float>)message).infos);
                break;

            case type_message.cooldown_start_hammer:
                hammerHUD.Activate(((Message<float>)message).infos);
                break;
                
            case type_message.cooldown_start_other:
                //emptyHUD.Activate(((Message<float>)message).infos);
                break;

            case type_message.restart_hud_timers:
                //distanceHUD.Restart();
                chronoHUD.Restart();
                jumpHUD.Restart();
                //boostHUD.Restart();
                hammerHUD.Restart();
                //emptyHUD.Restart();
                break;

            default:
                Debug.Log("\"" + message.type + "\" : Type message non reconnu !");
                break;
        }
    }

    public LifeManagerHUD GetLifeManagerHUD()
    {
        return lifeManagerHUD;
    }
}

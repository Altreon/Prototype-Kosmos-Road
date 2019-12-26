using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoolDownHUD : MonoBehaviour
{
    private float coolDown; // cooldown en seconde de la capacité

    private Image cooldownImage;
    private bool isOnCoolDown;

    // Start is called before the first frame update
    void Start()
    {
        isOnCoolDown = false;
        cooldownImage = transform.Find("Cooldown").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isOnCoolDown)
        {
            cooldownImage.fillAmount -= 1/coolDown*Time.deltaTime;

            if (cooldownImage.fillAmount <= 0)
            {
                cooldownImage.fillAmount = 0;
                isOnCoolDown = false;
            }
        }
    }

    // active le cooldown avec un certain cooldown
    public void Activate(float cooldown)
    {
        cooldownImage.fillAmount = 1;
        isOnCoolDown = true;
        this.coolDown = cooldown;
    }

    public void Restart()
    {
        cooldownImage.fillAmount = 0;
        isOnCoolDown = false;
    }
}

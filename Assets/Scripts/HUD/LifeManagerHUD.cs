using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeManagerHUD : MonoBehaviour
{
    private GameObject coeur;

    // Start is called before the first frame update
    void Start()
    {
        coeur = transform.Find("PV").gameObject;
    }

    // change le nombre de coeur à l'écran
    public void SetNbCoeur(int nb)
    {
        transform.DetachChildren();

        for (int i = 0; i < nb; i++)
        {
            GameObject newHearth = Object.Instantiate(coeur, transform);
            newHearth.SetActive(true);
        }
    }
}

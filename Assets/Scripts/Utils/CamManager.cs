using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CamManager : Dictionary<string, Camera>
{
    private string currentMainCam = null;

    // met la caméra passée en paramètre devant
    public void SetFrontCam(string name)
    {
        if (!this.ContainsKey(name)) // la caméra doit exister dans le camManager
            return;

        // on désactive toutes les caméras et met à jour leur profondeur
        foreach (KeyValuePair<string, Camera> paire in this)
        {
            this[paire.Key].gameObject.GetComponent<Camera>().enabled = (paire.Key == name);
            this[paire.Key].gameObject.GetComponent<AudioListener>().enabled = (paire.Key == name);
        }
        currentMainCam = name;
    }

    // retourne le nom de la caméra courante
    public string GetFrontCamName()
    {
        return currentMainCam;
    }

    // retourne la caméra courante
    public Camera GetFrontCam()
    {
        return this[currentMainCam];
    }
}